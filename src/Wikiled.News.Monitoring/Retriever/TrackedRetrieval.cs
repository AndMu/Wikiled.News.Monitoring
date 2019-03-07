using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Polly.Retry;

namespace Wikiled.News.Monitoring.Retriever
{
    public class TrackedRetrieval : ITrackedRetrieval
    {
        private CookieCollection collection;

        private readonly ILogger<TrackedRetrieval> logger;

        private readonly Func<Uri, IDataRetriever> retrieverFactory;

        private readonly AsyncRetryPolicy policy;

        public TrackedRetrieval(ILogger<TrackedRetrieval> logger, Func<Uri, IDataRetriever> retrieverFactory, RetrieveConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (config.LongRetryCodes == null)
            {
                throw new ArgumentNullException(nameof(config.LongRetryCodes));
            }

            this.retrieverFactory = retrieverFactory ?? throw new ArgumentNullException(nameof(retrieverFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var httpStatusCodesWorthRetrying = config.LongRetryCodes.Concat(config.RetryCodes).ToArray();
            policy = Policy
                     .Handle<WebException>(exception =>
                     {
                         var response = (HttpWebResponse)exception.Response;
                         return response != null && httpStatusCodesWorthRetrying.Contains(response.StatusCode);
                     })
                     .WaitAndRetryAsync(5,
                         (retries, ex, ctx) => ExecutionRoutine(logger, config, ex, retries),
                         (ts, i, ctx, task) => Task.CompletedTask);
        }

        public async Task Authenticate(Uri uri, string data, CancellationToken token, Action<HttpWebRequest> modify = null)
        {
            using (var retriever = retrieverFactory(uri))
            {
                retriever.Modifier = modify;
                retriever.AllCookies = new CookieCollection();
                retriever.AllowGlobalRedirection = true;
                await policy.ExecuteAsync(() => retriever.PostData(data, token)).ConfigureAwait(false);
                collection = retriever.AllCookies;
            }
        }

        public async Task<string> Read(Uri uri, CancellationToken token, Action<HttpWebRequest> modify = null)
        {
            using (var retriever = retrieverFactory(uri))
            {
                retriever.Modifier = modify;
                retriever.AllowGlobalRedirection = true;
                retriever.AllCookies = collection;
                await policy.ExecuteAsync(() => retriever.ReceiveData(token)).ConfigureAwait(false);
                return retriever.Data;
            }
        }

        public async Task ReadFile(Uri uri, Stream stream, CancellationToken token)
        {
            using (var retriever = retrieverFactory(uri))
            {
                retriever.AllCookies = collection;
                retriever.AllowGlobalRedirection = true;
                await policy.ExecuteAsync(() => retriever.ReceiveData(token, stream)).ConfigureAwait(false);
                collection = retriever.AllCookies;
            }
        }

        private static TimeSpan ExecutionRoutine(ILogger<TrackedRetrieval> logger, RetrieveConfiguration config, Exception ex, int retries)
        {
            if (!config.LongRetryCodes.Contains(((HttpWebResponse)((WebException)ex).Response).StatusCode))
            {
                return TimeSpan.FromSeconds(retries);
            }

            var wait = TimeSpan.FromSeconds(config.LongRetryDelay);
            logger.LogError("Forbidden detected. Waiting {0}", wait);
            return wait;
        }
    }
}
