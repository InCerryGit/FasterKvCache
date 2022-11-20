using FASTER.core;

namespace FasterKv.Cache.Core;

public class StringSerializer : BinaryObjectSerializer<string>
{
    public override void Deserialize(out string obj)
    {
        obj = reader.ReadString();
    }

    public override void Serialize(ref string obj)
    {
        writer.Write(obj);
    }
}