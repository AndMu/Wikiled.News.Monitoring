
using System.Net;
using Wikiled.Common.Net.Resilience;

namespace Wikiled.News.Monitoring.Retriever
{
    public class RetrieveConfiguration : IResilienceConfig
    {
        public static RetrieveConfiguration GenerateCommon()
        {
            return new RetrieveConfiguration
            {
                LongDelay = 1000 * 1000,
                ShortDelay = 1000,
                LongRetryCodes = new[] { HttpStatusCode.Forbidden },
                RetryCodes = new[]
                {
                    HttpStatusCode.RequestTimeout,      // 408
                    HttpStatusCode.InternalServerError, // 500
                    HttpStatusCode.BadGateway,          // 502
                    HttpStatusCode.ServiceUnavailable,  // 503
                    HttpStatusCode.GatewayTimeout       // 504
                },
                MaxConcurrent = 1,
                CallDelay = 50
            };
        }

        public HttpStatusCode[] RetryCodes { get; set; }

        public HttpStatusCode[] LongRetryCodes { get; set; }

        public string[] Ips { get; set; }

        public int LongDelay { get; set; }

        public int ShortDelay { get; set; }

        public int MaxConcurrent { get; set; }

        public int CallDelay { get; set; }
    }
}
