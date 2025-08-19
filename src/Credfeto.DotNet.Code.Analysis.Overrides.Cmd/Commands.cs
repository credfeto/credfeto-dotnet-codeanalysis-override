using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Cocona;
using Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Helpers;
using Credfeto.DotNet.Code.Analysis.Overrides.Cmd.LoggingExtensions;
using Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Models;
using Microsoft.Extensions.Logging;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Cmd;

[SuppressMessage(
    category: "Microsoft.Performance",
    checkId: "CA1812:AvoidUninstantiatedInternalClasses",
    Justification = "Instantiated by Cocona"
)]
[SuppressMessage(
    category: "ReSharper",
    checkId: "ClassNeverInstantiated.Global",
    Justification = "Instantiated by Cocona"
)]
internal sealed class Commands
{
    private readonly ILogger<Commands> _logger;
    private static readonly CancellationToken CancellationToken = CancellationToken.None;

    [SuppressMessage(
        category: "FunFair.CodeAnalysis",
        checkId: "FFS0023: Use ILogger rather than ILogger<T>",
        Justification = "Needed in this case"
    )]
    public Commands(ILogger<Commands> logger)
    {
        this._logger = logger;
    }

    [Command("ruleset", Description = "Update codeanalysiis.ruleset")]
    [SuppressMessage(category: "ReSharper", checkId: "UnusedMember.Global", Justification = "Used by Cocona")]
    public async Task UpdateRulesetAsync(
        [Option(name: "ruleset", ['r'], Description = "ruleset file to change")]
        string rulesetFileName,
        [Option(name: "changes", ['c'], Description = "file of changes to apply")]
        string changesFileName)
    {
        IReadOnlyList<RuleChange> changes = await ChangeSet.LoadAsync(changesFileName, CancellationToken);

        if (changes is [])
        {
            this._logger.NoChangesInFile(changesFileName);
            return;
        }

        bool changed = false;
        XmlDocument ruleSet = await RuleSet.LoadAsync(rulesetFileName);

        foreach (RuleChange change in changes)
        {
            this._logger.ChangingState(change.RuleSet, rule: change.Rule, change.State);
            bool hasChanged = ruleSet.ChangeValue(ruleSet: change.RuleSet, rule: change.Rule, name: change.Description, newState: change.State);
            changed |= hasChanged;
        }

        if (changed)
        {
            await RuleSet.SaveAsync(rulesetFileName, ruleSet);
        }
    }
}