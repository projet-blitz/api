namespace blitz_api.Config
{
    public static class Config
    {
        // Update Service
        static public readonly string UpdateFlagKey = "IsUpdating";
        static public readonly TimeSpan UpdateCheckerTimer = TimeSpan.FromMinutes(1);
        static public readonly TimeSpan UpdateCacheTimer = TimeSpan.FromMinutes(3);

        static public readonly string BusNetworkFlagKey = "BusNetwork";
        static public readonly TimeSpan BusNetworkCacheTimer = TimeSpan.FromDays(100);

        // Timezone
        static public readonly string Timezone = "Eastern Standard Time";

        // STM Values
        static public readonly int MetroLines = 5;
        static public readonly int MinutesErrorMargin = 2;
        static public readonly int DaysBeforeUpdateDate = 9;

        // API & URLs
        static public readonly string ApiKeyWord = "stm_api_key";
        static public readonly string ApiParam = "apiKey";
        static public readonly string ApiUrl = "https://api.stm.info/pub/od/gtfs-rt/ic/v2";
        static public readonly string StaticDownloadUrl = "https://www.stm.info/sites/default/files/gtfs/gtfs_stm.zip";

        // Endpoints
        static public readonly string TripEndpoint = "/tripUpdates";
        static public readonly string VehicleEndpoint = "/vehiclePositions";
        static public readonly string EtatEndpoint = "/etatService";

        // Directories
        static public readonly string DataDir = "Data/";
        static public readonly string StaticDir = "GtfsStatic/";
        static public readonly string JsonDir = "GeneratedJson/";

        // File Paths
        static public readonly string FeedInfo =    DataDir + StaticDir + "feed_info.txt";
        static public readonly string Routes =      DataDir + StaticDir + "routes.txt";
        static public readonly string Trips =       DataDir + StaticDir + "trips.txt";
        static public readonly string StopTimes =   DataDir + StaticDir + "stop_times.txt";
        static public readonly string Stops =       DataDir + StaticDir + "stops.txt";

        static public readonly string JsonFilePath = DataDir + JsonDir + "BusNetwork.json";
    }
}
