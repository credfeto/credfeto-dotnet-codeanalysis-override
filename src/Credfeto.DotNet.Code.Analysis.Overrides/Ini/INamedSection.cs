using System.Collections.Generic;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini;

public interface INamedSection : ISection
{
    IReadOnlyList<string> SectionComment();

    void SectionComment(IReadOnlyList<string> comments);
}