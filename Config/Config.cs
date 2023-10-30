namespace blitz_api.Config
{
    public static class Config
    {
        static public readonly string ApiUrl = "https://api.stm.info/pub/od/gtfs-rt/ic/v2";
        static public readonly string TripEndpoint = "/tripUpdates";
        static public readonly string VehicleEndpoint = "/vehiclePositions";
        static public readonly string EtatEndpoint = "/etatService";

        static public readonly string StaticUrl = "https://www.stm.info/sites/default/files/gtfs/gtfs_stm.zip";
        static public readonly string StaticDir = "gtfs_static/";

        static public readonly string Routes = "gtfs_static/routes.txt";
        static public readonly string Trips = "gtfs_static/trips.txt";
        static public readonly string StopTimes = "gtfs_static/stop_times.txt";
        static public readonly string Stops = "gtfs_static/stops.txt";

        static public readonly string JsonFolderName = "GeneratedJson";
        static public readonly string JsonFileName = "data.json";

        static public readonly int NbLignesMetro = 5;
    }
}
