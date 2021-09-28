using Wikiled.News.Monitoring.Data;

namespace Wikiled.News.Monitoring.Readers
{
    public class NullDefinitionTransformer : IDefinitionTransformer
    {
        public ArticleDefinition Transform(ArticleDefinition definition)
        {
            return definition;
        }
    }
}
