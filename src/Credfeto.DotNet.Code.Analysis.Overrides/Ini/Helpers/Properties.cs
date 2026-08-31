using System.Diagnostics.CodeAnalysis;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini.Exceptions;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini.Helpers;

internal static class Properties
{
    public static bool IsInvalidPropertyName(string name)
    {
        // Needs to be invalid when
        // * Is whitespace
        // * has leading/trailing whitespace
        // * contains comment chars
        // * contains []
        return string.IsNullOrWhiteSpace(name);
    }

    public static bool IsInvalidPropertyValue([NotNullWhen(false)] string? value)
    {
        if (value is null)
        {
            return true;
        }

        // Empty is a valid value (e.g. "key =" in an editorconfig); whitespace-only is not.
        return value.Length > 0 && string.IsNullOrWhiteSpace(value);
    }

    public static void RequireValidValue([NotNull] string? value)
    {
        if (IsInvalidPropertyValue(value))
        {
            throw new InvalidPropertyValueException();
        }
    }
}
