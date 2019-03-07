using System;

namespace Wikiled.News.Monitoring.Data
{
    public class CommentData
    {
        public string Id { get; set; }

        public string Author { get; set; }

        public string AuthorId { get; set; }

        public bool IsSpecialAuthor { get; set; }

        public string Text { get; set; }

        public DateTime Date { get; set; }

        public double Vote { get; set; }

        public override string ToString()
        {
            return $"{Author}({Date}) - {Text}";
        }
    }
}
