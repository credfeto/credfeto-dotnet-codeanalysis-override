using System;
using System.Threading;
using System.Threading.Tasks;
using Cocona;
using Cocona.Builder;
using Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Cmd;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine($"{VersionInformation.Product} {VersionInformation.Version}");
        Console.WriteLine();

        using (CoconaApp host = CreateApp(args))
        {
            ILoggerFactory loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            Logging.InitializeLogging(loggerFactory: loggerFactory);

            host.AddCommands<Commands>();

            await host.RunAsync(CancellationToken.None);
        }
    }

    private static CoconaApp CreateApp(string[] args)
    {
        CoconaAppBuilder builder = CoconaApp.CreateBuilder(args);
        builder.Services.AddServices();
        builder
            .Logging.AddFilter(category: "Microsoft", level: LogLevel.Warning)
            .AddFilter(category: "System.Net.Http.HttpClient", level: LogLevel.Warning)
            .ClearProviders();

        return builder.Build();
    }
}
