using System.Collections.Generic;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini;

public interface ISection
{
    string? Get(string key);

    void Set(string key, string value);

    void Delete(string key);

    IReadOnlyList<string> Comment(string key);

    void Comment(string key, IReadOnlyList<string> comments);
}