using Newtonsoft.Json;

namespace Horse.SerializableModel.Serialization
{
    /// <summary>
    /// Default model writer for Horse libraries
    /// </summary>
    public class HorseModelWriter : IModelWriter
    {
        private static HorseModelWriter _default;

        /// <summary>
        /// Default, singleton HorseModelWriter class
        /// </summary>
        public static HorseModelWriter Default
        {
            get
            {
                if (_default == null)
                    _default = new HorseModelWriter();

                return _default;
            }
        }

        /// <summary>
        /// Creates serialized string message from T model
        /// </summary>
        public string Serialize(ISerializableModel model)
        {
            string body;

            if (model is IPerformanceCriticalModel critical)
            {
                LightJsonWriter writer = new LightJsonWriter();
                body = writer.Serialize(critical);
            }
            else
                body = JsonConvert.SerializeObject(model);

            return $"[{model.Type},{body}]";
        }
    }
}