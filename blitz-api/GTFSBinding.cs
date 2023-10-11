using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TransitRealtime;

namespace blitz_api
{
    /// <summary>
    /// Class GTFSBinding uses protobuffer data and returns the desired information
    /// </summary>
    public class GTFSBinding
    {
        STMGetter stm = new STMGetter();

        public async Task<string> GiveMeData()
        {
            byte[] vehicleJson = await stm.GetVehiclePositions();
            byte[] tripJson = await stm.GetTripUpdates();

            FeedMessage vehicleFeed = FeedMessage.Parser.ParseFrom(vehicleJson);
            FeedMessage tripFeed = FeedMessage.Parser.ParseFrom(tripJson);

            var parsedJson = JsonConvert.DeserializeObject(tripFeed.ToString());
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }

        public async Task<List<string>> GetHoraireForStop(string routeId, string stopId)
        {
            byte[] tripJson = await stm.GetTripUpdates();
            FeedMessage tripFeed = FeedMessage.Parser.ParseFrom(tripJson);

            DateTime maintenant = DateTime.Now.AddMinutes(-2);
            List<DateTime> arrivalTimes = new List<DateTime>();

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
