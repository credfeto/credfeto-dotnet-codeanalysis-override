using System;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini;
using FunFair.Test.Common;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Tests;

public sealed class IniUpdaterTests : IntegrationTestBase
{
    private readonly ILogger<IniUpdaterTests> _logger;

    public IniUpdaterTests(ITestOutputHelper output)
        : base(output)
    {
        this._logger = this.GetTypedLogger<IniUpdaterTests>();
    }

    [Theory]
    [InlineData("ERROR", "error")]
    [InlineData("error", "error")]
    [InlineData("WARNING", "suggestion")]
    [InlineData("warning", "suggestion")]
    [InlineData("INFO", "suggestion")]
    [InlineData("info", "suggestion")]
    [InlineData("NONE", "none")]
    [InlineData("none", "none")]
    public void ChangeValueAddsNewKeyWithConvertedState(string inputState, string expectedState)
    {
        ISettings settings = IniFile.Create();

        bool changed = settings.ChangeValue(
            ruleSet: "MyAnalyzer",
            rule: "MA0001",
            name: "Some Rule",
            newState: inputState,
            logger: this._logger
        );

        Assert.False(changed, "Should not report a change when adding a new key");

        string? value = settings.Get("dotnet_diagnostic.MA0001.severity");
        Assert.Equal(expected: expectedState, actual: value);
    }

    [Fact]
    public void ChangeValueReturnsFalseWhenKeyNotPresent()
    {
        ISettings settings = IniFile.Create();

        bool changed = settings.ChangeValue(
            ruleSet: "MyAnalyzer",
            rule: "MA0001",
            name: "Some Rule",
            newState: "ERROR",
            logger: this._logger
        );

        Assert.False(changed, "Should return false when key is not present");
    }

    [Fact]
    public void ChangeValueReturnsFalseWhenExistingValueIsIdentical()
    {
        ISettings settings = IniFile.Create();
        settings.Set(key: "dotnet_diagnostic.MA0001.severity", value: "error");

        bool changed = settings.ChangeValue(
            ruleSet: "MyAnalyzer",
            rule: "MA0001",
            name: "Some Rule",
            newState: "ERROR",
            logger: this._logger
        );

        Assert.False(changed, "Should return false when existing value is identical");
    }

    [Fact]
    public void ChangeValueReturnsTrueWhenExistingValueDiffers()
    {
        ISettings settings = IniFile.Create();
        settings.Set(key: "dotnet_diagnostic.MA0001.severity", value: "none");

        bool changed = settings.ChangeValue(
            ruleSet: "MyAnalyzer",
            rule: "MA0001",
            name: "Some Rule",
            newState: "ERROR",
            logger: this._logger
        );

        Assert.True(changed, "Should return true when existing value differs");

        string? value = settings.Get("dotnet_diagnostic.MA0001.severity");
        Assert.Equal(expected: "error", actual: value);
    }

    [Fact]
    public void ChangeValueUpdatesValueWhenExistingValueDiffers()
    {
        ISettings settings = IniFile.Create();
        settings.Set(key: "dotnet_diagnostic.MA0002.severity", value: "suggestion");

        bool changed = settings.ChangeValue(
            ruleSet: "AnotherAnalyzer",
            rule: "MA0002",
            name: "Another Rule",
            newState: "NONE",
            logger: this._logger
        );

        Assert.True(changed, "Should return true when value is updated");

        string? value = settings.Get("dotnet_diagnostic.MA0002.severity");
        Assert.Equal(expected: "none", actual: value);
    }

    [Fact]
    public void ChangeValueThrowsArgumentOutOfRangeForUnsupportedState()
    {
        ISettings settings = IniFile.Create();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            settings.ChangeValue(
                ruleSet: "MyAnalyzer",
                rule: "MA0001",
                name: "Some Rule",
                newState: "INVALID_STATE",
                logger: this._logger
            )
        );
    }

    [Fact]
    public void ChangeValueUsesCorrectKeyFormat()
    {
        ISettings settings = IniFile.Create();

        bool changed = settings.ChangeValue(
            ruleSet: "MyAnalyzer",
            rule: "CS0001",
            name: "Some Rule",
            newState: "WARNING",
            logger: this._logger
        );

        Assert.False(changed, "Should return false when adding a new key");

        string? value = settings.Get("dotnet_diagnostic.CS0001.severity");
        Assert.NotNull(value);
    }

    [Fact]
    public void ChangeValueAddsBlockAndLineCommentWhenKeyNotPresent()
    {
        ISettings settings = IniFile.Create();

        bool changed = settings.ChangeValue(
            ruleSet: "MyAnalyzer",
            rule: "MA0001",
            name: "Some Rule",
            newState: "ERROR",
            logger: this._logger
        );

        Assert.False(changed, "Should return false when adding a new key");

        string saved = settings.Save();
        Assert.Contains("MA0001", saved, StringComparison.Ordinal);
    }
}
