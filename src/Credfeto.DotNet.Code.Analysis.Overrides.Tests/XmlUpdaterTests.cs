using System;
using System.Xml;
using Credfeto.DotNet.Code.Analysis.Overrides;
using Credfeto.DotNet.Code.Analysis.Overrides.Models;
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
    public void ChangeValueReturnsRuleNotPresentWhenRuleNotPresent()
    {
        XmlDocument doc = CreateEmptyXmlDocument();

        RuleChangeOutcome outcome = doc.ChangeValue(
            ruleSet: "MyAnalyzer",
            rule: "MA0001",
            name: "Some Rule",
            newState: "Error",
            logger: this._logger
        );

        Assert.Equal(RuleChangeOutcome.RuleNotPresent, outcome);
    }

    [Fact]
    public void ChangeValueReturnsUnchangedWhenExistingActionIsIdentical()
    {
        XmlDocument doc = CreateXmlDocumentWithRule(ruleSet: "MyAnalyzer", rule: "MA0001", action: "Error");

        RuleChangeOutcome outcome = doc.ChangeValue(
            ruleSet: "MyAnalyzer",
            rule: "MA0001",
            name: "Some Rule",
            newState: "Error",
            logger: this._logger
        );

        Assert.Equal(RuleChangeOutcome.Unchanged, outcome);
    }

    [Fact]
    public void ChangeValueReturnsChangedWhenActionDiffers()
    {
        XmlDocument doc = CreateXmlDocumentWithRule(ruleSet: "MyAnalyzer", rule: "MA0001", action: "None");

        RuleChangeOutcome outcome = doc.ChangeValue(
            ruleSet: "MyAnalyzer",
            rule: "MA0001",
            name: "Some Rule",
            newState: "Error",
            logger: this._logger
        );

        Assert.Equal(RuleChangeOutcome.Changed, outcome);
    }

    [Fact]
    public void ChangeValueUpdatesActionAttributeWhenDiffers()
    {
        XmlDocument doc = CreateXmlDocumentWithRule(ruleSet: "MyAnalyzer", rule: "MA0001", action: "None");

        RuleChangeOutcome outcome = doc.ChangeValue(
            ruleSet: "MyAnalyzer",
            rule: "MA0001",
            name: "Some Rule",
            newState: "Warning",
            logger: this._logger
        );

        Assert.Equal(RuleChangeOutcome.Changed, outcome);

        XmlElement? element =
            doc.SelectSingleNode("//RuleSet/Rules[@AnalyzerId='MyAnalyzer']/Rule[@Id='MA0001']") as XmlElement;
        Assert.NotNull(element);
        Assert.Equal(expected: "Warning", actual: element.GetAttribute("Action"));
    }

    [Fact]
    public void ChangeValueReturnsRuleNotPresentWhenRuleSetDoesNotMatch()
    {
        XmlDocument doc = CreateXmlDocumentWithRule(ruleSet: "OtherAnalyzer", rule: "MA0001", action: "None");

        RuleChangeOutcome outcome = doc.ChangeValue(
            ruleSet: "MyAnalyzer",
            rule: "MA0001",
            name: "Some Rule",
            newState: "Error",
            logger: this._logger
        );

        Assert.Equal(RuleChangeOutcome.RuleNotPresent, outcome);
    }

    [Fact]
    public void ChangeValueReturnsRuleNotPresentWhenRuleIdDoesNotMatch()
    {
        XmlDocument doc = CreateXmlDocumentWithRule(ruleSet: "MyAnalyzer", rule: "MA9999", action: "None");

        RuleChangeOutcome outcome = doc.ChangeValue(
            ruleSet: "MyAnalyzer",
            rule: "MA0001",
            name: "Some Rule",
            newState: "Error",
            logger: this._logger
        );

        Assert.Equal(RuleChangeOutcome.RuleNotPresent, outcome);
    }

    [Fact]
    public void ChangeValueReturnsChangedAndSetsNewStateForDifferentAction()
    {
        XmlDocument doc = CreateXmlDocumentWithRule(ruleSet: "AnalyzerX", rule: "AX001", action: "Warning");

        RuleChangeOutcome outcome = doc.ChangeValue(
            ruleSet: "AnalyzerX",
            rule: "AX001",
            name: "Rule AX001",
            newState: "None",
            logger: this._logger
        );

        Assert.Equal(RuleChangeOutcome.Changed, outcome);

        XmlElement? element =
            doc.SelectSingleNode("//RuleSet/Rules[@AnalyzerId='AnalyzerX']/Rule[@Id='AX001']") as XmlElement;
        Assert.NotNull(element);
        Assert.Equal(expected: "None", actual: element.GetAttribute("Action"));
    }

    [Fact]
    public void ChangeValueReturnsRuleNotPresentForRuleSetContainingApostropheWithoutThrowing()
    {
        XmlDocument doc = CreateEmptyXmlDocument();

        RuleChangeOutcome outcome = doc.ChangeValue(
            ruleSet: "O'Brien.Analyzers",
            rule: "OB0001",
            name: "Some Rule",
            newState: "Error",
            logger: this._logger
        );

        Assert.Equal(RuleChangeOutcome.RuleNotPresent, outcome);
    }

    [Fact]
    public void ChangeValueMatchesRuleSetContainingApostrophe()
    {
        XmlDocument doc = CreateXmlDocumentWithRule(ruleSet: "O'Brien.Analyzers", rule: "OB0001", action: "None");

        RuleChangeOutcome outcome = doc.ChangeValue(
            ruleSet: "O'Brien.Analyzers",
            rule: "OB0001",
            name: "Some Rule",
            newState: "Error",
            logger: this._logger
        );

        Assert.Equal(RuleChangeOutcome.Changed, outcome);
    }

    [Fact]
    public void ChangeValueThrowsForUnsupportedStateAndDoesNotMutateDocument()
    {
        XmlDocument doc = CreateXmlDocumentWithRule(ruleSet: "MyAnalyzer", rule: "MA0001", action: "None");

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            doc.ChangeValue(
                ruleSet: "MyAnalyzer",
                rule: "MA0001",
                name: "Some Rule",
                newState: "Eror",
                logger: this._logger
            )
        );

        XmlElement? element =
            doc.SelectSingleNode("//RuleSet/Rules[@AnalyzerId='MyAnalyzer']/Rule[@Id='MA0001']") as XmlElement;
        Assert.NotNull(element);
        Assert.Equal(expected: "None", actual: element.GetAttribute("Action"));
    }

    [Fact]
    public void ChangeValueNormalizesErrorStateToCanonicalValue()
    {
        XmlDocument doc = CreateXmlDocumentWithRule(ruleSet: "MyAnalyzer", rule: "MA0001", action: "None");

        RuleChangeOutcome outcome = doc.ChangeValue(
            ruleSet: "MyAnalyzer",
            rule: "MA0001",
            name: "Some Rule",
            newState: "error",
            logger: this._logger
        );

        Assert.Equal(RuleChangeOutcome.Changed, outcome);

        XmlElement? element =
            doc.SelectSingleNode("//RuleSet/Rules[@AnalyzerId='MyAnalyzer']/Rule[@Id='MA0001']") as XmlElement;
        Assert.NotNull(element);
        Assert.Equal(expected: "Error", actual: element.GetAttribute("Action"));
    }

    [Fact]
    public void ChangeValueNormalizesWarningStateToCanonicalValue()
    {
        XmlDocument doc = CreateXmlDocumentWithRule(ruleSet: "MyAnalyzer", rule: "MA0001", action: "None");

        RuleChangeOutcome outcome = doc.ChangeValue(
            ruleSet: "MyAnalyzer",
            rule: "MA0001",
            name: "Some Rule",
            newState: "warning",
            logger: this._logger
        );

        Assert.Equal(RuleChangeOutcome.Changed, outcome);

        XmlElement? element =
            doc.SelectSingleNode("//RuleSet/Rules[@AnalyzerId='MyAnalyzer']/Rule[@Id='MA0001']") as XmlElement;
        Assert.NotNull(element);
        Assert.Equal(expected: "Warning", actual: element.GetAttribute("Action"));
    }

    [Fact]
    public void ChangeValueNormalizesInfoStateToCanonicalValue()
    {
        XmlDocument doc = CreateXmlDocumentWithRule(ruleSet: "MyAnalyzer", rule: "MA0001", action: "None");

        RuleChangeOutcome outcome = doc.ChangeValue(
            ruleSet: "MyAnalyzer",
            rule: "MA0001",
            name: "Some Rule",
            newState: "info",
            logger: this._logger
        );

        Assert.Equal(RuleChangeOutcome.Changed, outcome);

        XmlElement? element =
            doc.SelectSingleNode("//RuleSet/Rules[@AnalyzerId='MyAnalyzer']/Rule[@Id='MA0001']") as XmlElement;
        Assert.NotNull(element);
        Assert.Equal(expected: "Info", actual: element.GetAttribute("Action"));
    }

    [Fact]
    public void ChangeValueNormalizesHiddenStateToCanonicalValue()
    {
        XmlDocument doc = CreateXmlDocumentWithRule(ruleSet: "MyAnalyzer", rule: "MA0001", action: "None");

        RuleChangeOutcome outcome = doc.ChangeValue(
            ruleSet: "MyAnalyzer",
            rule: "MA0001",
            name: "Some Rule",
            newState: "hidden",
            logger: this._logger
        );

        Assert.Equal(RuleChangeOutcome.Changed, outcome);

        XmlElement? element =
            doc.SelectSingleNode("//RuleSet/Rules[@AnalyzerId='MyAnalyzer']/Rule[@Id='MA0001']") as XmlElement;
        Assert.NotNull(element);
        Assert.Equal(expected: "Hidden", actual: element.GetAttribute("Action"));
    }
}
