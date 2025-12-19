using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini.Exceptions;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini.Extensions;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini;

internal sealed class Settings : ISettings
{
    private int _sectionOrder;

    internal Settings(Section global, Dictionary<string, Section> namedSections)
    {
        this.NamedSections = namedSections;
        this.Global = global;
        this._sectionOrder = namedSections.Values.Count + 1;
    }

    private Section Global { get; }

    private IDictionary<string, Section> NamedSections { get; }

    public INamedSection CreateSection(string sectionName, in ReadOnlySpan<string> comments)
    {
        if (string.IsNullOrWhiteSpace(sectionName) || this.NamedSections.ContainsKey(sectionName))
        {
            throw new SectionAlreadyExistsException();
        }

        Section section = new(order: ++this._sectionOrder, name: sectionName, [..comments]);
        this.NamedSections.Add(key: sectionName, value: section);

        return section;
    }

    public INamedSection? GetSection(string sectionName)
    {
        return this.NamedSections.TryGetValue(key: sectionName, out Section? section)
            ? section
            : null;
    }

    public string Save()
    {
        bool previousSection = false;

        return this.NamedSections.Values.Where(item => !item.IsEmpty)
                   .OrderBy(item => item.Order)
                   .Aggregate(this.SaveGlobalSection(ref previousSection), func: (current, values) => values.Save(current.WithPreviousSection(ref previousSection)))
                   .ToString();
    }

    public string? Get(string key)
    {
        return this.Global.Get(key);
    }

    public void Set(string key, string value)
    {
        this.Global.Set(key: key, value: value);
    }

    public void Delete(string key)
    {
        this.Global.Delete(key);
    }

    public IReadOnlyList<string> Comment(string key)
    {
        return this.Global.Comment(key);
    }

    public void Comment(string key, IReadOnlyList<string> comments)
    {
        this.Global.Comment(key: key, comments: comments);
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