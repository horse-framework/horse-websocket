using Horse.WebSocket.Protocol;
using Horse.WebSocket.Protocol.Serialization;

namespace Sample.Binary.Server;

[TextMessageType("Test")]
[BinaryMessageType(123)]
public class TestModel : IBinaryWebSocketModel
{
    public int Item1 { get; set; }
    public bool Item2 { get; set; }
    public string Item3 { get; set; }
    public long Item4 { get; set; }

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(Item1);
        writer.Write(Item2);
        writer.Write(Item3);
        writer.Write(Item4);
    }

    public void Deserialize(BinaryReader reader)
    {
        Item1 = reader.ReadInt32();
        Item2 = reader.ReadBoolean();
        Item3 = reader.ReadString();
        Item4 = reader.ReadInt64();
    }
}