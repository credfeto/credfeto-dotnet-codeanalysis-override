using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Cocona;
using Cocona.Builder;
using Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Constants;
using Credfeto.DotNet.Code.Analysis.Overrides.Cmd.LoggingExtensions;
using Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Cmd;

internal static class Program
{
    internal static async Task Main(string[] args)
    {
        Console.WriteLine($"{VersionInformation.Product} {VersionInformation.Version}");
        Console.WriteLine();

        using (CoconaApp host = CreateApp(args))
        {
            ILoggerFactory loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            Logging.InitializeLogging(loggerFactory: loggerFactory);

            host.AddCommands<Commands>();

            await RunGuardedAsync(run: () => host.RunAsync(CancellationToken.None), loggerFactory: loggerFactory);
        }
    }

    internal static async Task RunGuardedAsync(Func<Task> run, ILoggerFactory loggerFactory)
    {
        try
        {
            await run();
        }
        catch (Exception exception)
        {
            Environment.ExitCode = HandleUnhandledException(exception: exception, loggerFactory: loggerFactory);
        }
    }

    internal static int HandleUnhandledException(Exception exception, ILoggerFactory loggerFactory)
    {
        // Program is a static class so cannot be the type argument of ILogger<T>.
        ILogger logger = loggerFactory.CreateLogger(categoryName: nameof(Program));

        return HandleUnhandledException(exception: exception, logger: logger);
    }

    internal static int HandleUnhandledException(Exception exception, ILogger logger)
    {
        switch (exception)
        {
            case OperationCanceledException:
                logger.OperationCancelled(exception.Message);

                break;
            case IOException:
                logger.FileSystemError(exception.Message);

                break;
            case UnauthorizedAccessException:
                logger.AccessDenied(exception.Message);

                break;
            case XmlException:
                logger.InvalidXml(exception.Message);

                break;
            default:
                logger.UnhandledError(exception.Message);

                break;
        }

        return ExitCodes.Error;
    }

    internal static CoconaApp CreateApp(string[] args)
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
