using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using ConsoleAppFramework;
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

        if (SelfTest.IsRequested(args))
        {
            // The switch is not passed on, so nothing reaches the command dispatch.
            Environment.ExitCode = await SelfTest.RunAsync(
                createServices: CreateServices,
                error: Console.Error,
                cancellationToken: CancellationToken.None
            );

            return;
        }

        await using (ServiceProvider services = CreateServices())
        {
            ILoggerFactory loggerFactory = services.GetRequiredService<ILoggerFactory>();

            await RunGuardedAsync(
                run: () => RunCommandAsync(args: args, services: services),
                loggerFactory: loggerFactory
            );
        }
    }

    internal static async Task RunCommandAsync(string[] args, IServiceProvider services)
    {
        if (Commands.IsUnknownCommand(args))
        {
            Environment.ExitCode = ExitCodes.Error;
        }

        ConsoleApp.ServiceProvider = services;

        ConsoleApp.ConsoleAppBuilder app = ConsoleApp.Create();
        app.UseFilter<UnhandledExceptionFilter>();
        app.Add<Commands>();

        // The caller owns the service provider and disposes it after the run.
        await app.RunAsync(
            args: CommandArguments.ExpandAliases(args),
            disposeServiceProvider: false,
            cancellationToken: CancellationToken.None
        );
        await CommandArguments.WriteHelpExtrasAsync(
            args: args,
            output: Console.Out,
            cancellationToken: CancellationToken.None
        );
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

    internal static ServiceProvider CreateServices()
    {
        return new ServiceCollection()
            .AddLogging(builder =>
                builder
                    .AddFilter(category: "Microsoft", level: LogLevel.Warning)
                    .AddFilter(category: "System.Net.Http.HttpClient", level: LogLevel.Warning)
                    .ClearProviders()
                    .AddConsoleLogging()
            )
            .AddServices()
            .BuildServiceProvider();
    }
}
