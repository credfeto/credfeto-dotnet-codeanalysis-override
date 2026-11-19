using System;
using System.Threading;
using System.Threading.Tasks;
using ConsoleAppFramework;
using Microsoft.Extensions.Logging;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Setup;

internal sealed class UnhandledExceptionFilter : ConsoleAppFilter
{
    private readonly ILoggerFactory _loggerFactory;

    public UnhandledExceptionFilter(ConsoleAppFilter next, ILoggerFactory loggerFactory)
        : base(next)
    {
        this._loggerFactory = loggerFactory;
    }

    public override async Task InvokeAsync(ConsoleAppContext context, CancellationToken cancellationToken)
    {
        try
        {
            await this.Next.InvokeAsync(context: context, cancellationToken: cancellationToken);
        }
        catch (Exception exception) when (exception is not ArgumentParseFailedException)
        {
            // ConsoleAppFramework would otherwise print the full exception; keep the categorised log line instead.
            Environment.ExitCode = Program.HandleUnhandledException(
                exception: exception,
                loggerFactory: this._loggerFactory
            );
        }
    }
}
