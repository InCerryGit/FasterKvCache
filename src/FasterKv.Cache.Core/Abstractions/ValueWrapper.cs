using System;
using MessagePack;

namespace FasterKv.Cache.Core;

public struct ValueWrapper<T>
{
    public ValueWrapper()
    {
        
    }

    public ValueWrapper(T? data, DateTimeOffset? expiryTime = null)
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
    public DateTimeOffset? ExpiryTime { get; set; }
    
    /// <summary>
    /// HasExpired
    /// </summary>
    /// <param name="now">Now</param>
    /// <returns>value has expired</returns>
    public bool HasExpired(DateTimeOffset now) => now > ExpiryTime;
}

[MessagePackObject]
public class ValueWrapper
{
    internal object? Data;

    public ValueWrapper()
    {
        
    }

    public ValueWrapper(object? data, DateTimeOffset? expiryTime = null)
    {
        ExpiryTime = expiryTime;
        Data = data;
    }

    /// <summary>
    /// Expiry Time
    /// </summary>
    [Key(0)]
    public DateTimeOffset? ExpiryTime { get; set; }
    
    /// <summary>
    /// DataBytes
    /// </summary>
    [Key(1)]
    public byte[]? DataBytes { get; set; }
    
    /// <summary>
    /// HasExpired
    /// </summary>
    /// <param name="now">Now</param>
    /// <returns>value has expired</returns>
    public bool HasExpired(DateTimeOffset now) => now > ExpiryTime;

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
            Data = serializer.Deserialize<TValue>(DataBytes);
            DataBytes = null;
        }

        return Data is null ? default : (TValue)Data;
    }
}