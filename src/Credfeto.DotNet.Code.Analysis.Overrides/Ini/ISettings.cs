using System;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini;

public interface ISettings : ISection
{
    ISection CreateSection(string sectionName, in ReadOnlySpan<string> comments);

    ISection? GetSection(string sectionName);

    string Save();
}