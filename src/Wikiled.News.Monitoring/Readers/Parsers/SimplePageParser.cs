using HtmlAgilityPack;
using System;
using Fizzler.Systems.HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Wikiled.News.Monitoring.Config;
using Wikiled.News.Monitoring.Data;

namespace Wikiled.News.Monitoring.Readers.Parsers
{
    public class SimplePageParser : IPageParser
    {
        private readonly FeedParsingConfig config;

        private readonly ILogger<SimplePageParser> logger;

        public SimplePageParser(ILogger<SimplePageParser> logger, FeedParsingConfig config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string Name => config.Name;

        public ArticleContent Parse(ArticleDefinition definition, HtmlDocument document)
        {
            logger.LogInformation("Parsing: {0}", definition.Title);
            var doc = document.DocumentNode;
            var article = doc.QuerySelector(config.ContentPath)?.InnerText?.Trim();
            var title = config.TitlePath != null ? doc.QuerySelector(config.TitlePath)?.InnerText?.Trim() : null;

            return new ArticleContent
            {
                Title = title ?? definition.Title ?? definition.Url.ToString(),
                Text = article
            };
        }
    }
}
