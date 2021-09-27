using HtmlAgilityPack;
using Wikiled.News.Monitoring.Data;

namespace Wikiled.News.Monitoring.Readers.Parsers
{
    public interface IPageParser
    {
        string Name { get; }

        ArticleContent Parse(ArticleDefinition definition, HtmlDocument document);
    }
}
