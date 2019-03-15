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
                     .Handle<WebException>(exception => !(exception.Response is HttpWebResponse response) || httpStatusCodesWorthRetrying.Contains(response.StatusCode))
                     .Or<IOException>()
                     .WaitAndRetryAsync(5,
                         (retries, ex, ctx) => ExecutionRoutine(config, ex, retries),
                         (ts, i, ctx, task) => Task.CompletedTask);
        }

        public async Task Authenticate(Uri uri, string data, CancellationToken token, Action<HttpWebRequest> modify = null)
        {
            using (var retriever = retrieverFactory(uri))
            {
                retriever.Modifier = modify;
                retriever.AllCookies = new CookieCollection();
                retriever.AllowGlobalRedirection = true;
                await policy.ExecuteAsync(t => retriever.PostData(data, t), token).ConfigureAwait(false);
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
                await policy.ExecuteAsync(t => retriever.ReceiveData(t), token).ConfigureAwait(false);
                return retriever.Data;
            }
        }

        public async Task ReadFile(Uri uri, Stream stream, CancellationToken token)
        {
            using (var retriever = retrieverFactory(uri))
            {
                retriever.AllCookies = collection;
                retriever.AllowGlobalRedirection = true;
                await policy.ExecuteAsync(t => retriever.ReceiveData(t, stream), token).ConfigureAwait(false);
                collection = retriever.AllCookies;
            }
        }

        private TimeSpan ExecutionRoutine(RetrieveConfiguration config, Exception ex, int retries)
        {
            var webException = ex as WebException;
            if (webException == null)
            {
                var waitTime = TimeSpan.FromSeconds(retries);
                logger.LogError(ex, "Error detected. Waiting {0}", waitTime);
                return waitTime;
            }

            var response = webException.Response as HttpWebResponse;
            var errorCode = response?.StatusCode;
            if (errorCode == null ||
                !config.LongRetryCodes.Contains(errorCode.Value))
            {
                var waitTime = TimeSpan.FromSeconds(retries);
                logger.LogError(ex, "Web Error detected. Waiting {0}", waitTime);
                return waitTime;
            }

            var wait = TimeSpan.FromSeconds(config.LongRetryDelay);
            logger.LogError(ex, "Forbidden detected. Waiting {0}", wait);
            return wait;
        }
    }
}
