using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wikiled.Common.Net.Resilience;

namespace Wikiled.News.Monitoring.Retriever
{
    public class TrackedRetrieval : ITrackedRetrieval
    {
        private readonly ILogger<TrackedRetrieval> logger;

        private readonly Func<Uri, IDataRetriever> retrieverFactory;

        private CookieCollection collection;

        private readonly IResilience resilience;

        public TrackedRetrieval(ILogger<TrackedRetrieval> logger, Func<Uri, IDataRetriever> retrieverFactory, RetrieveConfiguration config, IResilience resilience)
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
            this.resilience = resilience ?? throw new ArgumentNullException(nameof(resilience));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ResetCookies();
        }

        public void ResetCookies()
        {
            collection = new CookieCollection();
        }

        public Task<string> Post(Uri uri, string data, CancellationToken token, Action<HttpWebRequest> modify = null)
        {
            return ProcessQuery(uri, (retriever, t) => retriever.PostData(data, t), token, modify);
        }

        public Task<string> Read(Uri uri, CancellationToken token, Action<HttpWebRequest> modify = null, Encoding encoding = null)
        {
            return ProcessQuery(uri, (retriever, t) => retriever.ReceiveData(t), token, modify, encoding);
        }

        public Task<string> ReadFile(Uri uri, Stream stream, CancellationToken token)
        {
            return ProcessQuery(uri, (retriever, t) => retriever.ReceiveData(t, stream), token, null);
        }

        private async Task<string> ProcessQuery(Uri uri, Func<IDataRetriever, CancellationToken, Task> query, CancellationToken token, Action<HttpWebRequest> modify, Encoding encoding = null)
        {
            using var retriever = retrieverFactory(uri);
            retriever.Modifier = modify;
            retriever.DefaultEncoding = encoding ?? Encoding.UTF8;
            retriever.AllowGlobalRedirection = true;
            retriever.AllCookies = collection;
            await resilience.WebPolicy.ExecuteAsync(t => query(retriever, t), token).ConfigureAwait(false);
            collection = retriever.AllCookies;
            return retriever.Data;
        }
    }
}