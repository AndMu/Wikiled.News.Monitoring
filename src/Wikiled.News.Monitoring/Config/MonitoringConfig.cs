namespace Wikiled.News.Monitoring.Config
{
    public class MonitoringConfig
    {
        /// <summary>
        /// In minutes
        /// </summary>
        public int ScanTime{ get; set; }

        public int DaysCutOff { get; set; }

        public int KeepDays { get; set; }
    }
}
