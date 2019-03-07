using System;
using System.Xml.Linq;
using Wikiled.News.Monitoring.Feeds;

namespace Wikiled.News.Monitoring.Data
{
    public class ArticleDefinition
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public DateTime? Date { get; set; }

        public string Topic { get; set; }

        public Uri Url { get; set; }

        public FeedName Feed { get; set; }

        public XElement Element { get; set; }

        public override string ToString()
        {
            return $"Article: {Id}";
        }
    }
}
