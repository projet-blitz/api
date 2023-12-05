using blitz_api.Helpers;
using System.Text.Json.Serialization;

namespace blitz_api.Models
{
    [JsonConverter(typeof(DirectionConverter))]
    public class Stop(string stopId, string stopSequence)
    {
        public string StopSequence { get; set; } = stopSequence;
        public string StopId { get; set; } = stopId;
        public string? StopName { get; set; }
    }

    public class Direction(string name, string sampleTrip)
    {
        public string Name { get; set; } = name;

        [JsonIgnore]
        public string SampleTrip { get; set; } = sampleTrip;

        public List<Stop> Stops { get; set; } = [];
    }

    public class Route(string id, string nom)
    {
        public string RouteId { get; set; } = id;
        public string RouteName { get; set; } = nom;
        public List<Direction> Directions { get; set; } = [];
    }
}
