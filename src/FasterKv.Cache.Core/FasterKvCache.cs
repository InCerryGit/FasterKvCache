using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FASTER.core;
using FasterKv.Cache.Core.Abstractions;
using FasterKv.Cache.Core.Configurations;
using FasterKv.Cache.Core.Serializers;
using Microsoft.Extensions.Logging;

namespace FasterKv.Cache.Core;

public sealed class FasterKvCache : IDisposable
{
    private readonly ILogger? _logger;
    private readonly ISystemClock _systemClock;

    private readonly string _name;
    private readonly FasterKvCacheOptions _options;
    private readonly FasterKV<string, ValueWrapper> _fasterKv;
    private readonly LogSettings? _logSettings;

    private readonly ConcurrentQueue<ClientSession<string, ValueWrapper, ValueWrapper,
            ValueWrapper, StoreContext<ValueWrapper>, StoreFunctions<string, ValueWrapper>>>
        _sessionPool;

    private readonly CancellationTokenSource? _scanExpiryCancellationToken;
    private readonly IFasterKvCacheSerializer _valueSerializer;

    public FasterKvCache(string name,
        ISystemClock systemClock,
        FasterKvCacheOptions? options,
        IEnumerable<IFasterKvCacheSerializer>? serializers,
        ILoggerFactory? loggerFactory)
    {
        _name = name;
        _systemClock = systemClock.ArgumentNotNull();
        _options = options.ArgumentNotNull();
        _logger = loggerFactory?.CreateLogger<FasterKvCache>();

        var serializerName = name.NotNullOrEmpty() ? name : options!.SerializerName;
        // ReSharper disable once PossibleMultipleEnumeration
        _valueSerializer = serializers.ArgumentNotNull()
                               .FirstOrDefault(s => s.Name == serializerName)
                           ?? throw new InvalidOperationException($"Not found {serializerName} serializer");

        if (options!.CustomStore is null)
        {
            var serializer = new SerializerSettings<string, ValueWrapper>
            {
                keySerializer = () => new StringSerializer(),
                valueSerializer = () => new FasterKvSerializer(_valueSerializer, _systemClock)
            };

            _logSettings = options.GetLogSettings(name);
            _fasterKv = new FasterKV<string, ValueWrapper>(
                options.IndexCount,
                _logSettings,
                checkpointSettings: new CheckpointSettings()
                {
                    CheckpointDir = options.LogPath + ".checkpoint"
                },
                serializerSettings: serializer,
                tryRecoverLatest: options.TryRecoverLatest
            );
        }
        else
        {
            _fasterKv = (FasterKV<string, ValueWrapper>) options.CustomStore;
        }

        _sessionPool =
            new ConcurrentQueue<ClientSession<string, ValueWrapper, ValueWrapper, ValueWrapper,
                StoreContext<ValueWrapper>, StoreFunctions<string, ValueWrapper>>>();

        if (_options.ExpiryKeyScanInterval > TimeSpan.Zero)
        {
            _scanExpiryCancellationToken = new CancellationTokenSource();
            Task.Run(ExpiryScanLoop);
        }
    }

    public TValue? Get<TValue>(string key)
    {
        key.ArgumentNotNullOrEmpty();

        using var scopeSession = GetSessionWrap();
        var context = new StoreContext<ValueWrapper>();
        var result = scopeSession.Session.Read(key, context);
        if (result.status.IsPending)
        {
            scopeSession.Session.CompletePending(true);
            context.FinalizeRead(out result.status, out result.output);
        }

        if (result.output is null)
        {
            return default;
        }

        if (result.output.HasExpired(_systemClock.NowUnixTimestamp()))
        {
            Delete(key);
            return default;
        }

        return result.output.Get<TValue>(_valueSerializer);
    }

    public TValue GetOrAdd<TValue>(string key, Func<string, TValue> factory)
    {
        key.ArgumentNotNullOrEmpty();
        factory.ArgumentNotNull();

        var result = Get<TValue>(key);
        if (result is not null)
            return result;
        
        result = factory(key);
        Set(key, result);
        return result;
    }
    
    public TValue GetOrAdd<TValue>(string key, Func<string, TValue> factory, TimeSpan expiryTime)
    {
        key.ArgumentNotNullOrEmpty();
        factory.ArgumentNotNull();
        expiryTime.ArgumentNotNegativeOrZero();

        var result = Get<TValue>(key);
        if (result is not null)
            return result;
        
        result = factory(key);
        Set(key, result, expiryTime);
        return result;
    }

    public void Delete(string key)
    {
        using var scopeSession = GetSessionWrap();
        scopeSession.Session.Delete(ref key);
    }

    public void Set<TValue>(string key, TValue? value)
    {
        key.ArgumentNotNullOrEmpty();

        using var sessionWrap = GetSessionWrap();
        SetInternal(sessionWrap, key, value);
    }

    public void Set<TValue>(string key, TValue value, TimeSpan expiryTime)
    {
        key.ArgumentNotNullOrEmpty();
        expiryTime.ArgumentNotNegativeOrZero();

        using var sessionWrap = GetSessionWrap();
        SetInternal(sessionWrap, key, value, expiryTime);
    }

    public async Task<TValue?> GetAsync<TValue>(string key, CancellationToken token = default)
    {
        key.ArgumentNotNullOrEmpty();

        using var scopeSession = GetSessionWrap();
        var result = (await scopeSession.Session.ReadAsync(ref key, token: token)).Complete();

        if (result.output is null)
        {
            return default;
        }

        if (result.output.HasExpired(_systemClock.NowUnixTimestamp()))
        {
            await DeleteAsync(key, token);
            return default;
        }

        return result.output.Get<TValue>(_valueSerializer);
    }
    
    public async Task<TValue> GetOrAddAsync<TValue>(string key, Func<string, Task<TValue>> factory, CancellationToken token = default)
    {
        factory.ArgumentNotNull();

        var result = await GetAsync<TValue>(key, token);
        if (result is not null)
            return result;
        
        result = await factory(key);
        await SetAsync(key, result, token);
        return result;
    }
    
    public async Task<TValue> GetOrAddAsync<TValue>(string key, Func<string, Task<TValue>> factory, TimeSpan expiryTime, CancellationToken token = default)
    {
        factory.ArgumentNotNull();

        var result = await GetAsync<TValue>(key, token);
        if (result is not null)
            return result;
        
        result = await factory(key);
        await SetAsync(key, result, expiryTime, token);
        return result;
    }
    
    public async Task DeleteAsync(string key, CancellationToken token = default)
    {
        key.ArgumentNotNull();

        using var scopeSession = GetSessionWrap();
        (await scopeSession.Session.DeleteAsync(ref key, token: token).ConfigureAwait(false)).Complete();
    }

    public async Task SetAsync<TValue>(string key, TValue? value, CancellationToken token = default)
    {
        key.ArgumentNotNullOrEmpty();

        using var sessionWrap = GetSessionWrap();
        await SetInternalAsync(sessionWrap, key, value, token);
    }

    public async Task SetAsync<TValue>(string key, TValue? value, TimeSpan expiryTime, CancellationToken token = default)
    {
        key.ArgumentNotNullOrEmpty();

        using var sessionWrap = GetSessionWrap();
        await SetInternalAsync(sessionWrap, key, value, token, expiryTime);
    }

    private async Task SetInternalAsync<TValue>(ClientSessionWrap sessionWrap, string key, TValue? value,
        CancellationToken cancellationToken, TimeSpan? expiryTime = null)
    {
        var wrapper = new ValueWrapper(value,
            expiryTime.HasValue
                ? _systemClock.Now()
                    .Add(expiryTime.Value)
                    .ToUnixTimeMilliseconds()
                : null);
        (await sessionWrap.Session.UpsertAsync(ref key, ref wrapper, token: cancellationToken)
            .ConfigureAwait(false)).Complete();
    }

    private void SetInternal<TValue>(ClientSessionWrap sessionWrap, string key, TValue? value,
        TimeSpan? expiryTime = null)
    {
        var wrapper = new ValueWrapper(value,
            expiryTime.HasValue
                ? _systemClock.Now()
                    .Add(expiryTime.Value)
                    .ToUnixTimeMilliseconds()
                : null);
        sessionWrap.Session.Upsert(ref key, ref wrapper);
    }

    /// <summary>
    /// Get ClientSession from pool
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ClientSessionWrap GetSessionWrap()
    {
        if (_sessionPool.TryDequeue(out var session) == false)
        {
            session = _fasterKv.For(new StoreFunctions<string, ValueWrapper>())
                .NewSession<StoreFunctions<string, ValueWrapper>>();
        }

        return new ClientSessionWrap(session, _sessionPool);
    }

    private async Task ExpiryScanLoop()
    {
        var cancelToken = _scanExpiryCancellationToken!.Token;
        while (cancelToken.IsCancellationRequested == false)
        {
            try
            {
                await Task.Delay(_options.ExpiryKeyScanInterval, cancelToken);
                using var sessionWrap = GetSessionWrap();
                using var iter = sessionWrap.Session.Iterate();
                while (iter.GetNext(out _) && cancelToken.IsCancellationRequested == false)
                {
                    var key = iter.GetKey();
                    var context = new StoreContext<ValueWrapper>();
                    var result = sessionWrap.Session.Read(key, context);
                    if (result.status.IsPending)
                    {
                        sessionWrap.Session.CompletePending(true);
                        context.FinalizeRead(out result.status, out result.output);
                    }

                    if (result.status.Found && result.output.HasExpired(_systemClock.NowUnixTimestamp()))
                    {
                        sessionWrap.Session.Delete(key);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning("Exception thrown in expiry scan loop:{Ex}", ex);
            }
        }
    }

    private void Dispose(bool _)
    {
        _scanExpiryCancellationToken?.Cancel();
        foreach (var session in _sessionPool)
        {
            session.Dispose();
        }

        if (_options.CustomStore != _fasterKv)
        {
            if (_options.DeleteFileOnClose == false)
            {
                _fasterKv.TakeFullCheckpointAsync(CheckpointType.FoldOver).AsTask().GetAwaiter().GetResult();
            }
            _fasterKv.Dispose();
        }
        
        _logSettings?.LogDevice.Dispose();
        _logSettings?.ObjectLogDevice.Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~FasterKvCache()
    {
        Dispose(false);
    }

    internal ValueWrapper GetWithOutExpiry(string key)
    {
        var context = new StoreContext<ValueWrapper>();
        using var scopeSession = GetSessionWrap();
        var result = scopeSession.Session.Read(key, context);
        if (result.status.IsPending)
        {
            scopeSession.Session.CompletePending(true);
            context.FinalizeRead(out result.status, out result.output);
        }

        return result.output;
    }
}