using System;
using System.Xml;

public static class XmlUpdater
{
    public static void ChangeValue(XmlDocument xmlDoc, string ruleSet, string rule, string name, string newState)
    {
        // Construct the XPath query to find the Rule element
        string query = $"//RuleSet/Rules[@AnalyzerId='{ruleSet}']/Rule[@Id='{rule}']";

        // Find the element using XPath
        XmlNode element = xmlDoc.SelectSingleNode(query);

        if (element != null)
        {
            // Get the existing 'Action' attribute value
            string existingValue = element.Attributes["Action"]?.Value;

            if (existingValue == newState)
            {
                Console.WriteLine($"Did not change '{name}' status as already '{newState}'");
            }
            else
            {
                // Set the new value to the 'Action' attribute
                element.Attributes["Action"].Value = newState;
                Console.WriteLine($"Updating '{name}' from '{existingValue}' to '{newState}'");
            }
        }
        else
        {
            Console.Error.WriteLine($"'{name}' is not present");
        }
    }
}