using System;
using System.Buffers;
using FasterKv.Cache.Core.Serializers;

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
    /// <param name="nowTimestamp">Now</param>
    /// <returns>value has expired</returns>
    public bool HasExpired(long nowTimestamp) => nowTimestamp > ExpiryTime;

    /// <summary>
    /// Get FasterKvSerializerFlags
    /// </summary>
    /// <param name="nowTimestamp"></param>
    /// <returns></returns>
    internal FasterKvSerializerFlags GetFlags(long nowTimestamp)
    {
        var flags = FasterKvSerializerFlags.None;
        if (ExpiryTime is not null)
        {
            flags |= FasterKvSerializerFlags.HasExpiryTime;
        }

        // don't serializer expired value body
        if (Data is not null && HasExpired(nowTimestamp) == false)
        {
            flags |= FasterKvSerializerFlags.HasBody;
        }

        return flags;
    }
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
    /// <param name="nowTimestamp">Now</param>
    /// <returns>value has expired</returns>
    public bool HasExpired(long nowTimestamp) => nowTimestamp > ExpiryTime;

    /// <summary>
    /// Get FasterKvSerializerFlags
    /// </summary>
    /// <param name="nowTimestamp"></param>
    /// <returns></returns>
    internal FasterKvSerializerFlags GetFlags(long nowTimestamp)
    {
        var flags = FasterKvSerializerFlags.None;
        if (ExpiryTime is not null)
        {
            flags |= FasterKvSerializerFlags.HasExpiryTime;
        }

        // don't serializer expired value body
        if (Data is not null && HasExpired(nowTimestamp) == false)
        {
            flags |= FasterKvSerializerFlags.HasBody;
        }

        return flags;
    }

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