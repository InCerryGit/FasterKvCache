namespace FasterKv.Cache.Core;

public interface IFasterKvCacheSerializer
{
    string Name { get;}
    byte[] Serialize<TValue>(TValue data);
    TValue? Deserialize<TValue>(byte[] serializerData);
}