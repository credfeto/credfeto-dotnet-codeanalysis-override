using System;
using System.IO;
using System.Threading.Tasks;
using Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Setup;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Tests.Setup;

public sealed class CommandArgumentsTests : IntegrationTestBase
{
    public CommandArgumentsTests(ITestOutputHelper output)
        : base(output) { }

    [Theory]
    [InlineData("", "")]
    [InlineData("ruleset -r a.ruleset -c b.json", "ruleset --ruleset a.ruleset --changes b.json")]
    [InlineData("globalconfig -c b.json -r a.globalconfig", "globalconfig --changes b.json --ruleset a.globalconfig")]
    [InlineData("ruleset --ruleset a.ruleset --changes b.json", "ruleset --ruleset a.ruleset --changes b.json")]
    [InlineData("ruleset -r -c -c -r", "ruleset --ruleset -c --changes -r")]
    [InlineData("ruleset --ruleset -r --changes -c", "ruleset --ruleset -r --changes -c")]
    [InlineData("ruleset -r a.ruleset -- -c", "ruleset --ruleset a.ruleset -- -c")]
    [InlineData("-r ruleset", "-r ruleset")]
    [InlineData("ruleset -R a.ruleset", "ruleset -R a.ruleset")]
    [InlineData("ruleset -r", "ruleset --ruleset")]
    public static void ExpandAliasesRewritesOnlyOptionNames(string commandLine, string expected)
    {
        string[] expanded = CommandArguments.ExpandAliases(Split(commandLine));

        Assert.Equal(expected: Split(expected), actual: expanded);
    }

    [Fact]
    public static void ExpandAliasesDoesNotModifyInput()
    {
        string[] args = ["ruleset", "-r", "a.ruleset"];

        _ = CommandArguments.ExpandAliases(args);

        Assert.Equal(expected: ["ruleset", "-r", "a.ruleset"], actual: args);
    }

    [Theory]
    [InlineData("")]
    [InlineData("-h")]
    [InlineData("--help")]
    public async Task WriteHelpExtrasForRootHelpListsSelfTestSwitchAsync(string commandLine)
    {
        string output = await this.CaptureHelpExtrasAsync(commandLine);

        Assert.Contains(
            expectedSubstring: SelfTest.Switch,
            actualString: output,
            comparisonType: StringComparison.Ordinal
        );
    }

    [Theory]
    [InlineData("ruleset -h")]
    [InlineData("ruleset --help")]
    [InlineData("globalconfig -h")]
    [InlineData("globalconfig --help")]
    public async Task WriteHelpExtrasForCommandHelpListsAliasesAsync(string commandLine)
    {
        string output = await this.CaptureHelpExtrasAsync(commandLine);

        Assert.Contains(
            expectedSubstring: "-r    Same as --ruleset.",
            actualString: output,
            comparisonType: StringComparison.Ordinal
        );
        Assert.Contains(
            expectedSubstring: "-c    Same as --changes.",
            actualString: output,
            comparisonType: StringComparison.Ordinal
        );
    }

    [Theory]
    [InlineData("ruleset -r a.ruleset -c b.json")]
    [InlineData("self-test --help")]
    [InlineData("ruleset")]
    [InlineData("--version")]
    [InlineData("--help ruleset")]
    public async Task WriteHelpExtrasWhenHelpNotRequestedWritesNothingAsync(string commandLine)
    {
        string output = await this.CaptureHelpExtrasAsync(commandLine);

        Assert.Empty(output);
    }

    private async Task<string> CaptureHelpExtrasAsync(string commandLine)
    {
        await using (StringWriter output = new())
        {
            await CommandArguments.WriteHelpExtrasAsync(
                args: Split(commandLine),
                output: output,
                cancellationToken: this.CancellationToken()
            );

            return output.ToString();
        }
    }

    private static string[] Split(string commandLine)
    {
        return commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }
}
