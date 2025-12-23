using System.Collections.Generic;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini.Extensions;

internal static class PropertyBuilderExtensions
{
    public static IPropertyBuilder<T> WithOptionalLineComment<T>(this IPropertyBuilder<T> builder, string? lineComment)
        where T : ISection
    {
        return string.IsNullOrWhiteSpace(lineComment)
            ? builder
            : builder.WithLineComment(lineComment);
    }

    public static IPropertyBuilder<T> WithOptionalBlockComment<T>(this IPropertyBuilder<T> builder, IReadOnlyList<string> blockComment)
        where T : ISection
    {
        return blockComment is []
            ? builder
            : builder.WithBlockComment(blockComment);
    }
}