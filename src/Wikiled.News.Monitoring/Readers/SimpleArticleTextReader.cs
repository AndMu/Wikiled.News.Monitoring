using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wikiled.News.Monitoring.Data;
using Wikiled.News.Monitoring.Extensions;
using Wikiled.News.Monitoring.Readers.Parsers;
using Wikiled.News.Monitoring.Retriever;

namespace Wikiled.News.Monitoring.Readers
{
    public class SimpleArticleTextReader : IArticleTextReader
    {
        private readonly ILogger<SimpleArticleTextReader> logger;

        private readonly ILookup<string, IPageParser> parsersLookup;

        private readonly DefaultPageParser defaultPageParser;

        public SimpleArticleTextReader(ILogger<SimpleArticleTextReader> logger, IEnumerable<IPageParser> parsers, DefaultPageParser defaultPageParser)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.defaultPageParser = defaultPageParser ?? throw new ArgumentNullException(nameof(defaultPageParser));
            parsersLookup = parsers.ToLookup(item => item.Name, item => item);
        }

        public async Task<ArticleContent> ReadArticle(ITrackedRetrieval reader, ArticleDefinition definition, CancellationToken token)
        {
            logger.LogDebug("Reading article text: {0} {1}", definition.Id, definition.Title);
            var encoding = definition.Feed?.Encoding != null ? Encoding.GetEncoding(definition.Feed.Encoding) : null;
            var page = (await reader.Read(definition.Url, token, encoding: encoding).ConfigureAwait(false)).GetDocument();
            IPageParser parser = defaultPageParser;
            if (definition.Feed?.Category != null &&
                parsersLookup.Contains(definition.Feed.Category))
            {
                parser = parsersLookup[definition.Feed.Category].First();
            }

            return parser.Parse(definition, page);
        }

    }
}
