using System;
using System.Text;

namespace Wikiled.News.Monitoring.Data
{
    public class Article
    {
        public Article(ArticleDefinition definition, CommentData[] comments, ArticleContent articleContent, DateTime dateTime)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Content = articleContent ?? throw new ArgumentNullException(nameof(articleContent));
            DateTime = dateTime;
            Comments = comments ?? throw new ArgumentNullException(nameof(comments));
        }

        public void RefreshComments(CommentData[] comments)
        {
            Comments = comments;
        }

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public Language Language { get; set; } = Language.English;

        public DateTime DateTime { get; }

        public ArticleDefinition Definition { get; }

        public CommentData[] Comments { get; private set; }

        public ArticleContent Content { get; }

        public object Additional { get; set; }
    }
}
