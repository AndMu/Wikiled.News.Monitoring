using Wikiled.News.Monitoring.Readers;

namespace Wikiled.News.Monitoring.Config
{
    public class ScrapperConfig
    {
        public ParsingConfig Parsers { get; set; }

        public PersistencyConfig Persistency { get; set; }
    }
}
