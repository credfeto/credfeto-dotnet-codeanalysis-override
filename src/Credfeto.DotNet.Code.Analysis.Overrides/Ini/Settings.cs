using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini.Exceptions;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini.Extensions;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini.Helpers;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini;

internal sealed class Settings : ISettings
{
    private int _sectionOrder;

    internal Settings()
    {
        this._sectionOrder = 0;
        this.NamedSections = new Dictionary<string, Section>(StringComparer.Ordinal);
        this.Global = new(this, order: this._sectionOrder, name: null, []);
    }

    private Section Global { get; }

    private IDictionary<string, Section> NamedSections { get; }

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

    public IReadOnlyList<string> PropertyBlockComment(string key)
    {
        return this.Global.PropertyBlockComment(key);
    }

    public void PropertyBlockComment(string key, IReadOnlyList<string> comments)
    {
        this.Global.PropertyBlockComment(key: key, comments: comments);
    }

    public string PropertyLineComment(string key)
    {
        return this.Global.PropertyLineComment(key);
    }

    public void PropertyLineComment(string key, string comment)
    {
        this.Global.PropertyLineComment(key: key, comment: comment);
    }

    public INamedSection CreateSection(string sectionName, in IReadOnlyList<string> comments)
    {
        if (Sections.IsInvalidSectionName(sectionName))
        {
            return Raise.InvalidSectionName();
        }

        if (this.NamedSections.ContainsKey(sectionName))
        {
            return Raise.SectionAlreadyExists();
        }

        Section section = new(this,
                              order: ++this._sectionOrder,
                              name: sectionName,
                              [
                                  .. comments.Select(Comments.Parse)
                              ]);
        this.NamedSections.Add(key: sectionName, value: section);

        return section;
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

    private static class Raise
    {
        [DoesNotReturn]
        public static INamedSection InvalidSectionName()
        {
            throw new InvalidSectionNameException();
        }

        [DoesNotReturn]
        public static INamedSection SectionAlreadyExists()
        {
            throw new SectionAlreadyExistsException();
        }
    }
}