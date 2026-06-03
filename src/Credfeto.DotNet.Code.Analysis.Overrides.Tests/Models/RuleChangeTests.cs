using Credfeto.DotNet.Code.Analysis.Overrides.Models;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Tests.Models;

public sealed class RuleChangeTests : IntegrationTestBase
{
    public RuleChangeTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ConstructorSetsRuleSet()
    {
        RuleChange ruleChange = new(
            ruleSet: "MyAnalyzer",
            rule: "MA0001",
            state: "ERROR",
            description: "Some description"
        );

        Assert.Equal(expected: "MyAnalyzer", actual: ruleChange.RuleSet);
    }

    [Fact]
    public void ConstructorSetsRule()
    {
        RuleChange ruleChange = new(
            ruleSet: "MyAnalyzer",
            rule: "MA0001",
            state: "ERROR",
            description: "Some description"
        );

        Assert.Equal(expected: "MA0001", actual: ruleChange.Rule);
    }

    [Fact]
    public void ConstructorSetsState()
    {
        RuleChange ruleChange = new(
            ruleSet: "MyAnalyzer",
            rule: "MA0001",
            state: "ERROR",
            description: "Some description"
        );

        Assert.Equal(expected: "ERROR", actual: ruleChange.State);
    }

    [Fact]
    public void ConstructorSetsDescription()
    {
        RuleChange ruleChange = new(
            ruleSet: "MyAnalyzer",
            rule: "MA0001",
            state: "ERROR",
            description: "Some description"
        );

        Assert.Equal(expected: "Some description", actual: ruleChange.Description);
    }

    [Theory]
    [InlineData("AnalyzerA", "RULE001", "WARNING", "First rule")]
    [InlineData("AnalyzerB", "RULE002", "NONE", "Second rule")]
    [InlineData("AnalyzerC", "RULE003", "INFO", "Third rule")]
    public void AllPropertiesAreSetFromConstructor(string ruleSet, string rule, string state, string description)
    {
        RuleChange ruleChange = new(ruleSet: ruleSet, rule: rule, state: state, description: description);

        Assert.Equal(expected: ruleSet, actual: ruleChange.RuleSet);
        Assert.Equal(expected: rule, actual: ruleChange.Rule);
        Assert.Equal(expected: state, actual: ruleChange.State);
        Assert.Equal(expected: description, actual: ruleChange.Description);
    }
}
