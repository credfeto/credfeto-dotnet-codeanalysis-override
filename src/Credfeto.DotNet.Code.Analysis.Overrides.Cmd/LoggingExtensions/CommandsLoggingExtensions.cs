using Microsoft.Extensions.Logging;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Cmd.LoggingExtensions;

internal static partial class CommandsLoggingExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "No changes in {fileName}")]
    public static partial void NoChangesInFile(this ILogger<Commands> logger, string fileName);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Changing {ruleSet}/{rule} to {state}")]
    public static partial void ChangingState(this ILogger<Commands> logger, string ruleSet, string rule, string state);
}
