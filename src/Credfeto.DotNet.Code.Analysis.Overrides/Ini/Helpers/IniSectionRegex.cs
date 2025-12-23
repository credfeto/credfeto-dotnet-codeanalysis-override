using System.Text.RegularExpressions;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini.Helpers;

internal static partial class IniSectionRegex
{
    private const int TIMEOUT_MS = 1000;
    private const RegexOptions REGEX_OPTIONS = RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.NonBacktracking | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture;

    [GeneratedRegex(pattern: @"^\s*\[(?<Section>([^#;]|\\#|\\;)+)\]\s*([#;].*)?$", options: REGEX_OPTIONS, matchTimeoutMilliseconds: TIMEOUT_MS)]
    public static partial Regex Section();

    [GeneratedRegex(pattern: @"^\s*[#;](?<Comment>.*)", options: REGEX_OPTIONS, matchTimeoutMilliseconds: TIMEOUT_MS)]
    public static partial Regex Comment();

    [GeneratedRegex(pattern: @"^\s*(?<Key>[\w\.\-_]+)\s*[=:]\s*(?<Value>.*?)\s*([#;](?<Comment>.*))?$", options: REGEX_OPTIONS, matchTimeoutMilliseconds: TIMEOUT_MS)]
    public static partial Regex Property();
}