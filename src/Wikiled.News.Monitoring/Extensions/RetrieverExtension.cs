using System;
using HtmlAgilityPack;

namespace Wikiled.News.Monitoring.Extensions
{
    public static class RetrieverExtension
    {
        public static HtmlDocument GetDocument(this string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(html));
            }

            var document = new HtmlDocument();
            document.LoadHtml(html);
            return document;
        }
    }
}
