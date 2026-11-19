using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Setup;

// ConsoleAppFramework can only declare short aliases and help text through XML doc comments, which this
// repository does not allow, so the aliases are expanded and the extra help is written here instead.
internal static class CommandArguments
{
    public const string ShortHelp = "-h";
    public const string LongHelp = "--help";

    private const string EndOfOptions = "--";

    private static readonly FrozenDictionary<string, string> Aliases = new Dictionary<string, string>(
        StringComparer.Ordinal
    )
    {
        ["-r"] = "--ruleset",
        ["-c"] = "--changes",
    }.ToFrozenDictionary(StringComparer.Ordinal);

    private static readonly FrozenSet<string> ValueOptions = Aliases.Values.ToFrozenSet(StringComparer.Ordinal);

    private static readonly FrozenSet<string> HelpSwitches = new[] { ShortHelp, LongHelp }.ToFrozenSet(
        StringComparer.Ordinal
    );

    private static readonly FrozenSet<string> CommandsWithAliases = new[]
    {
        Commands.Ruleset,
        Commands.GlobalConfig,
    }.ToFrozenSet(StringComparer.Ordinal);

    private static readonly string RootHelp = string.Join(
        separator: Environment.NewLine,
        "Switches:",
        $"  {SelfTest.Switch}    Check the tool starts without doing any work (same as the {Commands.SelfTestCommand} command).",
        ""
    );

    private static readonly string CommandHelp = string.Join(
        separator: Environment.NewLine,
        "Aliases:",
        "  -r    Same as --ruleset.",
        "  -c    Same as --changes.",
        ""
    );

    public static string[] ExpandAliases(IReadOnlyList<string> args)
    {
        string[] expanded = [.. args];

        // Index 0 is the command name and a value never needs expanding, so only option names are considered.
        int index = 1;

        while (index < expanded.Length && !StringComparer.Ordinal.Equals(x: expanded[index], y: EndOfOptions))
        {
            if (Aliases.TryGetValue(key: expanded[index], out string? option))
            {
                expanded[index] = option;
            }

            index += ValueOptions.Contains(expanded[index]) ? 2 : 1;
        }

        return expanded;
    }

    public static async Task WriteHelpExtrasAsync(
        IReadOnlyList<string> args,
        TextWriter output,
        CancellationToken cancellationToken
    )
    {
        string? extra = args switch
        {
            [] => RootHelp,
            [string help] when HelpSwitches.Contains(help) => RootHelp,
            [string command, string help] when CommandsWithAliases.Contains(command) && HelpSwitches.Contains(help) =>
                CommandHelp,
            _ => null,
        };

        if (extra is not null)
        {
            await output.WriteLineAsync(buffer: extra.AsMemory(), cancellationToken: cancellationToken);
        }
    }
}
