﻿using System;
using System.Net;
using Autofac;
using Wikiled.Common.Net.Resilience;
using Wikiled.News.Monitoring.Retriever;

namespace Wikiled.News.Monitoring.Containers
{
    public class NewsRetrieverModule : Module
    {
        private readonly RetrieveConfiguration configuration;

        public NewsRetrieverModule(RetrieveConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(configuration).As<IResilienceConfig>().AsSelf();
            builder.RegisterType<CommonResilience>().As<IResilience>();
            builder.RegisterInstance(IPAddress.Any);
            builder.RegisterType<SimpleDataRetriever>().As<IDataRetriever>();
            builder.RegisterType<IPHandler>().As<IIPHandler>().SingleInstance();
        }
    }
}
