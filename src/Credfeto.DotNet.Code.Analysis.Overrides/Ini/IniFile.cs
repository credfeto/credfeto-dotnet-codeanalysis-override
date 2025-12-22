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
        return new Settings();
    }

    public static async ValueTask<ISettings> LoadAsync(string fileName, CancellationToken cancellationToken)
    {
        string[] lines = await File.ReadAllLinesAsync(path: fileName, cancellationToken: cancellationToken);

        return Extract(lines);
    }

    private static ISettings Extract(in ReadOnlySpan<string> lines)
    {
        Settings settings = new();
        ISection globalSection = settings;
        ISection currentSection = globalSection;

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
                currentSection = settings.CreateSection(sectionName: newSection, [..context.OnSection()]);

                continue;
            }

            if (IsProperty(line: line, out string? key, out string? value, out string? lineComment))
            {
                currentSection = currentSection switch
                {
                    INamedSection namedSection => AddSectionProperty(namedSection: namedSection, key: key, value: value, lineComment: lineComment, context: context),
                    ISettings globalSettings => AddGlobalProperty(globalSettings: globalSettings, key: key, value: value, lineComment: lineComment, context: context),
                    _ => throw new UnknownFormatException(line)
                };

                continue;
            }

            throw new UnknownFormatException(line);
        }

        return settings;
    }

    private static INamedSection AddSectionProperty(INamedSection namedSection, string key, string value, string lineComment, ExtractContext context)
    {
        return ConfigureProperty(namedSection.CreateProperty(key), value: value, lineComment: lineComment, context: context);
    }

    private static T ConfigureProperty<T>(IPropertyBuilder<T> builder, string value, string lineComment, ExtractContext context)
        where T : ISection
    {
        builder = builder.WithValue(value);

        if (!string.IsNullOrWhiteSpace(lineComment))
        {
            builder = builder.WithLineComment(lineComment);
        }

        IReadOnlyList<string> bc = context.OnProperty();

        if (bc is not [])
        {
            builder = builder.WithBlockComment(bc);
        }

        return builder.Apply();
    }

    private static ISettings AddGlobalProperty(ISettings globalSettings, string key, string value, string lineComment, ExtractContext context)
    {
        return ConfigureProperty(globalSettings.CreateProperty(key), value: value, lineComment: lineComment, context: context);
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