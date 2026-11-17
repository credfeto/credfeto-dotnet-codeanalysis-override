using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cocona;
using Cocona.Application;
using Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Constants;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Setup;

internal static class SelfTest
{
    public const string Switch = "--self-test";

    public static bool IsRequested(IReadOnlyList<string> args)
    {
        return args.Contains(value: Switch, comparer: StringComparer.Ordinal);
    }

    public static async Task<int> RunAsync(
        Func<CoconaApp> createApp,
        TextWriter error,
        CancellationToken cancellationToken
    )
    {
        try
        {
            using (CoconaApp host = createApp())
            {
                ILoggerFactory loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
                Logging.InitializeLogging(loggerFactory: loggerFactory);

                Verify(host.Services);

                return ExitCodes.Success;
            }
        }
        catch (Exception exception)
        {
            // The caller (e.g. an image build) greps stderr, so the reason must stay on one line.
            string reason =
                $"Self-test failed: {exception.GetType().Name}: {exception.Message.ReplaceLineEndings(" ")}";
            await error.WriteLineAsync(buffer: reason.AsMemory(), cancellationToken: cancellationToken);

            return ExitCodes.Error;
        }
    }

    public static void Verify(IServiceProvider services)
    {
        _ = services.GetRequiredService<ILoggerFactory>();
        _ = services.GetRequiredService<ICoconaAppContextAccessor>();
        _ = ActivatorUtilities.CreateInstance<Commands>(services);

        // Loads the core library without reading or writing any file.
        _ = IniFile.Create();
    }
}
