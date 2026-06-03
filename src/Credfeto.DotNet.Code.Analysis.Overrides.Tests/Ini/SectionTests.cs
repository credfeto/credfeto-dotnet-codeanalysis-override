using System.Collections.Generic;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini.Exceptions;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Tests.Ini;

public sealed class SectionTests : IntegrationTestBase
{
    public SectionTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void DeleteRemovesPropertyFromSection()
    {
        ISettings settings = IniFile.Create();
        INamedSection section = settings.CreateSection(sectionName: "TestSection", []);
        section.Set(key: "key1", value: "value1");

        section.Delete("key1");

        string? value = section.Get("key1");
        Assert.Null(value);
    }

    [Fact]
    public void DeleteNonExistentKeyDoesNotThrow()
    {
        ISettings settings = IniFile.Create();
        INamedSection section = settings.CreateSection(sectionName: "TestSection", []);

        section.Delete("nonexistent");
    }

    [Fact]
    public void PropertyBlockCommentThrowsWhenKeyDoesNotExist()
    {
        ISettings settings = IniFile.Create();
        INamedSection section = settings.CreateSection(sectionName: "TestSection", []);

        Assert.Throws<PropertyNotFoundException>(() => section.PropertyBlockComment(key: "nonexistent", ["comment"]));
    }

    [Fact]
    public void PropertyLineCommentGetThrowsWhenKeyDoesNotExist()
    {
        ISettings settings = IniFile.Create();
        INamedSection section = settings.CreateSection(sectionName: "TestSection", []);

        Assert.Throws<PropertyNotFoundException>(() => section.PropertyLineComment("nonexistent"));
    }

    [Fact]
    public void PropertyLineCommentSetThrowsWhenKeyDoesNotExist()
    {
        ISettings settings = IniFile.Create();
        INamedSection section = settings.CreateSection(sectionName: "TestSection", []);

        Assert.Throws<PropertyNotFoundException>(() =>
            section.PropertyLineComment(key: "nonexistent", comment: "some comment")
        );
    }

    [Fact]
    public void SectionCommentGetReturnsEmptyWhenNoCommentSet()
    {
        ISettings settings = IniFile.Create();
        INamedSection section = settings.CreateSection(sectionName: "TestSection", []);

        IReadOnlyList<string> comments = section.SectionComment();

        Assert.NotNull(comments);
        Assert.Empty(comments);
    }

    [Fact]
    public void SectionCommentSetAndGetRoundTrips()
    {
        ISettings settings = IniFile.Create();
        INamedSection section = settings.CreateSection(sectionName: "TestSection", []);

        section.SectionComment(["First line", "Second line"]);

        IReadOnlyList<string> comments = section.SectionComment();

        Assert.NotNull(comments);
        Assert.Equal(expected: 2, actual: comments.Count);
    }

    [Fact]
    public void PropertyBlockCommentGetThrowsWhenKeyDoesNotExist()
    {
        ISettings settings = IniFile.Create();
        INamedSection section = settings.CreateSection(sectionName: "TestSection", []);

        Assert.Throws<PropertyNotFoundException>(() => section.PropertyBlockComment("nonexistent"));
    }

    [Fact]
    public void PropertyBlockCommentGetReturnsSetComment()
    {
        ISettings settings = IniFile.Create();
        INamedSection section = settings.CreateSection(sectionName: "TestSection", []);
        section.Set(key: "key1", value: "value1");
        section.PropertyBlockComment(key: "key1", ["Hello World"]);

        IReadOnlyList<string> comments = section.PropertyBlockComment("key1");

        Assert.NotNull(comments);
        Assert.Single(comments);
    }

    [Fact]
    public void PropertyLineCommentGetReturnsSetComment()
    {
        ISettings settings = IniFile.Create();
        INamedSection section = settings.CreateSection(sectionName: "TestSection", []);
        section.Set(key: "key1", value: "value1");
        section.PropertyLineComment(key: "key1", comment: "This is a comment");

        string lineComment = section.PropertyLineComment("key1");

        Assert.Equal(expected: "This is a comment", actual: lineComment);
    }

    [Fact]
    public void ToSettingsReturnsTheParentSettings()
    {
        ISettings settings = IniFile.Create();
        INamedSection section = settings.CreateSection(sectionName: "TestSection", []);

        ISettings returned = section.ToSettings();

        Assert.Same(expected: settings, actual: returned);
    }
}
