using Wikiled.News.Monitoring.Data;

namespace Wikiled.News.Monitoring.Readers
{
    public interface IDefinitionTransformer
    {
        ArticleDefinition Transform(ArticleDefinition definition);
    }
}