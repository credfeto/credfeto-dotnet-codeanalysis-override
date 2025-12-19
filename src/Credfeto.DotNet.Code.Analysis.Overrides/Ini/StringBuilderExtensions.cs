using System.Text;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini;

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
}