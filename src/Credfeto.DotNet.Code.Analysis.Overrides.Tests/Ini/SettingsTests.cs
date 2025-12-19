using System;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Tests.Ini;

public sealed class SettingsTests : IntegrationTestBase
{
    public SettingsTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [Fact]
    public void CreateEmpty()
    {
        const string expected = "";

        ISettings file = IniFile.Create();

        this.SaveAndCheck(file: file, expected: expected);
    }

    [Fact]
    public void CreateAndAddSetting()
    {
        const string expected = @"Example = 42
";

        ISettings file = IniFile.Create();

        file.Set(key: "Example", value: "42");

        this.SaveAndCheck(file: file, expected: expected);
    }

    [Fact]
    public void CreateAndAddSettingWithComment()
    {
        const string expected = @"# Hello World!
Example = 42
";

        ISettings file = IniFile.Create();

        file.Set(key: "Example", value: "42");
        file.Comment(key: "Example", ["Hello World!"]);

        this.SaveAndCheck(file: file, expected: expected);
    }

    [Fact]
    public void LoadEmpty()
    {
        const string original = "";
        const string expected = "";

        ISettings file = IniFile.Load(original);

        this.SaveAndCheck(file: file, expected: expected);
    }

    [Fact]
    public void LoadGlobalNoComments()
    {
        const string original = @"global = true
";
        const string expected = @"global = true
";
        ISettings file = IniFile.Load(original);

        this.SaveAndCheck(file: file, expected: expected);
    }

    [Fact]
    public void LoadGlobalWithLineComment()
    {
        const string original = @"global = true#This is a root config
";
        const string expected = @"global = true # This is a root config
";
        ISettings file = IniFile.Load(original);

        this.SaveAndCheck(file: file, expected: expected);
    }

    [Fact]
    public void LoadGlobalWithLineCommentAndCommentsAbove()
    {
        const string original = @"#This is a block comment
#Line 2

#Line 3
global = true#This is a root config
";
        const string expected = @"# This is a block comment
# Line 2
#
# Line 3
global = true # This is a root config
";
        ISettings file = IniFile.Load(original);

        this.SaveAndCheck(file: file, expected: expected);
    }

    [Fact]
    public void LoadJustGlobalNoCommentsStandardisesSpacing()
    {
        const string original = @"global=true
";
        const string expected = @"global = true
";
        ISettings file = IniFile.Load(original);

        this.SaveAndCheck(file: file, expected: expected);
    }

    [Fact]
    public void LoadNamedSectionNoComments()
    {
        const string original = @"[Named]
test = true
";
        const string expected = @"[Named]
test = true
";
        ISettings file = IniFile.Load(original);

        this.SaveAndCheck(file: file, expected: expected);
    }

    [Fact]
    public void LoadNamedSectionWithComments()
    {
        const string original = @"# Comment Line 1
#Comment Line 2


#Comment Line 3

[Named]
test = true
";
        const string expected = @"# Comment Line 1
# Comment Line 2
#
# Comment Line 3
[Named]
test = true
";
        ISettings file = IniFile.Load(original);

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
charset = utf-8
trim_trailing_whitespace = true
insert_final_newline = false
indent_style = space
indent_size = 4
max_line_length = 200
spaces_around_operators = true
indent_brace_style = K&R
end_of_line = crlf

[*.csproj]
charset = utf-8
trim_trailing_whitespace = true
insert_final_newline = false
indent_style = space
max_line_length = 200
indent_size = 2
end_of_line = crlf
";
        ISettings file = IniFile.Load(original);

        this.SaveAndCheck(file: file, expected: expected);
    }

    private void SaveAndCheck(ISettings file, string expected)
    {
        string updated = file.Save();
        this.Output.WriteLine(updated);
        Assert.Equal(expected.Replace(oldValue: Environment.NewLine, newValue: "\n", comparisonType: StringComparison.Ordinal), actual: updated);
    }
}