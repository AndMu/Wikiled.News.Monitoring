using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Wikiled.Common.Extensions;
using Wikiled.Common.Helpers;
using Wikiled.News.Monitoring.Config;
using Wikiled.News.Monitoring.Data;

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
                var currentPath = Path.Combine(config.Location, article.Definition.Feed.Category);
                var file = Path.Combine(currentPath, $"{article.Definition.Title.CreateLetterText()}_{article.Definition.Id}.zip");
                var data = output.ZipAsTextFile($"{article.Definition.Title.CreateLetterText()}.json");
                lock (syncRoot)
                {
                    if (!File.Exists(file))
                    {
                        currentPath.EnsureDirectoryExistence();
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
