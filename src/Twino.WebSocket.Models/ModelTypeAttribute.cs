using System;

namespace Twino.WebSocket.Models
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ModelTypeAttribute : Attribute
    {
        public string TypeName { get; }

        public ModelTypeAttribute(string typeName)
        {
            TypeName = typeName;
        }
    }
}