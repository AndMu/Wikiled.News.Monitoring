using HtmlAgilityPack;
using System;
using Wikiled.News.Monitoring.Data;

namespace Wikiled.News.Monitoring.Readers.Parsers
{
    public class DefaultPageParser : IPageParser
    {
        public string Name => "Default";

        public ArticleContent Parse(ArticleDefinition definition, HtmlDocument document)
        {
            var doc = document.DocumentNode.InnerText;

            return new ArticleContent
            {
                Title = definition.Title ?? definition.Url.ToString(),
                Text = doc
            };
        }
    }
}
