using System.Collections.Generic;

namespace UrlShortenerApi.Services
{
    public class UrlDatabase
    {
        public Dictionary<string, string> Map { get; }

        public UrlDatabase()
        {
            Map = new Dictionary<string, string>();
        }
    }
}