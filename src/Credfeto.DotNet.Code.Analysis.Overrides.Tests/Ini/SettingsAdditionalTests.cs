using System.Collections.Generic;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini.Exceptions;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Tests.Ini;

public sealed class SettingsAdditionalTests : IntegrationTestBase
{
    public SettingsAdditionalTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void GetSectionReturnsNullForMissingSection()
    {
        ISettings settings = IniFile.Create();

        INamedSection? section = settings.GetSection("DoesNotExist");

        Assert.Null(section);
    }

    [Fact]
    public void GetSectionReturnsCreatedSection()
    {
        ISettings settings = IniFile.Create();
        settings.CreateSection(sectionName: "ExistingSection", []);

        INamedSection? section = settings.GetSection("ExistingSection");

        Assert.NotNull(section);
    }

    [Fact]
    public void CreateSectionWithEmptyNameThrowsInvalidSectionNameException()
    {
        ISettings settings = IniFile.Create();

        Assert.Throws<InvalidSectionNameException>(() => settings.CreateSection(sectionName: "", []));
    }

    [Fact]
    public void CreateSectionWithWhitespaceNameThrowsInvalidSectionNameException()
    {
        ISettings settings = IniFile.Create();

        Assert.Throws<InvalidSectionNameException>(() => settings.CreateSection(sectionName: "   ", []));
    }

    [Fact]
    public void CreateDuplicateSectionThrowsSectionAlreadyExistsException()
    {
        ISettings settings = IniFile.Create();
        settings.CreateSection(sectionName: "MySection", []);

        Assert.Throws<SectionAlreadyExistsException>(() => settings.CreateSection(sectionName: "MySection", []));
    }

    [Fact]
    public void DeleteRemovesPropertyFromGlobalSection()
    {
        ISettings settings = IniFile.Create();
        settings.Set(key: "GlobalKey", value: "GlobalValue");

        settings.Delete("GlobalKey");

        string? value = settings.Get("GlobalKey");
        Assert.Null(value);
    }

    [Fact]
    public void PropertyBlockCommentGetFromGlobalSectionReturnsSetComment()
    {
        ISettings settings = IniFile.Create();
        settings.Set(key: "GlobalKey", value: "GlobalValue");
        settings.PropertyBlockComment(key: "GlobalKey", ["Global comment"]);

        IReadOnlyList<string> comments = settings.PropertyBlockComment("GlobalKey");

        Assert.NotNull(comments);
        Assert.Single(comments);
    }

    [Fact]
    public void PropertyBlockCommentGetFromGlobalSectionThrowsWhenKeyDoesNotExist()
    {
        ISettings settings = IniFile.Create();

        Assert.Throws<PropertyNotFoundException>(() => settings.PropertyBlockComment("NonExistent"));
    }
}
