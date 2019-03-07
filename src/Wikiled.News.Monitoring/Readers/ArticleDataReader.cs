using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Wikiled.News.Monitoring.Data;

namespace Wikiled.News.Monitoring.Readers
{
    public class ArticleDataReader : IArticleDataReader
    {
        private readonly ILogger<ArticleDataReader> logger;

        private readonly ISessionReader sessionReader;

        public ArticleDataReader(ILoggerFactory loggerFactory, ISessionReader sessionReader)
        {
            this.sessionReader = sessionReader ?? throw new ArgumentNullException(nameof(sessionReader));
            logger = loggerFactory?.CreateLogger<ArticleDataReader>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Article> Read(ArticleDefinition definition, CancellationToken token)
        {
            logger.LogDebug("Reading article: {0}[{1}]", definition.Title, definition.Id);
            var comments = ReadComments(definition, token);
            var readArticle = sessionReader.ReadArticle(definition, token);
            return new Article(definition, await comments.ConfigureAwait(false), await readArticle.ConfigureAwait(false), DateTime.UtcNow);
        }

        public Task<CommentData[]> ReadComments(ArticleDefinition definition, CancellationToken token)
        {
            return sessionReader.ReadComments(definition, token);
        }
    }
}
