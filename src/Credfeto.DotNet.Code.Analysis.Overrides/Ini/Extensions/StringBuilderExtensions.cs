using System.Collections.Generic;
using System.Text;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini.Extensions;

internal static class StringBuilderExtensions
{
    public static StringBuilder WithPreviousSection(this StringBuilder stringBuilder, ref bool previousSection)
    {
        if (previousSection)
        {
            return stringBuilder.AppendLine();
        }

        previousSection = true;

        return stringBuilder;
    }

    public static StringBuilder AppendComments(this StringBuilder stringBuilder, IReadOnlyList<string> comments)
    {
        bool lastLineWasBlank = false;

        foreach (string comment in comments)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                lastLineWasBlank = true;

                continue;
            }

            if (lastLineWasBlank)
            {
                stringBuilder = stringBuilder.AppendLine("#");
                lastLineWasBlank = false;
            }

            stringBuilder = stringBuilder.AppendComment(comment: comment);
        }

        return lastLineWasBlank
            ? stringBuilder.AppendLine("#")
            : stringBuilder;
    }

    private static StringBuilder AppendComment(this StringBuilder stringBuilder, string comment)
    {
        return stringBuilder.AppendLine($"#{comment.TrimEnd()}");
    }

    public static StringBuilder AppendProperty(this StringBuilder stringBuilder, string key, string value, string lineComment)
    {
        return string.IsNullOrWhiteSpace(lineComment)
            ? stringBuilder.AppendLine($"{key} = {value}")
            : stringBuilder.AppendLine($"{key} = {value} #{lineComment.TrimEnd()}");
    }
}