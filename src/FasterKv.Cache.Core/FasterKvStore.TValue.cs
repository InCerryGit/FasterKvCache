using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FASTER.core;
using FasterKv.Cache.Core.Abstractions;
using FasterKv.Cache.Core.Configurations;
using FasterKv.Cache.Core.Serializers;
using Microsoft.Extensions.Logging;

namespace FasterKv.Cache.Core;

public sealed class FasterKvCache<TValue> : IDisposable
{
    private readonly ILogger? _logger;
    private readonly ISystemClock _systemClock;

    private readonly string _name;
    private readonly FasterKvCacheOptions _options;
    private readonly FasterKV<string, ValueWrapper<TValue>> _fasterKv;
    private readonly LogSettings? _logSettings;

    private readonly ConcurrentQueue<ClientSession<string, ValueWrapper<TValue>, ValueWrapper<TValue>,
            ValueWrapper<TValue>, StoreContext<ValueWrapper<TValue>>, StoreFunctions<string, ValueWrapper<TValue>>>>
        _sessionPool;

    private readonly CancellationTokenSource? _scanExpiryCancellationToken;

    public FasterKvCache(string name,
        ISystemClock systemClock,
        FasterKvCacheOptions? options,
        IEnumerable<IFasterKvCacheSerializer>? serializers,
        ILoggerFactory? loggerFactory)
    {
        _name = name;
        _systemClock = systemClock.ArgumentNotNull(nameof(_systemClock));
        _options = options.ArgumentNotNull(nameof(options));
        _logger = loggerFactory?.CreateLogger<FasterKvCache<TValue>>();

        if (options!.CustomStore is null)
        {
            var serializerName = name.NotNullOrEmpty() ? name : options.SerializerName;
            // ReSharper disable once PossibleMultipleEnumeration
            var valueSerializer =
                serializers.ArgumentNotNull(nameof(serializers)).FirstOrDefault(s => s.Name == serializerName)
                ?? throw new InvalidOperationException($"Not found {serializerName} serializer");

            var serializer = new SerializerSettings<string, ValueWrapper<TValue>>
            {
                keySerializer = () => new StringSerializer(),
                valueSerializer = () => new FasterKvSerializer<TValue>(valueSerializer)
            };

            _logSettings = options.GetLogSettings(name);
            _fasterKv = new FasterKV<string, ValueWrapper<TValue>>(
                options.IndexCount,
                _logSettings,
                serializerSettings: serializer);
        }
        else
        {
            _fasterKv = (FasterKV<string, ValueWrapper<TValue>>) options.CustomStore;
        }

        _sessionPool =
            new ConcurrentQueue<ClientSession<string, ValueWrapper<TValue>, ValueWrapper<TValue>, ValueWrapper<TValue>,
                StoreContext<ValueWrapper<TValue>>, StoreFunctions<string, ValueWrapper<TValue>>>>();

        if (_options.ExpiryKeyScanInterval > TimeSpan.Zero)
        {
            _scanExpiryCancellationToken = new CancellationTokenSource();
            Task.Run(ExpiryScanLoop);
        }
    }

    public TValue? Get(string key)
    {
        key.ArgumentNotNullOrEmpty(nameof(key));

        using var scopeSession = GetSessionWrap();
        var context = new StoreContext<ValueWrapper<TValue>>();
        var result = scopeSession.Session.Read(key, context);
        if (result.status.IsPending)
        {
            scopeSession.Session.CompletePending(true);
            context.FinalizeRead(out result.status, out result.output);
        }

        if (result.output.HasExpired(_systemClock.Now()))
        {
            Delete(key);
            return default;
        }

        return result.output.Data;
    }
    
    public void Delete(string key)
    {
        key.ArgumentNotNull(nameof(key));

        using var scopeSession = GetSessionWrap();
        scopeSession.Session.Delete(ref key);
    }



    public void Set(string key, TValue? value)
    {
        key.ArgumentNotNullOrEmpty(nameof(key));

        using var sessionWrap = GetSessionWrap();
        SetInternal(sessionWrap, key, value);
    }



    public void Set(string key, TValue value, TimeSpan expiryTime)
    {
        key.ArgumentNotNullOrEmpty(nameof(key));
        expiryTime.ArgumentNotNegativeOrZero(nameof(expiryTime));

        using var sessionWrap = GetSessionWrap();
        SetInternal(sessionWrap, key, value, expiryTime);
    }

    public async Task<TValue?> GetAsync(string key, CancellationToken token = default)
    {
        key.ArgumentNotNullOrEmpty(nameof(key));

        using var scopeSession = GetSessionWrap();
        var result = (await scopeSession.Session.ReadAsync(ref key, token: token)).Complete();

        if (result.output.HasExpired(_systemClock.Now()))
        {
            await DeleteAsync(key, token);
            return default;
        }

        return result.output.Data;
    }

    public async Task DeleteAsync(string key, CancellationToken token = default)
    {
        key.ArgumentNotNull(nameof(key));

        using var scopeSession = GetSessionWrap();
        (await scopeSession.Session.DeleteAsync(ref key, token: token).ConfigureAwait(false)).Complete();
    }
    
    public async Task SetAsync(string key, TValue? value, CancellationToken token = default)
    {
        key.ArgumentNotNullOrEmpty(nameof(key));

        using var sessionWrap = GetSessionWrap();
        await SetInternalAsync(sessionWrap, key, value, token);
    }
    
    public async Task SetAsync(string key, TValue? value, TimeSpan expiryTime, CancellationToken token = default)
    {
        key.ArgumentNotNullOrEmpty(nameof(key));

        using var sessionWrap = GetSessionWrap();
        await SetInternalAsync(sessionWrap, key, value, token, expiryTime);
    }
    

    private void SetInternal(ClientSessionWrap<TValue> sessionWrap, string key, TValue? value,
        TimeSpan? expiryTime = null)
    {
        var wrapper = new ValueWrapper<TValue>(value,
            expiryTime.HasValue ? _systemClock.Now().Add(expiryTime.Value).ToUnixTimeMilliseconds() : null);
        sessionWrap.Session.Upsert(ref key, ref wrapper);
    }

    private async Task SetInternalAsync(ClientSessionWrap<TValue> sessionWrap, string key, TValue? value,
        CancellationToken cancellationToken, TimeSpan? expiryTime = null)
    {
        var wrapper = new ValueWrapper<TValue>(value,
            expiryTime.HasValue ? _systemClock.Now().Add(expiryTime.Value).ToUnixTimeMilliseconds() : null);
        (await sessionWrap.Session.UpsertAsync(ref key, ref wrapper, token: cancellationToken)
                .ConfigureAwait(false)).Complete();
    }

    /// <summary>
    /// Get ClientSession from pool
    /// </summary>
    /// <returns></returns>
    private ClientSessionWrap<TValue> GetSessionWrap()
    {
        if (_sessionPool.TryDequeue(out var session) == false)
        {
            session = _fasterKv.For(new StoreFunctions<string, ValueWrapper<TValue>>())
                .NewSession<StoreFunctions<string, ValueWrapper<TValue>>>();
        }

        return new ClientSessionWrap<TValue>(session, _sessionPool);
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
                    var context = new StoreContext<ValueWrapper<TValue>>();
                    var result = sessionWrap.Session.Read(key, context);
                    if (result.status.IsPending)
                    {
                        sessionWrap.Session.CompletePending(true);
                        context.FinalizeRead(out result.status, out result.output);
                    }

                    if (result.status.Found && result.output.HasExpired(_systemClock.Now()))
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
            _fasterKv.Dispose();
        }

        _logSettings?.LogDevice.Dispose();
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

    internal ValueWrapper<TValue> GetWithOutExpiry(string key)
    {
        var context = new StoreContext<ValueWrapper<TValue>>();
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