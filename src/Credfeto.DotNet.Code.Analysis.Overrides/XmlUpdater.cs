using System;
using System.Xml;

namespace Credfeto.DotNet.Code.Analysis.Overrides;

public static class XmlUpdater
{
    public static void ChangeValue(this XmlDocument xmlRuleSet, string ruleSet, string rule, string name, string newState)
    {
        // Construct the XPath query to find the Rule element
        string query = $"//RuleSet/Rules[@AnalyzerId='{ruleSet}']/Rule[@Id='{rule}']";

        // Find the element using XPath
        XmlElement? element = xmlRuleSet.SelectSingleNode(query) as XmlElement;

        if (element is null)
        {
            Console.Error.WriteLine($"'{name}' is not present");

            return;
        }

        // Get the existing 'Action' attribute value
        string existingValue = element.GetAttribute("Action");

        if (StringComparer.Ordinal.Equals(existingValue, newState))
        {
            Console.WriteLine($"Did not change '{name}' status as already '{newState}'");
        }
        else
        {
            // Set the new value to the 'Action' attribute
            element.SetAttribute("Action",  newState);
            Console.WriteLine($"Updating '{name}' from '{existingValue}' to '{newState}'");
        }
    }
}