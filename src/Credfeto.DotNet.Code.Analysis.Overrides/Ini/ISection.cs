using System.Collections.Generic;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini;

public interface ISection
{
    string? Get(string key);

    void Set(string key, string value);

    void Delete(string key);

    IReadOnlyList<string> PropertyBlockComment(string key);

    void PropertyBlockComment(string key, IReadOnlyList<string> comments);

    string PropertyLineComment(string key);

    void PropertyLineComment(string key, string comment);
}