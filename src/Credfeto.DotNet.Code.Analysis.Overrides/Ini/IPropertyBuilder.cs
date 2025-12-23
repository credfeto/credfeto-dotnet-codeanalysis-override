using System.Collections.Generic;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini;

public interface IPropertyBuilder<out TSection>
    where TSection : ISection
{
    IPropertyBuilder<TSection> WithValue(string value);

    IPropertyBuilder<TSection> WithLineComment(string line);

    IPropertyBuilder<TSection> WithBlockComment(IReadOnlyList<string> comments);

    TSection Apply();
}