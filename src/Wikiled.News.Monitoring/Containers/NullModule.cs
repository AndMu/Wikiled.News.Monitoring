﻿using Autofac;
using Wikiled.News.Monitoring.Readers;

namespace Wikiled.News.Monitoring.Containers
{
    public class NullModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<NullAuthentication>().As<IAuthentication>();
            builder.RegisterType<NullCommentsReader>().As<ICommentsReader>();
        }
    }
}
