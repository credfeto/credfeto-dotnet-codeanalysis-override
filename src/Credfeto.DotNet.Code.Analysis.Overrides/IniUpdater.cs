using System;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini;
using Credfeto.DotNet.Code.Analysis.Overrides.LoggingExtensions;
using Microsoft.Extensions.Logging;

namespace Credfeto.DotNet.Code.Analysis.Overrides;

public static class IniUpdater
{
    public static bool ChangeValue(this ISection section, string ruleSet, string rule, string name, string newState, ILogger logger)
    {
        string key = $"dotnet_diagnostic.{rule}.severity";
        string state = ConvertState(newState);

        string? existingValue = section.Get(key);

        if (existingValue is null)
        {
            logger.RuleNotPresentAdding(ruleSet: ruleSet, rule: rule, name: name, setting: newState);

            section.Set(key: key, value: state);
            section.PropertyBlockComment(key: key, [$"{rule}: {name}"]);
            section.PropertyLineComment(key: key, $"Ruleset: {ruleSet}");

            return false;
        }

        if (StringComparer.Ordinal.Equals(x: existingValue, y: state))
        {
            logger.RuleNotChangedAsIdentical(ruleSet: ruleSet, rule: rule, name: name, setting: existingValue);

            return false;
        }

        section.Set(key: key, value: state);

        logger.RuleChanged(ruleSet: ruleSet, rule: rule, name: name, existingSetting: existingValue, newSetting: newState);

        return true;
    }

    private static string ConvertState(string newState)
    {
        return newState.ToUpperInvariant() switch
        {
            "ERROR" => "error",
            "WARNING" => "suggestion",
            "INFO" => "suggestion",
            "NONE" => "none",
            _ => throw new ArgumentOutOfRangeException(nameof(newState), actualValue: newState, message: "Unsupported state")
        };
    }
}