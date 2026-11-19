using Microsoft.Extensions.Logging;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Setup;

internal static class Logging
{
    public static ILoggingBuilder AddConsoleLogging(this ILoggingBuilder builder)
    {
        return builder.AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.TimestampFormat = "HH:mm:ss ";
        });
    }
}
