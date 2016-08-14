using Newtonsoft.Json;
using System.Linq;

namespace DataCloner.Universal.Serialization
{
    /// <summary>
    /// Helper class for serialization.
    /// </summary>
    public static class SerializationHelper
    {
        /// <summary>
        /// Deserializes the specified XML.
        /// </summary>
        /// <typeparam name="T">Target type.</typeparam>
        /// <param name="xml">The XML.</param>
        /// <returns>The deserialized object.</returns>
        public static T Deserialize<T>(string xml)
        {
            return JsonConvert.DeserializeObject<T>(xml);
        }

        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The string.</returns>
        public static string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }
    }
}
