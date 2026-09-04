using System;
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

    [Fact]
    public void SaveAppendsNewlyAddedPropertyAfterDeleteReusesSlot()
    {
        ISettings settings = IniFile.Create();
        INamedSection section = settings.CreateSection(sectionName: "TestSection", []);
        section.Set(key: "key1", value: "value1");
        section.Set(key: "key2", value: "value2");
        section.Set(key: "key3", value: "value3");

        section.Delete("key2");
        section.Set(key: "key4", value: "value4");

        string content = settings.Save();

        int key1Index = content.IndexOf("key1", StringComparison.Ordinal);
        int key3Index = content.IndexOf("key3", StringComparison.Ordinal);
        int key4Index = content.IndexOf("key4", StringComparison.Ordinal);

        Assert.True(key1Index < key3Index, "key1 should appear before key3");
        Assert.True(key3Index < key4Index, "key3 should appear before newly added key4");
    }

    [Fact]
    public void SaveDoesNotReorderPropertyWhenValueIsUpdated()
    {
        ISettings settings = IniFile.Create();
        INamedSection section = settings.CreateSection(sectionName: "TestSection", []);
        section.Set(key: "key1", value: "value1");
        section.Set(key: "key2", value: "value2");

        section.Set(key: "key1", value: "updated");

        string content = settings.Save();

        int key1Index = content.IndexOf("key1", StringComparison.Ordinal);
        int key2Index = content.IndexOf("key2", StringComparison.Ordinal);

        Assert.True(key1Index < key2Index, "key1 should keep its original position after being updated");
    }
}
