using System;
using Horse.WebSocket.Models.Internal;

namespace Horse.WebSocket.Models
{
    internal class ObserverDescriptor
    {
        public Type ModelType { get; set; }
        public Type ObserverType { get; set; }
        public ObserverExecuter Executer { get; set; }
    }
}