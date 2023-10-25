using CsvHelper.Configuration;
using CsvHelper;
using Newtonsoft.Json;
using System.Globalization;
using blitz_api.Models;

namespace blitz_api.Controllers
{
    public class CsvWorker
    {
        private readonly string routes = "gtfs_static/routes.txt";
        private readonly string trips = "gtfs_static/trips.txt";
        private readonly string stopTimes = "gtfs_static/stop_times.txt";
        private readonly string stops = "gtfs_static/stops.txt";

        private readonly string jsonFolderName = "GeneratedJson";
        private readonly string jsonFileName = "data.json";

        private readonly int nbLignesMetro = 5;

        public string GetBusSystem()
        {
            // Obtenir toutes les routes (IDs & Noms)
            Console.WriteLine("[1/4] Obtaining Routes...");

            List<Models.Route> routesList = new();

            using var reader = new StreamReader(routes);
            using CsvReader csv = new(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
            for (int i = 0; i < nbLignesMetro; i++)
            {
                reader.ReadLine();
            }

            while (csv.Read())
            {
                routesList.Add(new Models.Route(csv.GetField(0)!, csv.GetField(3)!));
            }

            // Obtenir toutes les directions
            Console.WriteLine("[2/4] Obtaining Directions...");

            Dictionary<string, string> directionMap = new()
            {
                { "E", "O" },
                { "O", "E" },
                { "N", "S" },
                { "S", "N" }
            };

            foreach (var route in routesList)
            {
                string direction1 = string.Empty;
                string direction2 = string.Empty;

                using var readerDirs = new StreamReader(trips);
                using CsvReader csvTrips = new(readerDirs, new CsvConfiguration(CultureInfo.InvariantCulture));
                {
                    while (csvTrips.Read())
                    {
                        if (route.Directions.Count == 0)
                        {
                            if (csvTrips.GetField(0) == route.RouteId)
                            {
                                direction1 = csvTrips.GetField(3)!.Last().ToString();
                                direction2 = directionMap[direction1];

                                route.Directions.Add(new Direction(direction1, csvTrips.GetField(2)!));
                            }
                        }
                        else
                        {
                            if (csvTrips.GetField(3) == route.RouteId + "-" + direction2)
                            {
                                route.Directions.Add(new Direction(directionMap[direction1], csvTrips.GetField(2)!));
                                break;
                            }
                        }
                    }
                }
            }

            // Obtenir tous les stops pour les routes
            Console.WriteLine("[3/4] Obtaining Stops...");

            Dictionary<string, List<Tuple<string, string>>> csvData = new();

            using var readerStops = new StreamReader(stopTimes);
            using CsvReader csvStopTimes = new(readerStops, new CsvConfiguration(CultureInfo.InvariantCulture));
            while (csvStopTimes.Read())
            {
                string tripId = csvStopTimes.GetField(0)!;
                string stopId = csvStopTimes.GetField(3)!;
                string stopSequence = csvStopTimes.GetField(4)!;

                if (!csvData.ContainsKey(tripId))
                {
                    csvData[tripId] = new List<Tuple<string, string>>();
                }

                csvData[tripId].Add(Tuple.Create(stopId, stopSequence));
            }

            foreach (var route in routesList)
            {
                foreach (var direction in route.Directions)
                {
                    if (csvData.TryGetValue(direction.SampleTrip, out var trip1Stops))
                    {
                        foreach (var stop in trip1Stops)
                        {
                            direction.Stops.Add(new Stop(stop.Item1, stop.Item2));
                        }
                    }
                }
            }

            // Obtenir tous les noms des stops
            Console.WriteLine("[4/4] Obtaining Stop Names...");

            Dictionary<string, string> csvStopData = new();
            using CsvReader csvStops = new(new StreamReader(stops), new CsvConfiguration(CultureInfo.InvariantCulture));
            while (csvStops.Read())
            {
                string stopId = csvStops.GetField(0)!;
                string stopName = csvStops.GetField(2)!;

                if (!csvStopData.ContainsKey(stopId))
                {
                    csvStopData[stopId] = stopName;
                }
            }

            foreach (var route in routesList)
            {
                foreach (var direction in route.Directions)
                {
                    foreach (var stopDir in direction.Stops)
                    {
                        if (csvStopData.TryGetValue(stopDir.StopId, out var csvStopName))
                        {
                            stopDir.StopName = csvStopName;
                        }
                    }
                }
            }

            Console.WriteLine("Process Finished");

            string json = JsonConvert.SerializeObject(routesList, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            });

            string baseDirectory = Environment.CurrentDirectory;
            string destinationPath = Path.Combine(baseDirectory, jsonFolderName);
            string localFilePath = Path.Combine(destinationPath, jsonFileName);

            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            File.WriteAllText(localFilePath, json);

            return json;
        }
    }
}
