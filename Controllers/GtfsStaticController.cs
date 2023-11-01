﻿using blitz_api.Helpers;
using blitz_api.Models;
using CsvHelper.Configuration;
using CsvHelper;
using Newtonsoft.Json;
using System.Globalization;
using System.IO.Compression;
using static blitz_api.Config.Config;

namespace blitz_api.Controllers
{
    public class GtfsStaticController
    {
        private bool isUpdating = false;

        public bool IsUpdating()
        {
            return isUpdating;
        }

        public string GetBusNetwork()
        {
            string baseDirectory = Environment.CurrentDirectory;
            string localFilePath = Path.Combine(baseDirectory, JsonFilePath);

            return localFilePath;
        }

        public async void MakeBusNetwork()
        {
            isUpdating = true;

            try
            {
                CreateDirs();
                await GetGtfsStatic();
                GetBusSystem();
            } 
            catch (Exception ex)
            {
                Console.WriteLine("Failed to create bus network : " + ex.ToString());
            }

            isUpdating = false;
        }

        private void CreateDirs()
        {
            Console.WriteLine("[1/9] Creating folders...");

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
        private async Task GetGtfsStatic()
        {
            string baseDirectory = Environment.CurrentDirectory;
            string destinationPath = Path.Combine(baseDirectory, DataDir + StaticDir);

            Console.WriteLine("[2/9] Downloading files...");

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

                Console.WriteLine($"[3/9] Download successful. File saved in: {localFilePath}");

                ZipFile.ExtractToDirectory(localFilePath, destinationPath);

                Console.WriteLine($"[4/9] Successfully unzipped file");

                File.Delete(localFilePath);

                Console.WriteLine($"[5/9] {fileName} has been deleted.");
            }
            else
            {
                throw new Exception($"Download failed. Status code: {response.StatusCode}");
            }
        }

        /// <summary>
        /// Génère le système de bus selon le GTFS static
        /// et le sauvegarde dans un fichier .json
        /// </summary>
        private void GetBusSystem()
        {
            // Obtenir toutes les routes (IDs & Noms)
            Console.WriteLine("[6/9] Obtaining Routes...");

            List<Models.Route> routesList = new();

            using var reader = new StreamReader(Routes);
            using CsvReader csv = new(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
            for (int i = 0; i < NbLignesMetro; i++)
            {
                reader.ReadLine();
            }

            while (csv.Read())
            {
                routesList.Add(new Models.Route(csv.GetField(0)!, csv.GetField(3)!));
            }

            // Obtenir toutes les directions
            Console.WriteLine("[7/9] Obtaining Directions...");

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
            Console.WriteLine("[8/9] Obtaining Stops...");

            Dictionary<string, List<Tuple<string, string>>> csvData = new();

            using var readerStops = new StreamReader(StopTimes);
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
            Console.WriteLine("[9/9] Obtaining Stop Names...");

            Dictionary<string, string> csvStopData = new();
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
                throw new Exception("Cannot write file - JSON directory does not exist");
            }

            Console.WriteLine("Bus Network update successful");
        }
    }
}
