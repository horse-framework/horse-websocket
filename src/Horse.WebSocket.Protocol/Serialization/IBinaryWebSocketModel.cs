using System.IO;

namespace Horse.WebSocket.Protocol.Serialization;

public interface IBinaryWebSocketModel
{
    void Serialize(BinaryWriter writer);

    void Deserialize(BinaryReader reader);
}