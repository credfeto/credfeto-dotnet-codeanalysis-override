using System;
using System.Xml;
using Credfeto.DotNet.Code.Analysis.Overrides.LoggingExtensions;
using Credfeto.DotNet.Code.Analysis.Overrides.Models;
using Microsoft.Extensions.Logging;

namespace Credfeto.DotNet.Code.Analysis.Overrides;

public static class XmlUpdater
{
    public static RuleChangeOutcome ChangeValue(
        this XmlDocument xmlRuleSet,
        string ruleSet,
        string rule,
        string name,
        string newState,
        ILogger logger
    )
    {
        string state = ConvertState(newState);

        XmlElement? element = FindRuleElement(xmlRuleSet: xmlRuleSet, ruleSet: ruleSet, rule: rule);

        if (element is null)
        {
            logger.RuleNotPresent(ruleSet: ruleSet, rule: rule, name: name);

            return RuleChangeOutcome.RuleNotPresent;
        }

        string existingValue = element.GetAttribute("Action");

        if (StringComparer.Ordinal.Equals(x: existingValue, y: state))
        {
            logger.RuleNotChangedAsIdentical(ruleSet: ruleSet, rule: rule, name: name, setting: existingValue);

            return RuleChangeOutcome.Unchanged;
        }

        element.SetAttribute(name: "Action", value: state);
        logger.RuleChanged(ruleSet: ruleSet, rule: rule, name: name, existingSetting: existingValue, newSetting: state);

        return RuleChangeOutcome.Changed;
    }

    private static XmlElement? FindRuleElement(XmlDocument xmlRuleSet, string ruleSet, string rule)
    {
        XmlNodeList? rulesNodes = xmlRuleSet.SelectNodes("//RuleSet/Rules");

        if (rulesNodes is null)
        {
            return null;
        }

        foreach (XmlNode rulesNode in rulesNodes)
        {
            if (
                rulesNode is not XmlElement rulesElement
                || !StringComparer.Ordinal.Equals(rulesElement.GetAttribute("AnalyzerId"), ruleSet)
            )
            {
                continue;
            }

            foreach (XmlNode ruleNode in rulesElement.ChildNodes)
            {
                if (
                    ruleNode is not XmlElement ruleElement
                    || !StringComparer.Ordinal.Equals(ruleElement.Name, "Rule")
                    || !StringComparer.Ordinal.Equals(ruleElement.GetAttribute("Id"), rule)
                )
                {
                    continue;
                }

                return ruleElement;
            }
        }

        return null;
    }

    private static string ConvertState(string newState)
    {
        return newState.ToUpperInvariant() switch
        {
            "ERROR" => "Error",
            "WARNING" => "Warning",
            "INFO" => "Info",
            "HIDDEN" => "Hidden",
            "NONE" => "None",
            _ => throw new ArgumentOutOfRangeException(
                nameof(newState),
                actualValue: newState,
                message: "Unsupported state"
            ),
        };
    }
}
