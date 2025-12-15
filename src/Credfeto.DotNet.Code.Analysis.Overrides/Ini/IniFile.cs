using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini;

public sealed class IniFile
{
    private IniFile(IniSection global, Dictionary<string, IniSection> namedSections)
    {
        this.NamedSections = namedSections;
        this.Global = global;
    }

    private IniSection Global { get; }

    private IDictionary<string, IniSection> NamedSections { get; }

    public string Save()
    {
        StringBuilder stringBuilder = new();

        bool previousSection = false;

        if (!this.Global.IsEmpty)
        {
            stringBuilder = this.Global.Save(stringBuilder);
            previousSection = true;
        }

        foreach ((_, string section, IniSection values) in this.NamedSections.Select(item => (order: item.Value.Order, section: item.Key, entries: item.Value))
                                                               .OrderBy(item => item.order))
        {
            if (!values.IsEmpty)
            {
                if (previousSection)
                {
                    stringBuilder = stringBuilder.AppendLine();
                }

                stringBuilder = stringBuilder.AppendLine($"[{section}]");
                stringBuilder = this.Global.Save(stringBuilder);
                previousSection = true;
            }
        }

        return stringBuilder.ToString();
    }

    public static async ValueTask<IniFile> LoadAsync(string fileName, CancellationToken cancellationToken)
    {
        string[] lines = await File.ReadAllLinesAsync(path: fileName, cancellationToken: cancellationToken);

        return Extract(lines);
    }

    private static IniFile Extract(in ReadOnlySpan<string> lines)
    {
        int order = 0;
        IniSection globalSection = new(order);
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
                currentSection = new(++order);

                foreach (string commentLine in commentLines)
                {
                    currentSection.AppendLine(commentLine);
                }

                commentLines.Clear();
                namedSections.Add(key: newSection, value: currentSection);

                continue;
            }

            currentSection.AppendLine(line);
        }

        foreach (string commentLine in commentLines)
        {
            currentSection.AppendLine(commentLine);
        }

        return new(global: globalSection, namedSections: namedSections);
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

    public static IniFile Load(string content)
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