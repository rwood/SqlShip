using System;
using System.IO;
using System.Linq;
using Autofac;
using SqlShip.Helpers;
using SqlShip.Interfaces;
using SqlShip.Logging;
using SqlShip.Services;
using Topshelf;

namespace SqlShip
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var container = ContainerBuilder(args);
            using (var scope = container.BeginLifetimeScope())
            {
                var fileWriter = scope.Resolve<LogFileWriter>();
                fileWriter.Subscribe(LogLevel.Trace);
                if (scope.IsRegistered<IUpdatePackager>() && scope.Resolve<IUpdatePackager>().Package()) return;
                var rc = HostFactory.Run(x =>
                {
                    x.Service<DataUploaderService>(s =>
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        s.ConstructUsing(name => scope.Resolve<DataUploaderService>());
                        s.WhenStarted((server, control) => server.Start(control));
                        s.WhenStopped(tc => tc.Stop());
                    });
                    
                    x.EnableServiceRecovery(r =>
                    {
                        r.RestartService(0);
                        r.SetResetPeriod(1);
                    });
                    x.RunAsLocalService();

                    x.SetDescription("SqlShip sends SQL Query results to a REST endpoint.");
                    x.SetDisplayName("SqlShip");
                    x.SetServiceName("SqlShip");
                    x.StartAutomatically();
                });
                var exitCode = (int) Convert.ChangeType(rc, rc.GetTypeCode());
                Environment.ExitCode = exitCode;
            }
        }

        private static IContainer ContainerBuilder(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<LogBroker>().SingleInstance();
            builder.RegisterType<Logger>().As<ILogger>();
            builder.RegisterType<LogFileWriter>().SingleInstance();
            builder.RegisterType<DataUploaderService>();
            var i = args.ToList().IndexOf("--pack");
            if (i >= 0)
            {
                var source = args[i + 1];
                var destination = args[i + 2];
                builder.RegisterInstance(new UpdatePackagerConfig
                {
                    DestinationDirectory = new DirectoryInfo(destination),
                    SourceDirectory = new DirectoryInfo(source)
                }).As<IUpdatePackagerConfig>();
                builder.RegisterType<UpdatePackager>().As<IUpdatePackager>();
            }

            builder.RegisterType<Settings>().As<IUpdaterConfig>();
            builder.RegisterType<AutoUpdateService>().As<IAutoUpdateService>();
            builder.RegisterType<HttpUpdaterService>().As<IUpdaterService>();
            builder.RegisterType<HighWaterMarkQueryService>();
            return builder.Build();
        }
    }
}