using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini.Exceptions;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini.Helpers;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini;

public static class PropertyBuilder
{
    public static IPropertyBuilder<ISettings> CreateProperty(this ISettings section, string key)
    {
        if (Properties.IsInvalidPropertyName(key))
        {
            return Raise.SettingsPropertyNameInvalid();
        }

        if (section.Get(key) is not null)
        {
            return Raise.SettingsPropertyAlreadyExists();
        }

        return new TypedPropertyBuilder<ISettings>(section: section, key: key);
    }

    public static IPropertyBuilder<INamedSection> CreateProperty(this INamedSection section, string key)
    {
        if (Properties.IsInvalidPropertyName(key))
        {
            return Raise.SectionPropertyNameInvalid();
        }

        if (section.Get(key) is null)
        {
            return new TypedPropertyBuilder<INamedSection>(section: section, key: key);
        }

        return Raise.SectionPropertyAlreadyExists();
    }

    private static class Raise
    {
        [DoesNotReturn]
        public static IPropertyBuilder<ISettings> SettingsPropertyNameInvalid()
        {
            throw new InvalidPropertyNameException();
        }

        [DoesNotReturn]
        public static IPropertyBuilder<INamedSection> SectionPropertyNameInvalid()
        {
            throw new InvalidPropertyNameException();
        }

        [DoesNotReturn]
        public static IPropertyBuilder<ISettings> SettingsPropertyAlreadyExists()
        {
            throw new DuplicatePropertyException();
        }

        [DoesNotReturn]
        public static IPropertyBuilder<INamedSection> SectionPropertyAlreadyExists()
        {
            throw new DuplicatePropertyException();
        }
    }

    [DebuggerDisplay("{_key}: {_value}")]
    private sealed class TypedPropertyBuilder<TSection> : IPropertyBuilder<TSection>
        where TSection : ISection
    {
        private readonly string _key;
        private readonly TSection _section;
        private IReadOnlyList<string>? _blockComment;
        private string? _lineComment;
        private string? _value;

        public TypedPropertyBuilder(TSection section, string key)
        {
            this._section = section;
            this._key = key;
        }

        public IPropertyBuilder<TSection> WithValue(string value)
        {
            this._value = value;

            return this;
        }

        public IPropertyBuilder<TSection> WithLineComment(string line)
        {
            this._lineComment = line;

            return this;
        }

        public IPropertyBuilder<TSection> WithBlockComment(IReadOnlyList<string> comments)
        {
            this._blockComment = comments;

            return this;
        }

        public TSection Apply()
        {
            if (string.IsNullOrWhiteSpace(this._value))
            {
                return InvalidPropertyValue();
            }

            if (this._section.Get(this._key) is not null)
            {
                return DuplicateProperty();
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

        [DoesNotReturn]
        private static TSection InvalidPropertyValue()
        {
            throw new InvalidPropertyValueException();
        }

        [DoesNotReturn]
        private static TSection DuplicateProperty()
        {
            throw new DuplicatePropertyException();
        }
    }
}