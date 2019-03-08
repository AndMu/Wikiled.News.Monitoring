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

        private readonly Func<ArticleDefinition, IArticleTextReader> articleTextReader;

        private readonly Func<ArticleDefinition, ICommentsReader> commentsReader;

        public ArticleDataReader(ILoggerFactory loggerFactory,
                                 Func<ArticleDefinition, IArticleTextReader> articleTextReader,
                                 Func<ArticleDefinition, ICommentsReader> commentsReader)
        {
            this.articleTextReader = articleTextReader ?? throw new ArgumentNullException(nameof(articleTextReader));
            this.commentsReader = commentsReader ?? throw new ArgumentNullException(nameof(commentsReader));
            logger = loggerFactory?.CreateLogger<ArticleDataReader>() ??
                throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Article> Read(ArticleDefinition definition, CancellationToken token)
        {
            logger.LogDebug("Reading article: {0}[{1}]", definition.Title, definition.Id);
            var comments = ReadComments(definition);
            var readArticle = articleTextReader(definition).ReadArticle(definition, token);
            return new Article(definition, await comments.ToArray(), await readArticle.ConfigureAwait(false), DateTime.UtcNow);
        }

        public IObservable<CommentData> ReadComments(ArticleDefinition definition)
        {
            return commentsReader(definition).ReadAllComments();
        }
    }
}
