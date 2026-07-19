using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Constants;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Tests;

public sealed class CommandsTests : IntegrationTestBase
{
    public CommandsTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task UpdateRulesetAsync_WithEmptyChanges_ReturnsWithoutReadingRuleset()
    {
        Commands commands = new(this.GetTypedLogger<Commands>());
        CancellationToken cancellationToken = this.CancellationToken();
        string changesFile = await WriteJsonTempAsync(content: "[]", cancellationToken: cancellationToken);

        try
        {
            int exitCode = await commands.UpdateRulesetAsync(
                rulesetFileName: "non-existent.ruleset",
                changesFileName: changesFile
            );

            Assert.Equal(ExitCodes.Success, exitCode);
        }
        finally
        {
            DeleteIfExists(changesFile);
        }
    }

    [Fact]
    public async Task UpdateRulesetAsync_WithChangesNoMatchingRule_DoesNotSaveAndReturnsErrorExitCode()
    {
        Commands commands = new(this.GetTypedLogger<Commands>());
        CancellationToken cancellationToken = this.CancellationToken();
        string changesFile = await WriteJsonTempAsync(
            content: """[{"ruleSet":"TestAnalyzer","rule":"TEST001","state":"Warning","description":"Test Rule"}]""",
            cancellationToken: cancellationToken
        );
        string rulesetFile = await WriteXmlTempAsync(
            content: """<RuleSet Name="Test" ToolsVersion="16.0"><Rules AnalyzerId="OtherAnalyzer"><Rule Id="OTHER001" Action="None" /></Rules></RuleSet>""",
            cancellationToken: cancellationToken
        );

        try
        {
            int exitCode = await commands.UpdateRulesetAsync(
                rulesetFileName: rulesetFile,
                changesFileName: changesFile
            );

            Assert.Equal(ExitCodes.Error, exitCode);
        }
        finally
        {
            DeleteIfExists(changesFile);
            DeleteIfExists(rulesetFile);
        }
    }

    [Fact]
    public async Task UpdateRulesetAsync_WithChangesMatchingRule_SavesUpdatedRulesetAndReturnsSuccessExitCode()
    {
        Commands commands = new(this.GetTypedLogger<Commands>());
        CancellationToken cancellationToken = this.CancellationToken();
        string changesFile = await WriteJsonTempAsync(
            content: """[{"ruleSet":"TestAnalyzer","rule":"TEST001","state":"Warning","description":"Test Rule"}]""",
            cancellationToken: cancellationToken
        );
        string rulesetFile = await WriteXmlTempAsync(
            content: """<RuleSet Name="Test" ToolsVersion="16.0"><Rules AnalyzerId="TestAnalyzer"><Rule Id="TEST001" Action="None" /></Rules></RuleSet>""",
            cancellationToken: cancellationToken
        );

        try
        {
            int exitCode = await commands.UpdateRulesetAsync(
                rulesetFileName: rulesetFile,
                changesFileName: changesFile
            );

            Assert.Equal(ExitCodes.Success, exitCode);

            string saved = await File.ReadAllTextAsync(path: rulesetFile, cancellationToken: cancellationToken);
            Assert.Contains("Warning", saved, StringComparison.Ordinal);
        }
        finally
        {
            DeleteIfExists(changesFile);
            DeleteIfExists(rulesetFile);
        }
    }

    [Fact]
    public async Task UpdateRulesetAsync_WithOneMatchingAndOneMissingRule_SavesAndReturnsErrorExitCode()
    {
        Commands commands = new(this.GetTypedLogger<Commands>());
        CancellationToken cancellationToken = this.CancellationToken();
        string changesFile = await WriteJsonTempAsync(
            content: """
            [
              {"ruleSet":"TestAnalyzer","rule":"TEST001","state":"Warning","description":"Test Rule"},
              {"ruleSet":"TestAnalyzer","rule":"MISSING001","state":"Warning","description":"Missing Rule"}
            ]
            """,
            cancellationToken: cancellationToken
        );
        string rulesetFile = await WriteXmlTempAsync(
            content: """<RuleSet Name="Test" ToolsVersion="16.0"><Rules AnalyzerId="TestAnalyzer"><Rule Id="TEST001" Action="None" /></Rules></RuleSet>""",
            cancellationToken: cancellationToken
        );

        try
        {
            int exitCode = await commands.UpdateRulesetAsync(
                rulesetFileName: rulesetFile,
                changesFileName: changesFile
            );

            Assert.Equal(ExitCodes.Error, exitCode);

            string saved = await File.ReadAllTextAsync(path: rulesetFile, cancellationToken: cancellationToken);
            Assert.Contains("Warning", saved, StringComparison.Ordinal);
        }
        finally
        {
            DeleteIfExists(changesFile);
            DeleteIfExists(rulesetFile);
        }
    }

    [Fact]
    public async Task UpdateGlobalConfigAsync_WithEmptyChanges_ReturnsWithoutReadingConfig()
    {
        Commands commands = new(this.GetTypedLogger<Commands>());
        CancellationToken cancellationToken = this.CancellationToken();
        string changesFile = await WriteJsonTempAsync(content: "[]", cancellationToken: cancellationToken);

        try
        {
            await commands.UpdateGlobalConfigAsync(
                rulesetFileName: "non-existent.globalconfig",
                changesFileName: changesFile
            );
        }
        finally
        {
            DeleteIfExists(changesFile);
        }
    }

    [Fact]
    public async Task UpdateGlobalConfigAsync_WithChangesExistingRuleSameState_DoesNotSave()
    {
        Commands commands = new(this.GetTypedLogger<Commands>());
        CancellationToken cancellationToken = this.CancellationToken();
        string changesFile = await WriteJsonTempAsync(
            content: """[{"ruleSet":"TestAnalyzer","rule":"TEST001","state":"Warning","description":"Test Rule"}]""",
            cancellationToken: cancellationToken
        );
        string configFile = await WriteTextTempAsync(
            content: "dotnet_diagnostic.TEST001.severity = suggestion\n",
            cancellationToken: cancellationToken
        );
        DateTime beforeTest = File.GetLastWriteTimeUtc(configFile);

        try
        {
            await commands.UpdateGlobalConfigAsync(rulesetFileName: configFile, changesFileName: changesFile);

            DateTime afterTest = File.GetLastWriteTimeUtc(configFile);
            Assert.Equal(expected: beforeTest, actual: afterTest);
        }
        finally
        {
            DeleteIfExists(changesFile);
            DeleteIfExists(configFile);
        }
    }

    [Fact]
    public async Task UpdateGlobalConfigAsync_WithChangesExistingRuleDifferentState_SavesUpdatedConfig()
    {
        Commands commands = new(this.GetTypedLogger<Commands>());
        CancellationToken cancellationToken = this.CancellationToken();
        string changesFile = await WriteJsonTempAsync(
            content: """[{"ruleSet":"TestAnalyzer","rule":"TEST001","state":"Error","description":"Test Rule"}]""",
            cancellationToken: cancellationToken
        );
        string configFile = await WriteTextTempAsync(
            content: "dotnet_diagnostic.TEST001.severity = suggestion\n",
            cancellationToken: cancellationToken
        );

        try
        {
            await commands.UpdateGlobalConfigAsync(rulesetFileName: configFile, changesFileName: changesFile);

            string saved = await File.ReadAllTextAsync(path: configFile, cancellationToken: cancellationToken);
            Assert.Contains("error", saved, StringComparison.Ordinal);
        }
        finally
        {
            DeleteIfExists(changesFile);
            DeleteIfExists(configFile);
        }
    }

    private static async ValueTask<string> WriteJsonTempAsync(string content, CancellationToken cancellationToken)
    {
        string path = Path.GetTempFileName();
        await File.WriteAllTextAsync(
            path: path,
            contents: content,
            encoding: Encoding.UTF8,
            cancellationToken: cancellationToken
        );

        return path;
    }

    private static async ValueTask<string> WriteXmlTempAsync(string content, CancellationToken cancellationToken)
    {
        string path = Path.ChangeExtension(Path.GetTempFileName(), "ruleset");
        await File.WriteAllTextAsync(
            path: path,
            contents: content,
            encoding: Encoding.UTF8,
            cancellationToken: cancellationToken
        );

        return path;
    }

    private static async ValueTask<string> WriteTextTempAsync(string content, CancellationToken cancellationToken)
    {
        string path = Path.ChangeExtension(Path.GetTempFileName(), "globalconfig");
        await File.WriteAllTextAsync(
            path: path,
            contents: content,
            encoding: Encoding.UTF8,
            cancellationToken: cancellationToken
        );

        return path;
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
