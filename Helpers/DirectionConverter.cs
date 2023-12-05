using blitz_api.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace blitz_api.Helpers
{
    public class DirectionConverter : JsonConverter<Direction>
    {
        public override Direction ReadJson(JsonReader reader, Type objectType, Direction? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Deserialization is not supported.");
        }

        public override void WriteJson(JsonWriter writer, Direction? value, JsonSerializer serializer)
        {
            var jObject = new JObject();
            var stopsArray = new JArray();

            foreach (var stop in value!.Stops)
            {
                var stopObject = new JObject
                {
                    { "StopSequence", stop.StopSequence },
                    { "StopId", stop.StopId },
                    { "StopName", stop.StopName }
                };
                stopsArray.Add(stopObject);
            }

            jObject[value.Name] = stopsArray;
            jObject.WriteTo(writer);
        }
    }

    public class DirectionContractResolver : DefaultContractResolver
    {
        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (objectType == typeof(Direction))
            {
                return new DirectionConverter();
            }

            return base.ResolveContractConverter(objectType!)!;
        }
    }
}
