using System.IO.Compression;
using System.Reflection;
using System.Text;
using TransitRealtime;

namespace blitz_api
{
    public class STMGetter
    {
        private readonly string _apiKey = "your STM api key";
        private readonly string _url = "https://api.stm.info/pub/od/gtfs-rt/ic/v2";
        private readonly string _staticUrl = "https://www.stm.info/sites/default/files/gtfs/gtfs_stm.zip";
        private readonly string _staticDir = "gtfs_stm/";

        public async Task<byte[]> GetTripUpdates()
        {
            HttpClient client = new();
            client.DefaultRequestHeaders.Add("apiKey", _apiKey);

            using HttpResponseMessage response = await client.GetAsync(_url + "/tripUpdates");

            var jsonResponse = await response.Content.ReadAsByteArrayAsync();

            return jsonResponse;
        }

        public async Task<byte[]> GetVehiclePositions()
        {
            HttpClient client = new();
            client.DefaultRequestHeaders.Add("apiKey", _apiKey);

            using HttpResponseMessage response = await client.GetAsync(_url + "/vehiclePositions");

            var jsonResponse = await response.Content.ReadAsByteArrayAsync();

            return jsonResponse;
        }

        public async Task<byte[]> GetEtatService()
        {
            HttpClient client = new();
            client.DefaultRequestHeaders.Add("apiKey", _apiKey);

            using HttpResponseMessage response = await client.GetAsync(_url + "/etatservice");

            var jsonResponse = await response.Content.ReadAsByteArrayAsync();

            return jsonResponse;
        }


        public async Task GetGtfsStatic()
        {
            string url = _staticUrl;
            string relativePath = _staticDir;

            string baseDirectory = Environment.CurrentDirectory;

            string destinationPath = Path.Combine(baseDirectory, relativePath);

            Directory.CreateDirectory(destinationPath);

            using (HttpClient httpClient = new())
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string fileName = Path.GetFileName(new Uri(url).LocalPath);

                    string localFilePath = Path.Combine(destinationPath, fileName);

                    using (var fileStream = File.Create(localFilePath))
                    {
                        await response.Content.CopyToAsync(fileStream);
                    }

                    Console.WriteLine($"Download successful. The file has been saved in: {localFilePath}");

                    // Unzip the downloaded file
                    ZipFile.ExtractToDirectory(localFilePath, destinationPath);

                    Console.WriteLine($"File successfully unzipped to: {destinationPath}");

                    // Delete the .zip file
                    File.Delete(localFilePath);

                    Console.WriteLine($"{fileName} has been deleted.");
                }
                else
                {
                    Console.WriteLine($"Download failed. Status code: {response.StatusCode}");
                }
            }
        }
    }
}
