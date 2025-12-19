using System;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini;

public interface ISettings : ISection
{
    INamedSection CreateSection(string sectionName, in ReadOnlySpan<string> comments);

    INamedSection? GetSection(string sectionName);

    string Save();
}