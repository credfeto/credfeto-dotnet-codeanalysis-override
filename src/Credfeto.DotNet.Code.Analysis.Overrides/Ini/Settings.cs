using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini;

internal sealed class Settings : ISettings
{
    internal Settings(IniSection global, Dictionary<string, IniSection> namedSections)
    {
        this.NamedSections = namedSections;
        this.Global = global;
    }

    private IniSection Global { get; }

    private IDictionary<string, IniSection> NamedSections { get; }

    public string Save()
    {
        bool previousSection = false;

        return this.NamedSections.Values.Where(item => !item.IsEmpty)
                   .OrderBy(item => item.Order)
                   .Aggregate(this.SaveGlobalSection(ref previousSection), func: (current, values) => values.Save(current.WithPreviousSection(ref previousSection)))
                   .ToString();
    }

    private StringBuilder SaveGlobalSection(ref bool previousSection)
    {
        if (this.Global.IsEmpty)
        {
            return new();
        }

        previousSection = true;

        return this.Global.Save(new());
    }
}