using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using static blitz_api.Config.Config;
using blitz_api.Controllers;

namespace blitz_api.Services
{
    public class GtfsStaticUpdateService : IHostedService, IDisposable
    {
        private Timer? timer;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            timer = new Timer(CheckDates, null, TimeSpan.Zero, UpdateCheckerTimer);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            timer?.Dispose();
            GC.SuppressFinalize(this);
        }

        public void Test(object? state)
        {
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
                Console.WriteLine("JSON NOT EXIST - CREATING");
                Directory.CreateDirectory(jsonDestinationPath);
            }
            else
                Console.WriteLine("JSON EXISTS!!! - DELETING FILESS");
            {
                string[] files = Directory.GetFiles(jsonDestinationPath);
                foreach (string file in files)
                {
                    File.Delete(file);
                }
            }
        }

        private void CheckDates(object? state)
        {
            GtfsStaticController staticController = new();

            string staticDestinationPath = Path.Combine(Environment.CurrentDirectory, DataDir + StaticDir);
            if (!Directory.Exists(staticDestinationPath) || !File.Exists(FeedInfo))
            {
                Console.WriteLine("[UPDATE SERVICE] The static folder/file does not exist. Remaking folders.");
                staticController.MakeBusNetwork();

                while (staticController.IsUpdating())
                {

                }
            }
            try
            {
                using CsvReader csv = new(new StreamReader(FeedInfo), new CsvConfiguration(CultureInfo.InvariantCulture));
                {
                    if (csv.Read())
                    {
                        csv.Read(); // Skip the header
                        string endDate = csv.GetField(4)!;
                        csv.Dispose();

                        DateTime currentDate = DateTime.Now;

                        if (DateTime.TryParseExact(endDate, "yyyyMMdd", null, DateTimeStyles.None, out DateTime endDateTime))
                        {
                            endDateTime.AddDays(-NbJoursGtfsStaticDisponible);
                            int comparisonResult = currentDate.CompareTo(endDateTime);

                            if (comparisonResult >= 0)
                            {
                                Console.WriteLine("[UPDATE SERVICE] The current date is equal to or after the date from the CSV.");

                                staticController.MakeBusNetwork();
                            }
                            else
                            {
                                Console.WriteLine($"[UPDATE SERVICE] Current date ({currentDate.ToShortDateString()}) is before EXP date ({endDateTime.ToShortDateString()}).");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[UPDATE SERVICE] Invalid date format in CSV ({endDate}).");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ICI AAAAAAAAAAAAAAAAAAAA" + ex.ToString());
            }
        }
    }
}
