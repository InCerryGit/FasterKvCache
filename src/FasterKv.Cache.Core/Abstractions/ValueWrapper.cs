using System;
using System.Buffers;

namespace FasterKv.Cache.Core;

internal struct ValueWrapper<T>
{
    public ValueWrapper()
    {
        
    }

    public ValueWrapper(T? data, long? expiryTime = null)
    {
        ExpiryTime = expiryTime;
        Data = data;
    }
    
    /// <summary>
    /// Inner Data
    /// </summary>
    internal T? Data { get; set; }
    
    /// <summary>
    /// Expiry Time
    /// </summary>
    public long? ExpiryTime { get; set; }
    
    /// <summary>
    /// HasExpired
    /// </summary>
    /// <param name="now">Now</param>
    /// <returns>value has expired</returns>
    public bool HasExpired(DateTimeOffset now) => now.ToUnixTimeMilliseconds() > ExpiryTime;
}

internal sealed class ValueWrapper
{
    internal int DataByteLength = 0;
    internal object? Data;

    public ValueWrapper()
    {
        
    }

    public ValueWrapper(object? data, long? expiryTime = null)
    {
        ExpiryTime = expiryTime;
        Data = data;
    }

    /// <summary>
    /// Expiry Time
    /// </summary>
    public long? ExpiryTime { get; set; }
    
    /// <summary>
    /// DataBytes
    /// </summary>
    public byte[]? DataBytes { get; set; }
    
    /// <summary>
    /// HasExpired
    /// </summary>
    /// <param name="now">Now</param>
    /// <returns>value has expired</returns>
    public bool HasExpired(DateTimeOffset now) => now.ToUnixTimeMilliseconds() > ExpiryTime;

    /// <summary>
    /// Get TValue From Data or DataBytes
    /// </summary>
    /// <param name="serializer"></param>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    public TValue? Get<TValue>(IFasterKvCacheSerializer serializer)
    {
        if (DataBytes is not null)
        {
            Data = serializer.Deserialize<TValue>(DataBytes, DataByteLength);
            var bytes = DataBytes;
            DataBytes = null;
            DataByteLength = 0;
            ArrayPool<byte>.Shared.Return(bytes);
        }

        return Data is null ? default : (TValue)Data;
    }
}