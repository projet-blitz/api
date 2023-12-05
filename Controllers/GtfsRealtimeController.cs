using dotenv.net;
using Microsoft.AspNetCore.Mvc;
using TransitRealtime;
using static blitz_api.Config.Config;

namespace blitz_api.Controllers
{
    public class GtfsRealtimeController
    {
        public static async Task<byte[]> GetRealtime(string endpoint)
        {
            var envVars = DotEnv.Read();

            HttpClient client = new();
            client.DefaultRequestHeaders.Add(ApiParam, envVars[ApiKeyWord]);

            using HttpResponseMessage response = await client.GetAsync(ApiUrl + endpoint);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsByteArrayAsync();
                return jsonResponse;
            }
            else
            {
                return [];
            }
        }
    }
}
