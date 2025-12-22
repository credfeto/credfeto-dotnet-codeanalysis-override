using System.Collections.Generic;
using System.Diagnostics;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini.Exceptions;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini;

public static class PropertyBuilder
{
    public static IPropertyBuilder<ISettings> CreateProperty(this ISettings section, string key)
    {
        if (section.Get(key) is not null)
        {
            // Needs better exception;
            throw new PropertyNotFoundException();
        }

        return new TypedPropertyBuilder<ISettings>(section: section, key: key);
    }

    public static IPropertyBuilder<INamedSection> CreateProperty(this INamedSection section, string key)
    {
        if (section.Get(key) is not null)
        {
            // Needs better exception;
            throw new PropertyNotFoundException();
        }

        return new TypedPropertyBuilder<INamedSection>(section: section, key: key);
    }

    [DebuggerDisplay("{_key}: {_value}")]
    private sealed class TypedPropertyBuilder<T> : IPropertyBuilder<T>
        where T : ISection
    {
        private readonly string _key;
        private readonly T _section;
        private IReadOnlyList<string>? _blockComment;
        private string? _lineComment;
        private string? _value;

        public TypedPropertyBuilder(T section, string key)
        {
            this._section = section;
            this._key = key;
        }

        public IPropertyBuilder<T> WithValue(string value)
        {
            this._value = value;

            return this;
        }

        public IPropertyBuilder<T> WithLineComment(string line)
        {
            this._lineComment = line;

            return this;
        }

        public IPropertyBuilder<T> WithBlockComment(IReadOnlyList<string> comments)
        {
            this._blockComment = comments;

            return this;
        }

        public T Apply()
        {
            if (string.IsNullOrWhiteSpace(this._value))
            {
                // Needs better exception;
                throw new PropertyNotFoundException();
            }

            if (this._section.Get(this._key) is not null)
            {
                // Needs better exception;
                throw new PropertyNotFoundException();
            }

            this._section.Set(key: this._key, value: this._value);

            if (!string.IsNullOrWhiteSpace(this._lineComment))
            {
                this._section.PropertyLineComment(key: this._key, comment: this._lineComment);
            }

            if (this._blockComment is not null and not [])
            {
                this._section.PropertyBlockComment(key: this._key, comments: this._blockComment);
            }

            return this._section;
        }
    }
}