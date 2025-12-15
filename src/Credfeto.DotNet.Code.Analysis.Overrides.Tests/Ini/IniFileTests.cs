using Credfeto.DotNet.Code.Analysis.Overrides.Ini;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Tests.Ini;

public sealed class IniFileTests : IntegrationTestBase
{
    public IniFileTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [Fact]
    public void LoadEmpty()
    {
        IniFile file = IniFile.Load("");

        this.SaveAndCheck(file: file, expected: "");
    }

    [Fact]
    public void LoadJustGlobalNoComments()
    {
        const string original = "global = true";
        const string expected = "global = true\n\n";
        IniFile file = IniFile.Load(original);

        this.SaveAndCheck(file: file, expected: expected);
    }

    private void SaveAndCheck(IniFile file, string expected)
    {
        string updated = file.Save();
        this.Output.WriteLine(updated);
        Assert.Equal(expected, updated);
    }
}