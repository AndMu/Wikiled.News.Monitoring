using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Wikiled.News.Monitoring.Data;
using Wikiled.News.Monitoring.Extensions;
using Wikiled.News.Monitoring.Retriever;

namespace Wikiled.News.Monitoring.Readers
{
    public class SimpleArticleTextReader : IArticleTextReader
    {
        private readonly ILogger<SimpleArticleTextReader> logger;

        private readonly ITrackedRetrieval reader;

        public SimpleArticleTextReader(ILogger<SimpleArticleTextReader> logger, ITrackedRetrieval reader)
        {
            this.reader = reader ?? throw new ArgumentNullException(nameof(reader));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ArticleContent> ReadArticle(ArticleDefinition definition, CancellationToken token)
        {
            logger.LogDebug("Reading article text: {0}", definition.Id);
            var page = (await reader.Read(definition.Url, token).ConfigureAwait(false)).GetDocument();
            var doc = page.DocumentNode.InnerText;

            return new ArticleContent
                   {
                       Title = definition.Url.ToString(),
                       Text = doc
                   };
        }
    }
}
