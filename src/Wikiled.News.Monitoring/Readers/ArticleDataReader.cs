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

        public ArticleDataReader(ILoggerFactory loggerFactory, IReadingSession session)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            logger = loggerFactory?.CreateLogger<ArticleDataReader>() ??
                throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Article> Read(ArticleDefinition definition, CancellationToken token)
        {
            logger.LogDebug("Reading article: {0}[{1}]", definition.Title, definition.Id);
            var comments = ReadComments(definition);
            var readArticle = session.ReadArticle(definition, token);
            return new Article(definition, await comments.ConfigureAwait(false), await readArticle.ConfigureAwait(false), DateTime.UtcNow);
        }

        public Task<CommentData[]> ReadComments(ArticleDefinition definition)
        {
            return session.ReadComments(definition);
        }
    }
}
