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
}