using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini.Helpers;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini;

[DebuggerDisplay("Order {Order}")]
public sealed class IniSection
{
    private readonly Dictionary<string, PropertyValue> _properties;
    private readonly List<string> _sectionComments;

    public IniSection(int order, string? name, IReadOnlyList<string> sectionComments)
    {
        this._sectionComments = [..Comments.Clean(sectionComments)];
        this.Order = order;
        this.Name = name;
        this._properties = new(StringComparer.OrdinalIgnoreCase);
    }

    public int Order { get; }

    public string? Name { get; }

    // Allow for deletions
    public bool IsEmpty => this._properties.Values.Count == 0;

    public void AppendPropertyLine(string line, List<string> comments)
    {
        Match match = IniSectionRegex.Property()
                             .Match(line);

        string key = match.Groups["Key"].Value;
        string value = match.Groups["Value"].Value;

        this._properties.Add(key: key, new(value: value, [..Comments.Clean(comments)]));
    }

    public StringBuilder Save(StringBuilder stringBuilder)
    {
        // Need to:
        // * If it is a property and the value is different, then return the new value as a property
        // * If it is a comment make comment type consistent in the file
        // * if it is a property with a comment append the comment to the end of the file
        // * If >1 blank line; skip
        // * Leave with the last line being a property or comment; no blank lines

        if (!string.IsNullOrWhiteSpace(this.Name))
        {
            foreach (string comment in this._sectionComments)
            {
                stringBuilder = stringBuilder.AppendLine(comment);
            }

            stringBuilder = stringBuilder.AppendLine($"[{this.Name}]");
        }

        foreach ((string key, PropertyValue value) in this._properties)
        {
            foreach (string comment in value.Comments)
            {
                stringBuilder = stringBuilder.AppendLine(comment);
            }

            stringBuilder = stringBuilder.AppendLine($"{key} = {value.Value}");
        }

        return stringBuilder;
    }

    [DebuggerDisplay("{LineIndex}: {Value}")]
    private sealed class PropertyValue
    {
        public PropertyValue(string value, List<string> comments)
        {
            this.Value = value;
            this.Comments = comments;
        }

        public int LineIndex { get; }

        public string Value { get; }

        public List<string> Comments { get; }
    }
}