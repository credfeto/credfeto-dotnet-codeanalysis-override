using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Constants;
using FunFair.Test.Common;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Tests;

public sealed class CommandsTests : IntegrationTestBase
{
    public CommandsTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public static void IsUnknownCommand_WithNoArguments_ReturnsFalse()
    {
        Assert.False(
            condition: Commands.IsUnknownCommand([]),
            userMessage: "No arguments should show help, not an error"
        );
    }

    [Fact]
    public static void IsUnknownCommand_WithKnownCommandFollowedByOptions_ReturnsFalse()
    {
        Assert.False(
            condition: Commands.IsUnknownCommand(["ruleset", "-r", "not-a-command", "-c", "--unknown"]),
            userMessage: "Only the first argument names the command"
        );
    }

    [Theory]
    [InlineData("ruleset", false)]
    [InlineData("globalconfig", false)]
    [InlineData("self-test", false)]
    [InlineData("-h", false)]
    [InlineData("--help", false)]
    [InlineData("--version", false)]
    [InlineData("not-a-command", true)]
    [InlineData("Ruleset", true)]
    [InlineData("--unknown", true)]
    public static void IsUnknownCommand_ChecksTheFirstArgument(string firstArgument, bool expected)
    {
        Assert.Equal(expected: expected, actual: Commands.IsUnknownCommand([firstArgument]));
    }

    [Fact]
    public void RunSelfTest_WithApplicationServices_ReturnsSuccessExitCode()
    {
        Commands commands = new(this.GetTypedLogger<Commands>());

        using (ServiceProvider services = Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Program.CreateServices())
        {
            int exitCode = commands.RunSelfTest(services);

            Assert.Equal(ExitCodes.Success, exitCode);
        }
    }

    [Fact]
    public async Task UpdateRulesetAsync_WithEmptyChanges_ReturnsWithoutReadingRuleset()
    {
        Commands commands = new(this.GetTypedLogger<Commands>());
        CancellationToken cancellationToken = this.CancellationToken();
        string changesFile = await WriteJsonTempAsync(content: "[]", cancellationToken: cancellationToken);

        try
        {
            int exitCode = await commands.UpdateRulesetAsync(
                ruleset: "non-existent.ruleset",
                changes: changesFile,
                cancellationToken: cancellationToken
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
                ruleset: rulesetFile,
                changes: changesFile,
                cancellationToken: cancellationToken
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
                ruleset: rulesetFile,
                changes: changesFile,
                cancellationToken: cancellationToken
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
                ruleset: rulesetFile,
                changes: changesFile,
                cancellationToken: cancellationToken
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
    public async Task UpdateRulesetAsync_WithCancelledCancellationToken_ThrowsOperationCanceledException()
    {
        Commands commands = new(this.GetTypedLogger<Commands>());
        CancellationToken cancellationToken = this.CancellationToken();
        string changesFile = await WriteJsonTempAsync(content: "[]", cancellationToken: cancellationToken);

        try
        {
            using CancellationTokenSource cancelledSource = new();
            await cancelledSource.CancelAsync();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                commands.UpdateRulesetAsync(
                    ruleset: "non-existent.ruleset",
                    changes: changesFile,
                    cancellationToken: cancelledSource.Token
                )
            );
        }
        finally
        {
            DeleteIfExists(changesFile);
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
            int exitCode = await commands.UpdateGlobalConfigAsync(
                ruleset: "non-existent.globalconfig",
                changes: changesFile,
                cancellationToken: cancellationToken
            );

            Assert.Equal(expected: ExitCodes.Success, actual: exitCode);
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
            int exitCode = await commands.UpdateGlobalConfigAsync(
                ruleset: configFile,
                changes: changesFile,
                cancellationToken: cancellationToken
            );

            DateTime afterTest = File.GetLastWriteTimeUtc(configFile);
            Assert.Equal(expected: ExitCodes.Success, actual: exitCode);
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
            int exitCode = await commands.UpdateGlobalConfigAsync(
                ruleset: configFile,
                changes: changesFile,
                cancellationToken: cancellationToken
            );

            string saved = await File.ReadAllTextAsync(path: configFile, cancellationToken: cancellationToken);
            Assert.Equal(expected: ExitCodes.Success, actual: exitCode);
            Assert.Contains("error", saved, StringComparison.Ordinal);
        }
        finally
        {
            DeleteIfExists(changesFile);
            DeleteIfExists(configFile);
        }
    }

    [Fact]
    public async Task UpdateGlobalConfigAsync_WithPureAdditions_SavesConfig()
    {
        Commands commands = new(this.GetTypedLogger<Commands>());
        CancellationToken cancellationToken = this.CancellationToken();
        string changesFile = await WriteJsonTempAsync(
            content: """[{"ruleSet":"TestAnalyzer","rule":"TEST002","state":"Warning","description":"Test Rule"}]""",
            cancellationToken: cancellationToken
        );
        string configFile = await WriteTextTempAsync(
            content: "dotnet_diagnostic.OTHER001.severity = suggestion\n",
            cancellationToken: cancellationToken
        );

        try
        {
            int exitCode = await commands.UpdateGlobalConfigAsync(
                ruleset: configFile,
                changes: changesFile,
                cancellationToken: cancellationToken
            );

            string saved = await File.ReadAllTextAsync(path: configFile, cancellationToken: cancellationToken);
            Assert.Equal(expected: ExitCodes.Success, actual: exitCode);
            Assert.Contains("dotnet_diagnostic.TEST002.severity", saved, StringComparison.Ordinal);
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
