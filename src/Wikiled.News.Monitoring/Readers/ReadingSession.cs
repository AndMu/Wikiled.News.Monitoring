using Microsoft.Extensions.Logging;
using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Wikiled.News.Monitoring.Data;
using Wikiled.News.Monitoring.Retriever;

namespace Wikiled.News.Monitoring.Readers
{
    public class ReadingSession : IReadingSession
    {
        private readonly ILogger<ReadingSession> logger;

        private readonly IServiceProvider provider;

        private bool isInitialized;

        private bool isDisposed;

        private readonly ITrackedRetrieval reader;

        private readonly RetrieveConfiguration httpConfiguration;

        private readonly SemaphoreSlim callsLock = new SemaphoreSlim(1, 1);

        public ReadingSession(
            ILogger<ReadingSession> logger,
            RetrieveConfiguration httpConfiguration,
            ITrackedRetrieval reader,
            IServiceProvider provider)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.httpConfiguration = httpConfiguration ?? throw new ArgumentNullException(nameof(httpConfiguration));
            this.reader = reader ?? throw new ArgumentNullException(nameof(reader));
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public async Task Initialize(CancellationToken token)
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException("Can't initialize Already disposed");
            }

            if (isInitialized)
            {
                logger.LogDebug("Already initialized");
                return;
            }

            try
            {
                await callsLock.WaitAsync(token).ConfigureAwait(false);
                if (isInitialized)
                {
                    logger.LogDebug("Already initialized");
                    return;
                }

                var result = await provider.GetRequiredService<IAuthentication>().Authenticate(reader, token).ConfigureAwait(false);
                if (!result)
                {
                    logger.LogError("Authentication failed");
                    throw new Exception("Authentication failed");
                }

                isInitialized = true;
            }
            finally
            {
                callsLock.Release();
            }
        }

        public Task<CommentData[]> ReadComments(ArticleDefinition article, CancellationToken token)
        {
            return Wrapper(async () => await provider.GetRequiredService<ICommentsReader>().ReadAllComments(reader, article).ToArray(), token);
        }

        public Task<ArticleContent> ReadArticle(ArticleDefinition article, CancellationToken token)
        {
            return Wrapper(() => provider.GetRequiredService<IArticleTextReader>().ReadArticle(reader, article, token), token);
        }

        private async Task<T> Wrapper<T>(Func<Task<T>> logic, CancellationToken token)
        {
            if (!isInitialized)
            {
                await Initialize(token).ConfigureAwait(false);
            }

            try
            {
                logger.LogTrace("Starting...");
                await callsLock.WaitAsync(token).ConfigureAwait(false);
                if (httpConfiguration.CallDelay > 0)
                {
                    logger.LogTrace("Wait until calling...");
                    await Task.Delay(httpConfiguration.CallDelay, token).ConfigureAwait(false);
                }

                logger.LogDebug("Processing..");
                var result = await logic().ConfigureAwait(false);
                if (httpConfiguration.CallDelay > 0)
                {
                    logger.LogTrace("Cooldown after calling...");
                    await Task.Delay(httpConfiguration.CallDelay, token).ConfigureAwait(false);
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Final failure");
                throw;
            }
            finally
            {
                isInitialized = false;
                callsLock.Release();
            }
        }
    }
}
