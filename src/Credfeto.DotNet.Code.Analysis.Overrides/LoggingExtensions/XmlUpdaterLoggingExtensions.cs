using Microsoft.Extensions.Logging;

namespace Credfeto.DotNet.Code.Analysis.Overrides.LoggingExtensions;

internal static partial class XmlUpdaterLoggingExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Rule {ruleSet}/{rule} ({name}) is not configured in ruleset")]
    public static partial void RuleNotPresent(this ILogger logger, string ruleSet, string rule, string name);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Rule {ruleSet}/{rule} ({name}) is not configured in ruleset: Adding as {setting}")]
    public static partial void RuleNotPresentAdding(this ILogger logger, string ruleSet, string rule, string name, string setting);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Rule {ruleSet}/{rule} ({name}) already set to {setting}")]
    public static partial void RuleNotChangedAsIdentical(this ILogger logger, string ruleSet, string rule, string name, string setting);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Rule {ruleSet}/{rule} ({name}) changing from {existingSetting} to {newSetting}")]
    public static partial void RuleChanged(this ILogger logger, string ruleSet, string rule, string name, string existingSetting, string newSetting);
}