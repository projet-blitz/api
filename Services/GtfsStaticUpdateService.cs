using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using static blitz_api.Config.Config;
using blitz_api.Controllers;
using blitz_api.Helpers;

namespace blitz_api.Services
{
    public class GtfsStaticUpdateService() : IHostedService, IDisposable
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

        private void CheckDates(object? state)
        {
            if (!GlobalStore.GlobalVar["IsUpdating"])
            {
                GtfsStaticController staticController = new();

                string staticDestinationPath = Path.Combine(Environment.CurrentDirectory, DataDir + StaticDir);
                if (!Directory.Exists(staticDestinationPath) || !File.Exists(FeedInfo) || !File.Exists(JsonFilePath))
                {
                    Console.WriteLine("[UPDATE SERVICE] Some folders and/or files are missing. Remaking Bus Network.");
                    staticController.MakeBusNetwork();
                }
                else
                {
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
                                    endDateTime.AddDays(-DaysBeforeUpdateDate);
                                    int comparisonResult = currentDate.CompareTo(endDateTime);

                                    if (comparisonResult >= 0)
                                    {
                                        Console.WriteLine($"[UPDATE SERVICE] Current date is equal to or after expiration date minus {DaysBeforeUpdateDate} days.");

                                        staticController.MakeBusNetwork();
                                    }
                                    else
                                    {
                                        Console.WriteLine($"[UPDATE SERVICE] Current date ({currentDate.ToShortDateString()}) is before expiration date ({endDateTime.ToShortDateString()}).");
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
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
        }
    }
}
