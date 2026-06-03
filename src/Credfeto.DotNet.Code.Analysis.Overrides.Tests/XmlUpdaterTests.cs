using System.Xml;
using Credfeto.DotNet.Code.Analysis.Overrides;
using FunFair.Test.Common;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Tests;

public sealed class XmlUpdaterTests : IntegrationTestBase
{
    private readonly ILogger<XmlUpdaterTests> _logger;

    public XmlUpdaterTests(ITestOutputHelper output)
        : base(output)
    {
        this._logger = this.GetTypedLogger<XmlUpdaterTests>();
    }

    private static XmlDocument CreateXmlDocumentWithRule(string ruleSet, string rule, string action)
    {
        XmlDocument doc = new();
        doc.LoadXml(
            $@"<RuleSet>
  <Rules AnalyzerId=""{ruleSet}"">
    <Rule Id=""{rule}"" Action=""{action}"" />
  </Rules>
</RuleSet>"
        );

        return doc;
    }

    private static XmlDocument CreateEmptyXmlDocument()
    {
        XmlDocument doc = new();
        doc.LoadXml("<RuleSet></RuleSet>");

        return doc;
    }

    [Fact]
    public void ChangeValueReturnsFalseWhenRuleNotPresent()
    {
        XmlDocument doc = CreateEmptyXmlDocument();

        bool changed = doc.ChangeValue(
            ruleSet: "MyAnalyzer",
            rule: "MA0001",
            name: "Some Rule",
            newState: "error",
            logger: this._logger
        );

        Assert.False(changed, "Should return false when rule is not present");
    }

    [Fact]
    public void ChangeValueReturnsFalseWhenExistingActionIsIdentical()
    {
        XmlDocument doc = CreateXmlDocumentWithRule(ruleSet: "MyAnalyzer", rule: "MA0001", action: "error");

        bool changed = doc.ChangeValue(
            ruleSet: "MyAnalyzer",
            rule: "MA0001",
            name: "Some Rule",
            newState: "error",
            logger: this._logger
        );

        Assert.False(changed, "Should return false when existing action is identical");
    }

    [Fact]
    public void ChangeValueReturnsTrueWhenActionDiffers()
    {
        XmlDocument doc = CreateXmlDocumentWithRule(ruleSet: "MyAnalyzer", rule: "MA0001", action: "none");

        bool changed = doc.ChangeValue(
            ruleSet: "MyAnalyzer",
            rule: "MA0001",
            name: "Some Rule",
            newState: "error",
            logger: this._logger
        );

        Assert.True(changed, "Should return true when action differs");
    }

    [Fact]
    public void ChangeValueUpdatesActionAttributeWhenDiffers()
    {
        XmlDocument doc = CreateXmlDocumentWithRule(ruleSet: "MyAnalyzer", rule: "MA0001", action: "none");

        bool changed = doc.ChangeValue(
            ruleSet: "MyAnalyzer",
            rule: "MA0001",
            name: "Some Rule",
            newState: "suggestion",
            logger: this._logger
        );

        Assert.True(changed, "Should return true when action is updated");

        XmlElement? element =
            doc.SelectSingleNode("//RuleSet/Rules[@AnalyzerId='MyAnalyzer']/Rule[@Id='MA0001']") as XmlElement;
        Assert.NotNull(element);
        Assert.Equal(expected: "suggestion", actual: element.GetAttribute("Action"));
    }

    [Fact]
    public void ChangeValueDoesNotUpdateWhenRuleSetDoesNotMatch()
    {
        XmlDocument doc = CreateXmlDocumentWithRule(ruleSet: "OtherAnalyzer", rule: "MA0001", action: "none");

        bool changed = doc.ChangeValue(
            ruleSet: "MyAnalyzer",
            rule: "MA0001",
            name: "Some Rule",
            newState: "error",
            logger: this._logger
        );

        Assert.False(changed, "Should return false when ruleSet does not match");
    }

    [Fact]
    public void ChangeValueDoesNotUpdateWhenRuleIdDoesNotMatch()
    {
        XmlDocument doc = CreateXmlDocumentWithRule(ruleSet: "MyAnalyzer", rule: "MA9999", action: "none");

        bool changed = doc.ChangeValue(
            ruleSet: "MyAnalyzer",
            rule: "MA0001",
            name: "Some Rule",
            newState: "error",
            logger: this._logger
        );

        Assert.False(changed, "Should return false when rule ID does not match");
    }

    [Fact]
    public void ChangeValueReturnsTrueAndSetsNewStateForDifferentAction()
    {
        XmlDocument doc = CreateXmlDocumentWithRule(ruleSet: "AnalyzerX", rule: "AX001", action: "suggestion");

        bool changed = doc.ChangeValue(
            ruleSet: "AnalyzerX",
            rule: "AX001",
            name: "Rule AX001",
            newState: "none",
            logger: this._logger
        );

        Assert.True(changed, "Should return true when action is changed");

        XmlElement? element =
            doc.SelectSingleNode("//RuleSet/Rules[@AnalyzerId='AnalyzerX']/Rule[@Id='AX001']") as XmlElement;
        Assert.NotNull(element);
        Assert.Equal(expected: "none", actual: element.GetAttribute("Action"));
    }
}
