using System.IO;

namespace FasterKv.Cache.Core;

public interface IFasterKvCacheSerializer
{
    string Name { get;}
    void Serialize<TValue>(Stream stream, TValue data);
    TValue? Deserialize<TValue>(byte[] bytes, int length);
}