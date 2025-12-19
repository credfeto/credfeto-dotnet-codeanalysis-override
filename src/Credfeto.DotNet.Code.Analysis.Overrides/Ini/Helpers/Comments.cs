using System.Collections.Generic;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini.Helpers;

internal static class Comments
{
    public static string Parse(this string comment)
    {
        return string.IsNullOrWhiteSpace(comment)
            ? string.Empty
            : string.Concat(str0: " ", comment.TrimEnd());
    }

    public static IEnumerable<string> Clean(IEnumerable<string> sectionComments)
    {
        bool headSeen = false;
        bool previousWasBlankLine = false;

        foreach (string line in sectionComments)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                if (!headSeen)
                {
                    continue;
                }

                previousWasBlankLine = true;
            }
            else
            {
                if (headSeen)
                {
                    if (previousWasBlankLine)
                    {
                        yield return "";

                        previousWasBlankLine = false;
                    }
                }
                else
                {
                    headSeen = true;
                    previousWasBlankLine = false;
                }

                yield return line;
            }
        }
    }
}