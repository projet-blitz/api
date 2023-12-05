using blitz_api.Helpers;
using blitz_api.Models;
using CsvHelper.Configuration;
using CsvHelper;
using Newtonsoft.Json;
using System.Globalization;
using System.IO.Compression;
using static blitz_api.Config.Config;

namespace blitz_api.Controllers
{
    public class GtfsStaticController()
    {
        public async void MakeBusNetwork()
        {
            if (!GlobalStore.GlobalVar[UpdateFlagKey])
            {
                GlobalStore.GlobalVar[UpdateFlagKey] = true;

                try
                {
                    CreateDirs();
                    await GetGtfsStatic();
                    GetBusSystem();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[GTFS STATIC] Failed to create bus network : " + ex.ToString());
                }
                finally 
                {
                    GlobalStore.GlobalVar[UpdateFlagKey] = false;
                }
            } else
            {
                Console.WriteLine("[GTFS STATIC] Update already running.");
            }
        }

        private static void CreateDirs()
        {
            Console.WriteLine("[GTFS STATIC] Creating folders (1/9)");

            string baseDirectory = Environment.CurrentDirectory;

            // Data directory containing both subdirs
            string dataDestinationPath = Path.Combine(baseDirectory, DataDir);
            if (!Directory.Exists(dataDestinationPath))
                Directory.CreateDirectory(dataDestinationPath);

            // GTFS static dir
            string staticDestinationPath = Path.Combine(dataDestinationPath, StaticDir);
            if (!Directory.Exists(staticDestinationPath))
            {
                Directory.CreateDirectory(staticDestinationPath);
            }
            else
            {
                string[] files = Directory.GetFiles(staticDestinationPath);
                foreach (string file in files)
                {
                    File.Delete(file);
                }
            }

            // Generated json dir
            string jsonDestinationPath = Path.Combine(dataDestinationPath, JsonDir);
            if (!Directory.Exists(jsonDestinationPath))
            {
                Directory.CreateDirectory(jsonDestinationPath);
            }
            else
            {
                string[] files = Directory.GetFiles(jsonDestinationPath);
                foreach (string file in files)
                {
                    File.Delete(file);
                }
            }
        }

        /// <summary>
        /// Obtient le GTFS static (planifié) de la STM.
        /// </summary>
        private static async Task GetGtfsStatic()
        {
            string baseDirectory = Environment.CurrentDirectory;
            string destinationPath = Path.Combine(baseDirectory, DataDir + StaticDir);

            Console.WriteLine("[GTFS STATIC] Downloading files (2/9)");

            using HttpClient httpClient = new();
            HttpResponseMessage response = await httpClient.GetAsync(StaticDownloadUrl);

            if (response.IsSuccessStatusCode)
            {
                string fileName = Path.GetFileName(new Uri(StaticDownloadUrl).LocalPath);

                string localFilePath = Path.Combine(destinationPath, fileName);

                using (var fileStream = File.Create(localFilePath))
                {
                    await response.Content.CopyToAsync(fileStream);
                }

                Console.WriteLine($"[GTFS STATIC] Download successful (3/9). File saved in: {localFilePath}");

                ZipFile.ExtractToDirectory(localFilePath, destinationPath);

                Console.WriteLine($"[GTFS STATIC] Successfully unzipped file (4/9)");

                File.Delete(localFilePath);

                Console.WriteLine($"[GTFS STATIC] {fileName} has been deleted (5/9)");
            }
            else
            {
                throw new Exception($"[GTFS STATIC] Download failed. Status code: {response.StatusCode}");
            }
        }

        /// <summary>
        /// Generates the bus network from the static csv
        /// Saves the network in a json file.
        /// </summary>
        private void GetBusSystem()
        {
            // Obtain all bus lines (IDs & Names)
            Console.WriteLine("[GTFS STATIC] Obtaining Routes (6/9)");

            List<Models.Route> routesList = [];

            using var reader = new StreamReader(Routes);
            using CsvReader csv = new(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
            for (int i = 0; i < MetroLines; i++)
            {
                reader.ReadLine(); //Skip metro lines
            }

            while (csv.Read())
            {
                routesList.Add(new Models.Route(csv.GetField(0)!, csv.GetField(3)!));
            }

            // Obtain directions for all bus lines
            Console.WriteLine("[GTFS STATIC] Obtaining Directions (7/9)");

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

                using var readerDirs = new StreamReader(Trips);
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
                            if (csvTrips.GetField(3) == $"{route.RouteId}-{direction2}")
                            {
                                route.Directions.Add(new Direction(directionMap[direction1], csvTrips.GetField(2)!));
                                break;
                            }
                        }
                    }
                }
            }

            // Obtain stops for every bus lines
            Console.WriteLine("[GTFS STATIC] Obtaining Stops (8/9)");

            Dictionary<string, List<Tuple<string, string>>> csvData = [];

            using var readerStops = new StreamReader(StopTimes);
            using CsvReader csvStopTimes = new(readerStops, new CsvConfiguration(CultureInfo.InvariantCulture));
            while (csvStopTimes.Read())
            {
                string tripId = csvStopTimes.GetField(0)!;
                string stopId = csvStopTimes.GetField(3)!;
                string stopSequence = csvStopTimes.GetField(4)!;

                if (!csvData.TryGetValue(tripId, out List<Tuple<string, string>>? value))
                {
                    value = ([]);
                    csvData[tripId] = value;
                }

                value.Add(Tuple.Create(stopId, stopSequence));
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

            // Obtain names for all stops
            Console.WriteLine("[GTFS STATIC] Obtaining Stop Names (9/9)");

            Dictionary<string, string> csvStopData = [];
            using CsvReader csvStops = new(new StreamReader(Stops), new CsvConfiguration(CultureInfo.InvariantCulture));
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

            string json = JsonConvert.SerializeObject(routesList, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new DirectionContractResolver()
            });

            string baseDirectory = Environment.CurrentDirectory;
            string jsonDirectory = Path.Combine(baseDirectory, DataDir + JsonDir);
            string jsonFilePath = Path.Combine(baseDirectory, JsonFilePath);

            if (Directory.Exists(jsonDirectory))
            {
                File.WriteAllText(jsonFilePath, json);
            }
            else
            {
                throw new Exception("[GTFS STATIC] Cannot write file - JSON directory does not exist");
            }

            Console.WriteLine("[GTFS STATIC] Bus Network update successful");
        }
    }
}
