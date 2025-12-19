using System.Collections.Generic;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini.Exceptions;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini;

public static class PropertyBuilder
{
    public static IPropertyBuilder<ISettings> CreateProperty(this ISettings section)
    {
        return new GlobalPropertyBuilder<ISettings>(section);
    }

    public static IPropertyBuilder<INamedSection> CreateProperty(this INamedSection section)
    {
        return new GlobalPropertyBuilder<INamedSection>(section);
    }

    private sealed class GlobalPropertyBuilder<T> : IPropertyBuilder<T>
        where T : ISection
    {
        private readonly T _section;
        private IReadOnlyList<string>? _blockComment;
        private string? _key;
        private string? _lineComment;
        private string? _value;

        public GlobalPropertyBuilder(T section)
        {
            this._section = section;
        }

        public IPropertyBuilder<T> WithKey(string key)
        {
            if (this._section.Get(key) is not null)
            {
                // Needs better exception;
                throw new PropertyNotFoundException();
            }

            this._key = key;

            return this;
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

        public IPropertyBuilder<T> WithBlockComment(string line, IReadOnlyList<string> comments)
        {
            this._blockComment = comments;

            return this;
        }

        public T Apply()
        {
            if (string.IsNullOrWhiteSpace(this._key))
            {
                // Needs better exception;
                throw new PropertyNotFoundException();
            }

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