using Credfeto.DotNet.Code.Analysis.Overrides.Ini;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini.Exceptions;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Tests.Ini;

public sealed class PropertyBuilderTests : IntegrationTestBase
{
    public PropertyBuilderTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void CreatePropertyOnSettingsWithEmptyKeyThrowsInvalidPropertyNameException()
    {
        ISettings settings = IniFile.Create();

        Assert.Throws<InvalidPropertyNameException>(() => settings.CreateProperty(""));
    }

    [Fact]
    public void CreatePropertyOnSettingsWithWhitespaceKeyThrowsInvalidPropertyNameException()
    {
        ISettings settings = IniFile.Create();

        Assert.Throws<InvalidPropertyNameException>(() => settings.CreateProperty("   "));
    }

    [Fact]
    public void CreatePropertyOnSettingsWithDuplicateKeyThrowsDuplicatePropertyException()
    {
        ISettings settings = IniFile.Create();
        settings.Set(key: "ExistingKey", value: "somevalue");

        Assert.Throws<DuplicatePropertyException>(() => settings.CreateProperty("ExistingKey"));
    }

    [Fact]
    public void CreatePropertyOnNamedSectionWithEmptyKeyThrowsInvalidPropertyNameException()
    {
        ISettings settings = IniFile.Create();
        INamedSection section = settings.CreateSection(sectionName: "MySection", []);

        Assert.Throws<InvalidPropertyNameException>(() => section.CreateProperty(""));
    }

    [Fact]
    public void CreatePropertyOnNamedSectionWithWhitespaceKeyThrowsInvalidPropertyNameException()
    {
        ISettings settings = IniFile.Create();
        INamedSection section = settings.CreateSection(sectionName: "MySection", []);

        Assert.Throws<InvalidPropertyNameException>(() => section.CreateProperty("   "));
    }

    [Fact]
    public void CreatePropertyOnNamedSectionWithDuplicateKeyThrowsDuplicatePropertyException()
    {
        ISettings settings = IniFile.Create();
        INamedSection section = settings.CreateSection(sectionName: "MySection", []);
        section.Set(key: "ExistingKey", value: "somevalue");

        Assert.Throws<DuplicatePropertyException>(() => section.CreateProperty("ExistingKey"));
    }

    [Fact]
    public void ApplyWithoutValueThrowsInvalidPropertyValueException()
    {
        ISettings settings = IniFile.Create();

        Assert.Throws<InvalidPropertyValueException>(() => settings.CreateProperty("NewKey").Apply());
    }

    [Fact]
    public void ApplyWithWhitespaceValueThrowsInvalidPropertyValueException()
    {
        ISettings settings = IniFile.Create();

        Assert.Throws<InvalidPropertyValueException>(() => settings.CreateProperty("NewKey").WithValue("   ").Apply());
    }

    [Fact]
    public void ApplyOnNamedSectionWithoutValueThrowsInvalidPropertyValueException()
    {
        ISettings settings = IniFile.Create();
        INamedSection section = settings.CreateSection(sectionName: "MySection", []);

        Assert.Throws<InvalidPropertyValueException>(() => section.CreateProperty("NewKey").Apply());
    }

    [Fact]
    public void ApplyWithDuplicateKeyAfterConcurrentSetThrowsDuplicatePropertyException()
    {
        // Set up a scenario where the key is added between CreateProperty and Apply
        // by setting directly on the section
        ISettings settings = IniFile.Create();
        IPropertyBuilder<ISettings> builder = settings.CreateProperty("NewKey").WithValue("somevalue");

        // Add the key directly to the settings to simulate the race condition check
        settings.Set(key: "NewKey", value: "original");

        Assert.Throws<DuplicatePropertyException>(builder.Apply);
    }

    [Fact]
    public void ApplyOnNamedSectionWithDuplicateKeyAfterConcurrentSetThrowsDuplicatePropertyException()
    {
        ISettings settings = IniFile.Create();
        INamedSection section = settings.CreateSection(sectionName: "MySection", []);
        IPropertyBuilder<INamedSection> builder = section.CreateProperty("NewKey").WithValue("somevalue");

        // Add the key directly to the section to simulate the race condition check
        section.Set(key: "NewKey", value: "original");

        Assert.Throws<DuplicatePropertyException>(builder.Apply);
    }

    [Fact]
    public void ApplyWithValidValueSetsProperty()
    {
        ISettings settings = IniFile.Create();

        settings.CreateProperty("MyKey").WithValue("MyValue").Apply();

        string? value = settings.Get("MyKey");
        Assert.Equal(expected: "MyValue", actual: value);
    }
}
