using Wikiled.News.Monitoring.Data;

namespace Wikiled.News.Monitoring.Readers
{
    public class NulIDefinitionTransformer : IDefinitionTransformer
    {
        public ArticleDefinition Transform(ArticleDefinition definition)
        {
            return definition;
        }
    }
}
