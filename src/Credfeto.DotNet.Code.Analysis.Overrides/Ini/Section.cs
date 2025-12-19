using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini.Exceptions;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini.Extensions;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini.Helpers;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini;

[DebuggerDisplay("{Name} Order {Order}")]
internal sealed class Section : ISection
{
    private readonly Dictionary<string, PropertyValue> _properties;
    private readonly List<string> _sectionComments;

    public Section(int order, string? name, IReadOnlyList<string> sectionComments)
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

    public string? Get(string key)
    {
        return this._properties.TryGetValue(key: key, out PropertyValue? propertyValue)
            ? propertyValue.Value
            : null;
    }

    public void Set(string key, string value)
    {
        if (this._properties.TryGetValue(key: key, out PropertyValue? propertyValue))
        {
            propertyValue.Value = value;

            return;
        }

        PropertyValue newProperty = new(value: value, lineComment: "", []);
        this._properties.Add(key: key, value: newProperty);
    }

    public void Delete(string key)
    {
        _ = this._properties.Remove(key);
    }

    public void Comment(string key, in IReadOnlyList<string> comments)
    {
        if (this._properties.TryGetValue(key: key, out PropertyValue? propertyValue))
        {
            propertyValue.Comments =
            [
                ..comments.Select(ParseComment)
            ];

            return;
        }

        throw new PropertyNotFoundException();
    }

    public IReadOnlyList<string> Comment(string key)
    {
        return this._properties.TryGetValue(key: key, out PropertyValue? propertyValue)
            ? propertyValue.Comments
            : throw new PropertyNotFoundException();
    }

    private static string ParseComment(string comment)
    {
        return string.IsNullOrWhiteSpace(comment)
            ? string.Empty
            : string.Concat(str0: " ", str1: comment);
    }

    public void AppendPropertyLine(string key, string value, string lineComment, IReadOnlyList<string> comments)
    {
        this._properties.Add(key: key, new(value: value, lineComment: lineComment, [..Comments.Clean(comments)]));
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
            stringBuilder = stringBuilder.AppendComments(comments: this._sectionComments)
                                         .AppendLine($"[{this.Name}]");
        }

        foreach ((string key, PropertyValue propertyValue) in this._properties)
        {
            stringBuilder = stringBuilder.AppendComments(comments: propertyValue.Comments)
                                         .AppendProperty(key: key, value: propertyValue.Value, lineComment: propertyValue.LineComment);
        }

        return stringBuilder;
    }

    [DebuggerDisplay("{Value}")]
    private sealed class PropertyValue
    {
        public PropertyValue(string value, string lineComment, IReadOnlyList<string> comments)
        {
            this.Value = value;
            this.LineComment = lineComment;
            this.Comments = comments;
        }

        public string Value { get; set; }

        public string LineComment { get; }

        public IReadOnlyList<string> Comments { get; set; }
    }
}