using System;

namespace Horse.WebSocket.Protocol;

internal class ObserverDescriptor
{
    public Type ModelType { get; set; }
    public Type ObserverType { get; set; }
    public ObserverExecuter Executer { get; set; }
}