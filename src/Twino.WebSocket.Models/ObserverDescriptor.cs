using System;
using Twino.WebSocket.Models.Internal;

namespace Twino.WebSocket.Models
{
    internal class ObserverDescriptor
    {
        public Type ModelType { get; set; }
        public Type ObserverType { get; set; }
        public ObserverExecuter Executer { get; set; }
    }
}