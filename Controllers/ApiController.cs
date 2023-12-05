using blitz_api.Helpers;
using Microsoft.AspNetCore.Mvc;
using TransitRealtime;
using static blitz_api.Config.Config;

namespace blitz_api.Controllers
{
    [ApiController]
    [Route("/api")]
    public class ApiController() : Controller
    {
        [HttpGet]
        [Route("GetBusNetwork")]
        public ActionResult GetBusNetwork()
        {
            if (!GlobalStore.GlobalVar["IsUpdating"]) 
            {
                try
                {
                    string localFilePath = Path.Combine(Environment.CurrentDirectory, JsonFilePath);
                    string jsonContent = System.IO.File.ReadAllText(localFilePath);
                    return Content(jsonContent, "application/json");
                } 
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                } 
            }
            return Json("[105] Bus Network is updating");
        }

        [HttpGet]
        [Route("{routeId}/{stopId}")]
        public async Task<ActionResult> GetHoraireForStop(string routeId, string stopId)
        {
            byte[] tripJson = await GtfsRealtimeController.GetRealtime(TripEndpoint);

            if (tripJson == Array.Empty<byte>())
            {
                return Json("[500] Cannot connect to STM");
            }

            FeedMessage tripFeed = FeedMessage.Parser.ParseFrom(tripJson);

            DateTime maintenant = DateTime.Now.AddMinutes(-MinutesErrorMargin);
            List<DateTime> arrivalTimes = [];

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
                                if (stopTimeUpdate.Arrival != null)
                                {
                                    DateTime arrivalTime = TimeZoneInfo.ConvertTimeFromUtc(
                                        DateTimeOffset.FromUnixTimeSeconds(stopTimeUpdate.Arrival.Time).DateTime,
                                        TimeZoneInfo.FindSystemTimeZoneById(Timezone)
                                    );

                                    if (DateTime.Compare(maintenant, arrivalTime) < 0)
                                        arrivalTimes.Add(arrivalTime);
                                } 
                                else
                                {
                                    Console.WriteLine(stopTimeUpdate);
                                }
                            }
                        }
                    }
                }
            }

            if (arrivalTimes.Count > 0)
            {
                arrivalTimes = [.. arrivalTimes.OrderBy(x => x.TimeOfDay)];
                List<string> arrivalTimeString = arrivalTimes.Select(x => x.ToShortTimeString()).ToList();

                return Json(arrivalTimeString);
            }
            else
            {
                return Json("[200] No stop time found.");
            }
            
        }
        
    }
}
