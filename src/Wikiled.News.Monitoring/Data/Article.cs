using System;

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
            Language = Definition.Feed.Language;
        }

        public void RefreshComments(CommentData[] comments)
        {
            Comments = comments;
        }

        public string Language { get; set; }

        public DateTime DateTime { get; }

        public ArticleDefinition Definition { get; }

        public CommentData[] Comments { get; private set; }

        public ArticleContent Content { get; }

        public object Additional { get; set; }
    }
}
