using System.Collections.Concurrent;

namespace blitz_api.Helpers
{
    public static class GlobalStore
    {
        public static ConcurrentDictionary<string, bool> GlobalVar;
    }
}
