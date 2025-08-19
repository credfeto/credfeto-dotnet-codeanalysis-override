using System;
using System.Xml;

namespace Credfeto.DotNet.Code.Analysis.Overrides;

public static class XmlUpdater
{
    public static bool ChangeValue(this XmlDocument xmlRuleSet, string ruleSet, string rule, string name, string newState)
    {
        XmlElement? element = xmlRuleSet.SelectSingleNode($"//RuleSet/Rules[@AnalyzerId='{ruleSet}']/Rule[@Id='{rule}']") as XmlElement;

        if (element is null)
        {
            Console.Error.WriteLine($"'{name}' is not present");

            return false;
        }

        string existingValue = element.GetAttribute("Action");

        if (StringComparer.Ordinal.Equals(x: existingValue, y: newState))
        {
            Console.WriteLine($"Did not change '{name}' status as already '{newState}'");

            return false;
        }

        element.SetAttribute(name: "Action", value: newState);
        Console.WriteLine($"Updating '{name}' from '{existingValue}' to '{newState}'");

        return true;
    }
}