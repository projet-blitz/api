using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using System.Globalization;

namespace blitz_api
{
    public class StaticCsv
    {

        private readonly string routes = "gtfs_stm/routes.txt";
        private readonly string trips = "gtfs_stm/trips.txt";
        private readonly string stopTimes = "gtfs_stm/stop_times.txt";
        private readonly string stops = "gtfs_stm/stops.txt";

        /* Idéalement, on appelle une fois ces méthodes, 
         * on stocke le json dans un fichier et on renvoie 
         * le fichier sans avoir à trier les csv à chaque fois.
         * 
         * Puis, pour savoir quand le mettre à jour, on peut se fier
         * au "feed_end_date" dans "gtfs_stm/feed_info.txt".
         */

        public string GetAllRoutes()
        {

            List<RouteSTM> routesList = new();

            using (var reader = new StreamReader(routes))
            using (CsvReader csv = new(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                for (int i = 0; i < 5; i++)
                {
                    reader.ReadLine();
                }

                while (csv.Read())
                {
                    routesList.Add(new RouteSTM() 
                    {
                        routeId = csv.GetField(0)!, routeName = csv.GetField(3)!
                    });   
                }
            }

            var routesDict = new Dictionary<string, string>();

            foreach (var route in routesList)
            {
                routesDict.Add(route.routeId, route.routeName);
            }

            string jsonResponse = JsonConvert.SerializeObject(new { routes = routesDict }, Formatting.Indented);


            return jsonResponse;
        }

        public List<string> GetDirectionsForRouteId(string routeId)
        {
            List<string> directions = new();
            string direction1 = "";

            // Directions
            Dictionary<string, string> directionMap = new Dictionary<string, string>
            {
                { "E", "O" },
                { "O", "E" },
                { "N", "S" },
                { "S", "N" }
            };

            using (var reader = new StreamReader(trips))
            using (CsvReader csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                while (csv.Read())
                {
                    if (csv.GetField(0) == routeId)
                    {
                        direction1 = csv.GetField(3)!.Last().ToString();
                        break;
                    }
                }
            }

            if (directionMap.ContainsKey(direction1))
            {
                directions.Add(direction1);
                directions.Add(directionMap[direction1]);
            }
            else
            {
                Console.WriteLine("Unknown direction [" + direction1 + "]");
            }

            return directions;
        }

        public string GetStopIdsForRouteAndDirection(string routeId, string direction)
        {
            string routeHeadsign = routeId + "-" + direction;
            string selectedTrip = "";

            List<StopSTM> stopList = new List<StopSTM>();

            using (var reader = new StreamReader(trips))
            using (CsvReader csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                while (csv.Read())
                {
                    if (csv.GetField(3) == routeHeadsign)
                    {
                        selectedTrip = csv.GetField(2)!;
                        break;
                    }
                }
            }

            if (selectedTrip == "")
            {
                Console.WriteLine("No trip found for headsign [" + routeHeadsign + "]");
            } 
            else
            {
                using CsvReader csv = new CsvReader(new StreamReader(stopTimes), new CsvConfiguration(CultureInfo.InvariantCulture));
                while (csv.Read())
                {
                    if (csv.GetField(0) == selectedTrip)
                    {
                        stopList.Add(new StopSTM(csv.GetField(3)!, csv.GetField(4)!));
                    }
                }
            }

            stopList.ForEach(GetStopName);

            var stopTimesDict = new Dictionary<string, Dictionary<string, string>>();

            foreach (var stop in stopList)
            {
                stopTimesDict.Add(stop.stopSequence, new Dictionary<string, string>
                {
                    { "stopId", stop.stopId },
                    { "stopName", stop.stopName }
                });
            }

            string jsonResponse = JsonConvert.SerializeObject(new { stops = stopTimesDict }, Formatting.Indented);

            return jsonResponse;
        }

        public void GetStopName(StopSTM stop)
        {
            using (CsvReader csv = new CsvReader(new StreamReader(stops), new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                while (csv.Read())
                {
                    if (csv.GetField(1) == stop.stopId)
                    {
                        stop.stopName = csv.GetField(2)!;
                    }
                }
            }
        }


        public record RouteSTM
        {
            public string routeId = "";
            public string routeName = "";
        }


        public class StopSTM
        {
            public StopSTM(string id, string sequence) 
            { 
                stopId = id;
                stopSequence = sequence;
            }
            
            public string stopId = "";
            public string stopName = "";
            public string stopSequence = "";
            public string stopLon = "";
            public string stopLat = "";
        }

    }
}
