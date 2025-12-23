using System.Collections.Generic;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini;

public interface ISettings : ISection
{
    INamedSection CreateSection(string sectionName, in IReadOnlyList<string> comments);

    INamedSection? GetSection(string sectionName);

    string Save();
}