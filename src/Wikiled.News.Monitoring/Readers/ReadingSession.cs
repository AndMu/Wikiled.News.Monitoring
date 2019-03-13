using Microsoft.Extensions.Logging;
using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wikiled.News.Monitoring.Data;
using Wikiled.News.Monitoring.Retriever;

namespace Wikiled.News.Monitoring.Readers
{
    public class ReadingSession : IReadingSession
    {
        private readonly ILogger<ReadingSession> logger;

        private readonly Func<ITrackedRetrieval, IAuthentication> authentication;

        private readonly  Func<ITrackedRetrieval, IArticleTextReader> textReader;

        private readonly Func<ITrackedRetrieval, ArticleDefinition, ICommentsReader> commentReader;

        private bool isInitialized;

        private readonly ITrackedRetrieval reader;

        private readonly RetrieveConfiguration httpConfiguration;

        private readonly SemaphoreSlim calls;

        private readonly SemaphoreSlim initializationLock = new SemaphoreSlim(1, 1);

        public ReadingSession(
            ILogger<ReadingSession> logger,
            RetrieveConfiguration httpConfiguration,
            Func<ITrackedRetrieval, IAuthentication> authentication,
            Func<ITrackedRetrieval, IArticleTextReader> textReader,
            Func<ITrackedRetrieval, ArticleDefinition, ICommentsReader> commentReader,
            ITrackedRetrieval reader)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.httpConfiguration = httpConfiguration ?? throw new ArgumentNullException(nameof(httpConfiguration));
            this.authentication = authentication ?? throw new ArgumentNullException(nameof(authentication));
            this.textReader = textReader ?? throw new ArgumentNullException(nameof(textReader));
            this.commentReader = commentReader ?? throw new ArgumentNullException(nameof(commentReader));
            this.reader = reader ?? throw new ArgumentNullException(nameof(reader));
            calls = new SemaphoreSlim(httpConfiguration.MaxConcurrent);
        }

        public async Task Initialize(CancellationToken token)
        {
            logger.LogDebug("Initialize");

            if (isInitialized)
            {
                logger.LogDebug("Already initialized");
                return;
            }

            try
            {
                await initializationLock.WaitAsync(token).ConfigureAwait(false);
                var result = await authentication(reader).Authenticate(token).ConfigureAwait(false);
                if (!result)
                {
                    logger.LogError("Authentication failed");
                    throw new Exception("Authentication failed");
                }

                isInitialized = true;
                logger.LogDebug("Initialized");
            }
            finally
            {
                initializationLock.Release();
            }
        }

        public Task<CommentData[]> ReadComments(ArticleDefinition article, CancellationToken token)
        {
            return Wrapper(async () => await commentReader(reader, article).ReadAllComments().ToArray(), token);
        }

        public Task<ArticleContent> ReadArticle(ArticleDefinition article, CancellationToken token)
        {
            return Wrapper(() => textReader(reader).ReadArticle(article, token), token);
        }

        private async Task<T> Wrapper<T>(Func<Task<T>> logic, CancellationToken token)
        {
            if (!isInitialized)
            {
                await Initialize(token).ConfigureAwait(false);
            }

            try
            {
                await calls.WaitAsync(token).ConfigureAwait(false);
                if (httpConfiguration.CallDelay > 0)
                {
                    logger.LogDebug("Wait until calling...");
                    await Task.Delay(httpConfiguration.CallDelay, token).ConfigureAwait(false);
                }

                var result = await logic().ConfigureAwait(false);
                if (httpConfiguration.CallDelay > 0)
                {
                    logger.LogDebug("Cooldown after calling...");
                    await Task.Delay(httpConfiguration.CallDelay, token).ConfigureAwait(false);
                }

                return result;
            }
            finally
            {
                calls.Release();
            }
        }
    }
}
