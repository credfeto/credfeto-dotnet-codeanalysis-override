using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using ConsoleAppFramework;
using Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Constants;
using Credfeto.DotNet.Code.Analysis.Overrides.Cmd.LoggingExtensions;
using Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Setup;
using Credfeto.DotNet.Code.Analysis.Overrides.Helpers;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini;
using Credfeto.DotNet.Code.Analysis.Overrides.Models;
using Microsoft.Extensions.Logging;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Cmd;

[SuppressMessage(
    category: "Microsoft.Performance",
    checkId: "CA1812:AvoidUninstantiatedInternalClasses",
    Justification = "Instantiated by ConsoleAppFramework"
)]
[SuppressMessage(
    category: "ReSharper",
    checkId: "ClassNeverInstantiated.Global",
    Justification = "Instantiated by ConsoleAppFramework"
)]
internal sealed class Commands
{
    public const string Ruleset = "ruleset";
    public const string GlobalConfig = "globalconfig";
    public const string SelfTestCommand = "self-test";

    // ConsoleAppFramework shows help for anything else without failing, so these are the only first arguments
    // that must not be reported as an unknown command.
    private static readonly FrozenSet<string> KnownFirstArguments = new[]
    {
        Ruleset,
        GlobalConfig,
        SelfTestCommand,
        CommandArguments.ShortHelp,
        CommandArguments.LongHelp,
        "--version",
    }.ToFrozenSet(StringComparer.Ordinal);

    private readonly ILogger<Commands> _logger;

    [SuppressMessage(
        category: "FunFair.CodeAnalysis",
        checkId: "FFS0023: Use ILogger rather than ILogger<T>",
        Justification = "Needed in this case"
    )]
    public Commands(ILogger<Commands> logger)
    {
        this._logger = logger;
    }

    public static bool IsUnknownCommand(IReadOnlyList<string> args)
    {
        return args is [string first, ..] && !KnownFirstArguments.Contains(first);
    }

    [Command(Ruleset)]
    [SuppressMessage(
        category: "ReSharper",
        checkId: "UnusedMember.Global",
        Justification = "Used by ConsoleAppFramework"
    )]
    public async Task<int> UpdateRulesetAsync(string ruleset, string changes, CancellationToken cancellationToken)
    {
        IReadOnlyList<RuleChange> changeSet = await ChangeSet.LoadAsync(
            changesFileName: changes,
            cancellationToken: cancellationToken
        );

        if (changeSet is [])
        {
            this._logger.NoChangesInFile(changes);

            return ExitCodes.Success;
        }

        bool changed = false;
        bool ruleNotPresent = false;
        XmlDocument ruleSet = await RuleSet.LoadAsync(fileName: ruleset, cancellationToken: cancellationToken);

        foreach (RuleChange change in changeSet)
        {
            this._logger.ChangingState(change.RuleSet, rule: change.Rule, change.State);
            RuleChangeOutcome outcome = ruleSet.ChangeValue(
                ruleSet: change.RuleSet,
                rule: change.Rule,
                name: change.Description,
                newState: change.State,
                logger: this._logger
            );
            changed |= outcome == RuleChangeOutcome.Changed;
            ruleNotPresent |= outcome == RuleChangeOutcome.RuleNotPresent;
        }

        if (changed)
        {
            await RuleSet.SaveAsync(project: ruleset, doc: ruleSet, cancellationToken: cancellationToken);
        }

        return ruleNotPresent ? ExitCodes.Error : ExitCodes.Success;
    }

    [Command(GlobalConfig)]
    [SuppressMessage(
        category: "ReSharper",
        checkId: "UnusedMember.Global",
        Justification = "Used by ConsoleAppFramework"
    )]
    public async Task<int> UpdateGlobalConfigAsync(string ruleset, string changes, CancellationToken cancellationToken)
    {
        IReadOnlyList<RuleChange> changeSet = await ChangeSet.LoadAsync(
            changesFileName: changes,
            cancellationToken: cancellationToken
        );

        if (changeSet is [])
        {
            this._logger.NoChangesInFile(changes);

            return ExitCodes.Success;
        }

        bool changed = false;
        ISettings settings = await IniFile.LoadAsync(fileName: ruleset, cancellationToken: cancellationToken);

        foreach (RuleChange change in changeSet)
        {
            this._logger.ChangingState(ruleSet: change.RuleSet, rule: change.Rule, state: change.State);
            bool hasChanged = settings.ChangeValue(
                ruleSet: change.RuleSet,
                rule: change.Rule,
                name: change.Description,
                newState: change.State,
                logger: this._logger
            );
            changed |= hasChanged;
        }

        if (changed)
        {
            await IniFile.SaveAsync(fileName: ruleset, settings: settings, cancellationToken: cancellationToken);
        }

        return ExitCodes.Success;
    }

    [Command(SelfTestCommand)]
    public int RunSelfTest([FromServices] IServiceProvider services)
    {
        SelfTest.Verify(services);
        this._logger.SelfTestPassed();

        return ExitCodes.Success;
    }
}
