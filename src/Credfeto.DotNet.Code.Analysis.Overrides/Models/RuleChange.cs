using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Models;

[DebuggerDisplay("{RuleSet}.{Rule} => {State} ({Description})")]
public sealed class RuleChange
{
    [JsonConstructor]
    public RuleChange(string ruleSet, string rule, string state, string description)
    {
        this.RuleSet = ruleSet;
        this.Rule = rule;
        this.State = state;
        this.Description = description;
    }

    public string RuleSet { get; }

    public string Rule { get; }

    public string State { get; }

    public string Description { get; }
}
