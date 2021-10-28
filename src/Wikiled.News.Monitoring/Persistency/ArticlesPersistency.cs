using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Wikiled.Common.Extensions;
using Wikiled.News.Monitoring.Config;
using Wikiled.News.Monitoring.Data;
using Wikiled.News.Monitoring.Extensions;

namespace Wikiled.News.Monitoring.Persistency
{
    public class ArticlesPersistency : IArticlesPersistency
    {
        private readonly ILogger<ArticlesPersistency> logger;

        private readonly PersistencyConfig config;

        private readonly object syncRoot = new object();

        public ArticlesPersistency(ILogger<ArticlesPersistency> logger, PersistencyConfig config)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public Task<bool> Save(Article article)
        {
            try
            {
                logger.LogInformation("Saving: {0}", article.Definition.Title);
                var output = JsonConvert.SerializeObject(article, Formatting.Indented);
                var currentPath = article.Definition.Feed == null ? config.Location : Path.Combine(config.Location, article.Definition.Feed.Category);
                var file = Path.Combine(currentPath, article.Language ?? string.Empty, $"{article.Definition.Title.CreateLetterText()}_{DateTime.UtcNow.Ticks}.zip");
                var data = output.Zip($"{article.Definition.Title.CreateLetterText()}.json", Encoding.UTF8);
                lock (syncRoot)
                {
                    if (!File.Exists(file))
                    {
                        Path.GetDirectoryName(file).EnsureDirectoryExistence();
                        File.WriteAllBytes(file, data);
                    }
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed");
            }

            return Task.FromResult(false);
        }
    }
}
