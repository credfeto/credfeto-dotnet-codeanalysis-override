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
        XmlElement? element =
            xmlRuleSet.SelectSingleNode($"//RuleSet/Rules[@AnalyzerId='{ruleSet}']/Rule[@Id='{rule}']") as XmlElement;

        if (element is null)
        {
            logger.RuleNotPresent(ruleSet: ruleSet, rule: rule, name: name);

            return RuleChangeOutcome.RuleNotPresent;
        }

        string existingValue = element.GetAttribute("Action");

        if (StringComparer.Ordinal.Equals(x: existingValue, y: newState))
        {
            logger.RuleNotChangedAsIdentical(ruleSet: ruleSet, rule: rule, name: name, setting: existingValue);

            return RuleChangeOutcome.Unchanged;
        }

        element.SetAttribute(name: "Action", value: newState);
        logger.RuleChanged(
            ruleSet: ruleSet,
            rule: rule,
            name: name,
            existingSetting: existingValue,
            newSetting: newState
        );

        return RuleChangeOutcome.Changed;
    }
}
