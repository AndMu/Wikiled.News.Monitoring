using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Wikiled.News.Monitoring.Retriever
{
    public class TrackedRetrieval : ITrackedRetrieval
    {
        private readonly ILogger<TrackedRetrieval> logger;

        private readonly AsyncRetryPolicy policy;

        private readonly Func<Uri, IDataRetriever> retrieverFactory;
        private CookieCollection collection;

        public TrackedRetrieval(ILogger<TrackedRetrieval> logger,
                                Func<Uri, IDataRetriever> retrieverFactory,
                                RetrieveConfiguration config)
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
                .Handle<WebException>(exception => !(exception.Response is HttpWebResponse response) ||
                                          httpStatusCodesWorthRetrying.Contains(response.StatusCode))
                .Or<IOException>()
                .WaitAndRetryAsync(5,
                                   (retries, ex, ctx) => ExecutionRoutine(config, ex, retries),
                                   (ts, i, ctx, task) => Task.CompletedTask);
        }

        public void ResetCookies()
        {
            collection = new CookieCollection();
        }

        public Task Post(Uri uri, string data, CancellationToken token, Action<HttpWebRequest> modify = null)
        {
            return ProcessQuery(uri, (retriever, t) => retriever.PostData(data, t), token, modify);
        }

        public Task<string> Read(Uri uri, CancellationToken token, Action<HttpWebRequest> modify = null)
        {
            return ProcessQuery(uri, (retriever, t) => retriever.ReceiveData(t), token, modify);
        }

        public Task ReadFile(Uri uri, Stream stream, CancellationToken token)
        {
            return ProcessQuery(uri, (retriever, t) => retriever.ReceiveData(t, stream), token, null);
        }

        private async Task<string> ProcessQuery(Uri uri,
                                                Func<IDataRetriever, CancellationToken, Task> query,
                                                CancellationToken token,
                                                Action<HttpWebRequest> modify)
        {
            using (var retriever = retrieverFactory(uri))
            {
                retriever.Modifier = modify;
                retriever.AllowGlobalRedirection = true;
                retriever.AllCookies = collection;
                await policy.ExecuteAsync(t => query(retriever, t), token).ConfigureAwait(false);
                collection = retriever.AllCookies;
                return retriever.Data;
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