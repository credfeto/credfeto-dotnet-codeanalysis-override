using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini.Exceptions;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini.Helpers;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini;

public static class IniFile
{
    public static ISettings Create()
    {
        return new Settings(new(order: 0, name: null, []), []);
    }

    public static async ValueTask<ISettings> LoadAsync(string fileName, CancellationToken cancellationToken)
    {
        string[] lines = await File.ReadAllLinesAsync(path: fileName, cancellationToken: cancellationToken);

        return Extract(lines);
    }

    private static ISettings Extract(in ReadOnlySpan<string> lines)
    {
        int order = 0;
        Section globalSection = new(order: order, name: null, []);
        Dictionary<string, Section> namedSections = new(StringComparer.OrdinalIgnoreCase);

        Section currentSection = globalSection;

        ExtractContext context = new();

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                context.OnBlankLine();

                continue;
            }

            if (IsComment(line: line, out string? comment))
            {
                context.OnComment(comment);

                continue;
            }

            if (IsSection(line: line, out string? newSection))
            {
                currentSection = new(order: ++order, name: newSection, context.OnSection());

                namedSections.Add(key: newSection, value: currentSection);

                continue;
            }

            if (IsProperty(line: line, out string? key, out string? value, out string? lineComment))
            {
                currentSection.AppendPropertyLine(key: key, value: value, lineComment: lineComment, context.OnProperty());

                continue;
            }

            throw new UnknownFormatException(line);
        }

        return new Settings(global: globalSection, namedSections: namedSections);
    }

    private static bool IsProperty(string line, [NotNullWhen(true)] out string? key, [NotNullWhen(true)] out string? value, [NotNullWhen(true)] out string? lineComment)
    {
        Match match = IniSectionRegex.Property()
                                     .Match(line);

        if (!match.Success)
        {
            key = null;
            value = null;
            lineComment = null;

            return false;
        }

        key = match.Groups["Key"].Value;
        value = match.Groups["Value"].Value;
        lineComment = match.Groups["Comment"]
                           .Value.TrimEnd();

        return true;
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

        sectionTitle = match.Groups["Section"].Value;

        return true;
    }

    private static bool IsComment(string line, [NotNullWhen(true)] out string? comment)
    {
        Match match = IniSectionRegex.Comment()
                                     .Match(line);

        if (!match.Success)
        {
            comment = null;

            return false;
        }

        comment = match.Groups["Comment"]
                       .Value.TrimEnd();

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

    private sealed class ExtractContext
    {
        private ImmutableArray<string> _commentLines;

        private bool _commentStarted;
        private bool _lastLineWasBlank;

        public ExtractContext()
        {
            this._lastLineWasBlank = false;
            this._commentStarted = false;
            this._commentLines = [];
        }

        public void OnBlankLine()
        {
            this._lastLineWasBlank = true;
        }

        public void OnComment(string comment)
        {
            if (this._commentStarted)
            {
                if (this._lastLineWasBlank)
                {
                    this._commentLines = this._commentLines.Add(string.Empty);
                    this._lastLineWasBlank = false;
                }
            }
            else
            {
                this._commentStarted = true;
            }

            this._commentLines = this._commentLines.Add(comment.Parse());
        }

        public IReadOnlyList<string> OnSection()
        {
            return this.CommonComments();
        }

        public IReadOnlyList<string> OnProperty()
        {
            return this.CommonComments();
        }

        private IReadOnlyList<string> CommonComments()
        {
            try
            {
                return this._commentLines;
            }
            finally
            {
                this._lastLineWasBlank = false;
                this._commentStarted = false;
                this._commentLines = [];
            }
        }
    }
}