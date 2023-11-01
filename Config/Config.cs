namespace blitz_api.Config
{
    public static class Config
    {
        // URLs
        static public readonly string ApiUrl = "https://api.stm.info/pub/od/gtfs-rt/ic/v2";
        static public readonly string StaticDownloadUrl = "https://www.stm.info/sites/default/files/gtfs/gtfs_stm.zip";

        // Endpoints
        static public readonly string TripEndpoint = "/tripUpdates";
        static public readonly string VehicleEndpoint = "/vehiclePositions";
        static public readonly string EtatEndpoint = "/etatService";

        // Directories
        static public readonly string DataDir = "Data/";
        static public readonly string StaticDir = "gtfs_static/";
        static public readonly string JsonDir = "GeneratedJson/";

        // File Paths
        static public readonly string FeedInfo =    DataDir + StaticDir + "feed_info.txt";
        static public readonly string Routes =      DataDir + StaticDir + "routes.txt";
        static public readonly string Trips =       DataDir + StaticDir + "trips.txt";
        static public readonly string StopTimes =   DataDir + StaticDir + "stop_times.txt";
        static public readonly string Stops =       DataDir + StaticDir + "stops.txt";

        static public readonly string JsonFilePath = DataDir + JsonDir + "BusNetwork.json";

        // STM Values
        static public readonly int NbLignesMetro = 5;
        static public readonly int NbJoursGtfsStaticDisponible = 9;

        // Update Timer
        static public readonly TimeSpan UpdateCheckerTimer = TimeSpan.FromSeconds(50);
    }
}
