using blitz_api.Helpers;
using Newtonsoft.Json;

namespace blitz_api.Models
{
    [JsonConverter(typeof(DirectionConverter))]
    public class Stop
    {
        public string StopSequence { get; set; }
        public string StopId { get; set; }
        public string? StopName { get; set; }

        public Stop(string stopId, string stopSequence)
        {
            StopSequence = stopSequence;
            StopId = stopId;
        }
    }

    public class Direction
    {
        public string Name { get; set; }

        [JsonIgnore]
        public string SampleTrip { get; set; }
        public List<Stop> Stops { get; set; }

        public Direction(string name, string sampleTrip)
        {
            Name = name;
            SampleTrip = sampleTrip;
            Stops = new();
        }
    }

    public class Route
    {
        public string RouteId { get; set; }
        public string RouteName { get; set; }
        public List<Direction> Directions { get; set; }

        public Route(string id, string nom)
        {
            RouteId = id;
            RouteName = nom;
            Directions = new();
        }
    }
}
