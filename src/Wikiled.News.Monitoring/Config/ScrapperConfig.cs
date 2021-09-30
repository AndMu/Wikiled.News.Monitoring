namespace Wikiled.News.Monitoring.Config
{
    public class ScrapperConfig
    {
        public MonitoringConfig Monitoring { get; set; }

        public ParsingConfig Parsers { get; set; }

        public PersistencyConfig Persistency { get; set; }
    }
}
