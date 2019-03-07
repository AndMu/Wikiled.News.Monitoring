
using System.Net;

namespace Wikiled.News.Monitoring.Retriever
{
    public class RetrieveConfiguration
    {
        public HttpStatusCode[] RetryCodes { get; set; }

        public HttpStatusCode[] LongRetryCodes { get; set; }

        public string[] Ips { get; set; }

        public int LongRetryDelay { get; set; }

        public int MaxConcurrent { get; set; }

        public int CallDelay { get; set; }
    }
}
