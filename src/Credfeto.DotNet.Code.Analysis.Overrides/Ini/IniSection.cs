using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini;

[DebuggerDisplay("Order {Order}")]
public sealed class IniSection
{
    private readonly Dictionary<int, string> _lines;
    private readonly Dictionary<string, PropertyValue> _properties;
    private int _lineIndex;

    public IniSection(int order)
    {
        this.Order = order;
        this._lineIndex = 0;
        this._lines = [];
        this._properties = new(StringComparer.OrdinalIgnoreCase);
    }

    public int Order { get; }

    // Allow for deletions
    public bool IsEmpty => this._properties.Values.Count == 0;

    public void AppendLine(string line)
    {
        int index = this._lineIndex++;

        if (string.IsNullOrWhiteSpace(line))
        {
            this._lines.Add(key: index, value: "");

            return;
        }

        this._lines.Add(key: index, value: line);

        if (IsComment(line))
        {
            return;
        }

        Match match = IniSectionRegex.Property()
                                     .Match(line);

        string key = match.Groups["Key"].Value;
        string value = match.Groups["Value"].Value;

        this._properties.Add(key: key, new(lineIndex: index, value: value));
    }

    private static bool IsComment(string line)
    {
        return IniSectionRegex.Comment()
                              .IsMatch(line);
    }

    public StringBuilder Save(StringBuilder stringBuilder)
    {
        foreach (KeyValuePair<int, string> line in this._lines)
        {
            // Need to:
            // * If it is a property and the value is different, then return the new value as a property
            // * If it is a comment make comment type consistent in the file
            // * if it is a property with a comment append the comment to the end of the file
            // * If >1 blank line; skip
            // * Leave with the last line being a property or comment; no blank lines
            stringBuilder = stringBuilder.AppendLine(line.Value);
        }

        return stringBuilder;
    }

    [DebuggerDisplay("{LineIndex}: {Value}")]
    private sealed class PropertyValue
    {
        public PropertyValue(int lineIndex, string value)
        {
            this.Value = value;
            this.LineIndex = lineIndex;
        }

        public int LineIndex { get; }

        public string Value { get; }
    }
}