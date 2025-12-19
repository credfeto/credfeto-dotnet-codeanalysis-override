using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini.Helpers;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini;

public static class IniFile
{
    public static async ValueTask<ISettings> LoadAsync(string fileName, CancellationToken cancellationToken)
    {
        string[] lines = await File.ReadAllLinesAsync(path: fileName, cancellationToken: cancellationToken);

        return Extract(lines);
    }

    private static ISettings Extract(in ReadOnlySpan<string> lines)
    {
        int order = 0;
        IniSection globalSection = new(order: order, name: null, []);
        Dictionary<string, IniSection> namedSections = new(StringComparer.OrdinalIgnoreCase);

        IniSection currentSection = globalSection;

        List<string> commentLines = [];

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || IniSectionRegex.Comment()
                                                                  .IsMatch(line))
            {
                commentLines.Add(line);

                continue;
            }

            if (IsSection(line: line, out string? newSection))
            {
                currentSection = new(order: ++order, name: newSection, sectionComments: commentLines);

                commentLines = [];
                namedSections.Add(key: newSection, value: currentSection);

                continue;
            }

            currentSection.AppendPropertyLine(line: line, comments: commentLines);
            commentLines = [];
        }

        return new Settings(global: globalSection, namedSections: namedSections);
    }

    private static bool IsSection(string line, [NotNullWhen(true)] out string? sectionTitle)
    {
        Match match = IniSectionRegex.Section()
                                     .Match(line);

        if (!match.Success)
        {
            sectionTitle = null;

            return false;
        }

        sectionTitle = match.Groups[1].Value;

        return true;
    }

    public static ISettings Load(string content)
    {
        using (StringReader reader = new(content))
        {
            List<string> lines = [];

            for (string? line = reader.ReadLine(); line is not null; line = reader.ReadLine())
            {
                lines.Add(line);
            }

            return Extract([..lines]);
        }
    }
}