using System.IO;

namespace FasterKv.Cache.Core;

public interface IFasterKvCacheSerializer
{
    string Name { get;}
    byte[] Serialize<TValue>(TValue data);
    TValue? Deserialize<TValue>(byte[] bytes);
    void Serialize<TValue>(Stream stream, TValue data);
    TValue? Deserialize<TValue>(byte[] bytes, int length);
}