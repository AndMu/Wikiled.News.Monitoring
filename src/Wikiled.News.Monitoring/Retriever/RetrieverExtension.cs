using HtmlAgilityPack;

namespace Wikiled.News.Monitoring.Retriever
{
    public static class RetrieverExtension
    {
        public static HtmlDocument GetDocument(this string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);
            return document;
        }
    }
}
