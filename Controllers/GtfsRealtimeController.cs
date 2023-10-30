using dotenv.net;
using TransitRealtime;
using static blitz_api.Config.Config;

namespace blitz_api.Controllers
{
    public class GtfsRealtimeController
    {
        public async Task<byte[]> GetTripUpdates()
        {
            var envVars = DotEnv.Read();

            HttpClient client = new();
            client.DefaultRequestHeaders.Add("apiKey", envVars["stm_api_key"]);

            using HttpResponseMessage response = await client.GetAsync(ApiUrl + TripEndpoint);

            var jsonResponse = await response.Content.ReadAsByteArrayAsync();

            return jsonResponse;
        }

        public async Task<byte[]> GetVehiclePositions()
        {
            var envVars = DotEnv.Read();

            HttpClient client = new();
            client.DefaultRequestHeaders.Add("apiKey", envVars["stm_api_key"]);

            using HttpResponseMessage response = await client.GetAsync(ApiUrl + VehicleEndpoint);

            var jsonResponse = await response.Content.ReadAsByteArrayAsync();

            return jsonResponse;
        }

        public async Task<byte[]> GetEtatService()
        {
            var envVars = DotEnv.Read();

            HttpClient client = new();
            client.DefaultRequestHeaders.Add("apiKey", envVars["stm_api_key"]);

            using HttpResponseMessage response = await client.GetAsync(ApiUrl + EtatEndpoint);

            var jsonResponse = await response.Content.ReadAsByteArrayAsync();

            return jsonResponse;
        }

        public async Task<List<string>> GetHoraireForStop(string routeId, string stopId)
        {
            byte[] tripJson = await GetTripUpdates();
            FeedMessage tripFeed = FeedMessage.Parser.ParseFrom(tripJson);

            DateTime maintenant = DateTime.Now.AddMinutes(-2);
            List<DateTime> arrivalTimes = new();

            foreach (var entity in tripFeed.Entity)
            {
                if (entity.TripUpdate != null)
                {
                    var tripUpdate = entity.TripUpdate;

                    if (tripUpdate.Trip.RouteId == routeId)
                    {
                        foreach (var stopTimeUpdate in tripUpdate.StopTimeUpdate)
                        {
                            if (stopTimeUpdate.StopId == stopId)
                            {
                                DateTime arrivalTime = TimeZoneInfo.ConvertTimeFromUtc(
                                    DateTimeOffset.FromUnixTimeSeconds(stopTimeUpdate.Arrival.Time).DateTime,
                                    TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")
                                );

                                if (DateTime.Compare(maintenant, arrivalTime) < 0)
                                    arrivalTimes.Add(arrivalTime);
                            }
                        }
                    }
                }
            }

            arrivalTimes = arrivalTimes.OrderBy(x => x.TimeOfDay).ToList();
            List<string> arrivalTimeString = arrivalTimes.Select(x => x.ToShortTimeString()).ToList();

            return arrivalTimeString;
        }
    }
}
