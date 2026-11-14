using Microsoft.Extensions.Logging;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Cmd.LoggingExtensions;

internal static partial class ProgramLoggingExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Operation cancelled: {message}")]
    public static partial void OperationCancelled(this ILogger logger, string message);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "File system error: {message}")]
    public static partial void FileSystemError(this ILogger logger, string message);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Access denied: {message}")]
    public static partial void AccessDenied(this ILogger logger, string message);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Invalid XML: {message}")]
    public static partial void InvalidXml(this ILogger logger, string message);

    [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "Unhandled failure: {message}")]
    public static partial void UnhandledError(this ILogger logger, string message);
}
