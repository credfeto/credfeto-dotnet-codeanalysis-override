namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini.Helpers;

internal static class Sections
{
    public static bool IsInvalidSectionName(string name)
    {
        // Needs to be invalid when
        // * Is whitespace
        // * has leading/trailing whitespace
        // * contains comment chars
        // * contains []
        return string.IsNullOrWhiteSpace(name);
    }
}