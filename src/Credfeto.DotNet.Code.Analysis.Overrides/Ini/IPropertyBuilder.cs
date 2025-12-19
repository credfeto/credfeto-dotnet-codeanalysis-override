using System.Collections.Generic;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini;

public interface IPropertyBuilder<T>
    where T : ISection
{
    IPropertyBuilder<T> WithValue(string value);

    IPropertyBuilder<T> WithLineComment(string line);

    IPropertyBuilder<T> WithBlockComment(string line, IReadOnlyList<string> comments);

    T Apply();
}