using System;

namespace Twino.WebSocket.Models
{
    /// <summary>
    /// Model type descriptor attribute for observer objects
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ModelTypeAttribute : Attribute
    {
        /// <summary>
        /// Type code of the model
        /// </summary>
        public string TypeCode { get; }

        /// <summary>
        /// Creates new model type attribute
        /// </summary>
        public ModelTypeAttribute(string typeCode)
        {
            TypeCode = typeCode;
        }
    }
}