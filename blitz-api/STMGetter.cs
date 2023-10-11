using System.Text;
using TransitRealtime;

namespace blitz_api
{
    public class STMGetter
    {
        private readonly string _apiKey = "l7c26e992552bb4137814fddf740595bf5";
        private readonly string _url = "https://api.stm.info/pub/od/gtfs-rt/ic/v2";

        public async Task<byte[]> GetTripUpdates()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("apiKey", _apiKey);

            using HttpResponseMessage response = await client.GetAsync(_url + "/tripUpdates");

            var jsonResponse = await response.Content.ReadAsByteArrayAsync();

            return jsonResponse;
        }

        public async Task<byte[]> GetVehiclePositions()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("apiKey", _apiKey);

            using HttpResponseMessage response = await client.GetAsync(_url + "/vehiclePositions");

            var jsonResponse = await response.Content.ReadAsByteArrayAsync();

            return jsonResponse;
        }

        public async Task<byte[]> GetEtatService()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("apiKey", _apiKey);

            using HttpResponseMessage response = await client.GetAsync(_url + "/etatservice");

            var jsonResponse = await response.Content.ReadAsByteArrayAsync();

            return jsonResponse;
        }
    }
}
