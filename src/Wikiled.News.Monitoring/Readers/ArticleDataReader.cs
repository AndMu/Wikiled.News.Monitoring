using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Wikiled.News.Monitoring.Data;

namespace Wikiled.News.Monitoring.Readers
{
    public class ArticleDataReader : IArticleDataReader
    {
        private readonly ILogger<ArticleDataReader> logger;

        private readonly IReadingSession session;

        public ArticleDataReader(ILogger<ArticleDataReader> logger, IReadingSession session)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<Article> Read(ArticleDefinition definition, CancellationToken token)
        {
            logger.LogDebug("Reading article: {0}[{1}]", definition.Title, definition.Id);
            var comments = ReadComments(definition, token);
            var readArticle = await session.ReadArticle(definition, token).ConfigureAwait(false);
            if (string.IsNullOrEmpty(definition.Title))
            {
                definition.Title = readArticle.Title;
            }

            definition.Date ??= DateTime.UtcNow;
            var article = new Article(definition, await comments.ConfigureAwait(false), readArticle, DateTime.UtcNow);
            logger.LogDebug("Reading article: {0}[{1}] Completed", definition.Title, definition.Id);
            return article;
        }

        public Task<CommentData[]> ReadComments(ArticleDefinition definition, CancellationToken token)
        {
            return session.ReadComments(definition, token);
        }
    }
}
