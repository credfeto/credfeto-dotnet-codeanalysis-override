using System;
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
        const string original = @"global = true
";
        const string expected = @"global = true
";
        IniFile file = IniFile.Load(original);

        this.SaveAndCheck(file: file, expected: expected);
    }

    [Fact]
    public void RoundTripMultipleSectionsComments()
    {
        const string original = @"# Editor configuration, see http://editorconfig.org
root = true

[*.sol]
charset=utf-8
trim_trailing_whitespace=true
insert_final_newline=false
indent_style=space
indent_size=4
max_line_length=200
spaces_around_operators=true
indent_brace_style=K&R
end_of_line=crlf

[*.csproj]
charset=utf-8
trim_trailing_whitespace=true
insert_final_newline=false
indent_style=space
max_line_length=200
indent_size=2
end_of_line=crlf
";
        const string expected = @"# Editor configuration, see http://editorconfig.org
root = true

[*.sol]
charset=utf-8
trim_trailing_whitespace=true
insert_final_newline=false
indent_style=space
indent_size=4
max_line_length=200
spaces_around_operators=true
indent_brace_style=K&R
end_of_line=crlf

[*.csproj]
charset=utf-8
trim_trailing_whitespace=true
insert_final_newline=false
indent_style=space
max_line_length=200
indent_size=2
end_of_line=crlf
";
        IniFile file = IniFile.Load(original);

        this.SaveAndCheck(file: file, expected: expected);
    }

    private void SaveAndCheck(IniFile file, string expected)
    {
        string updated = file.Save();
        this.Output.WriteLine(updated);
        Assert.Equal(expected.Replace(Environment.NewLine, "\n", StringComparison.Ordinal), updated);
    }
}