using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Wikiled.Common.Extensions;
using Wikiled.Common.Helpers;
using Wikiled.News.Monitoring.Data;

namespace Wikiled.News.Monitoring.Persistency
{
    public class ArticlesPersistency : IArticlesPersistency
    {
        private readonly ILogger<ArticlesPersistency> logger;

        private readonly string path;

        private readonly object syncRoot = new object();

        public ArticlesPersistency(ILogger<ArticlesPersistency> logger, string path)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.path = path;
        }

        public Task<bool> Save(Article article)
        {
            try
            {
                logger.LogInformation("Saving: {0}", article.Definition.Title);
                string output = JsonConvert.SerializeObject(article, Formatting.Indented);
                string currentPath = Path.Combine(path, article.Definition.Feed.Category);
                string file = Path.Combine(currentPath, $"{article.Definition.Title.CreateLetterText()}_{article.Definition.Id}.zip");
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
