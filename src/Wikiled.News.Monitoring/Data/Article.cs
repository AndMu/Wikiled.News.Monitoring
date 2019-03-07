using System;

namespace Wikiled.News.Monitoring.Data
{
    public class Article
    {
        public Article(ArticleDefinition definition, CommentData[] comments, ArticleText articleText, DateTime dateTime)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            ArticleText = articleText ?? throw new ArgumentNullException(nameof(articleText));
            DateTime = dateTime;
            Comments = comments ?? throw new ArgumentNullException(nameof(comments));
        }

        public DateTime DateTime { get; }

        public ArticleDefinition Definition { get; }

        public CommentData[] Comments { get; }

        public ArticleText ArticleText { get; }
    }
}
