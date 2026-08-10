using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini.Exceptions;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Tests.Ini;

public sealed class IniFileTests : IntegrationTestBase
{
    public IniFileTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task LoadAsyncReturnsEmptySettingsForEmptyFileAsync()
    {
        CancellationToken cancellationToken = this.CancellationToken();
        string tempFile = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(
                path: tempFile,
                contents: string.Empty,
                encoding: Encoding.UTF8,
                cancellationToken: cancellationToken
            );

            ISettings settings = await IniFile.LoadAsync(fileName: tempFile, cancellationToken: cancellationToken);

            Assert.NotNull(settings);
            string saved = settings.Save();
            Assert.Equal(expected: string.Empty, actual: saved);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadAsyncReturnsSettingsWithGlobalPropertyAsync()
    {
        CancellationToken cancellationToken = this.CancellationToken();
        string tempFile = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(
                path: tempFile,
                contents: "global = true\n",
                encoding: Encoding.UTF8,
                cancellationToken: cancellationToken
            );

            ISettings settings = await IniFile.LoadAsync(fileName: tempFile, cancellationToken: cancellationToken);

            Assert.NotNull(settings);
            string? value = settings.Get("global");
            Assert.Equal(expected: "true", actual: value);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task SaveAsyncWritesSettingsToFileAsync()
    {
        CancellationToken cancellationToken = this.CancellationToken();
        string tempFile = Path.GetTempFileName();

        try
        {
            ISettings settings = IniFile.Create();
            settings.Set(key: "mykey", value: "myvalue");

            await IniFile.SaveAsync(fileName: tempFile, settings: settings, cancellationToken: cancellationToken);

            string content = await File.ReadAllTextAsync(path: tempFile, cancellationToken: cancellationToken);
            Assert.Contains("mykey", content, StringComparison.Ordinal);
            Assert.Contains("myvalue", content, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task RoundTripLoadAsyncAndSaveAsyncPreservesContentAsync()
    {
        CancellationToken cancellationToken = this.CancellationToken();
        string sourceTempFile = Path.GetTempFileName();
        string destTempFile = Path.GetTempFileName();

        try
        {
            const string original = "key1 = value1\n";
            await File.WriteAllTextAsync(
                path: sourceTempFile,
                contents: original,
                encoding: Encoding.UTF8,
                cancellationToken: cancellationToken
            );

            ISettings settings = await IniFile.LoadAsync(
                fileName: sourceTempFile,
                cancellationToken: cancellationToken
            );
            await IniFile.SaveAsync(fileName: destTempFile, settings: settings, cancellationToken: cancellationToken);

            string saved = await File.ReadAllTextAsync(path: destTempFile, cancellationToken: cancellationToken);
            Assert.Contains("key1", saved, StringComparison.Ordinal);
            Assert.Contains("value1", saved, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(sourceTempFile);
            File.Delete(destTempFile);
        }
    }

    [Fact]
    public async Task LoadAsyncThrowsInvalidSettingsExceptionForUnknownLineFormatAsync()
    {
        CancellationToken cancellationToken = this.CancellationToken();
        string tempFile = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(
                path: tempFile,
                contents: "!!!invalidline!!!\n",
                encoding: Encoding.UTF8,
                cancellationToken: cancellationToken
            );

            await Assert.ThrowsAsync<InvalidSettingsException>(() =>
                IniFile.LoadAsync(fileName: tempFile, cancellationToken: cancellationToken).AsTask()
            );
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task RoundTripLoadAsyncAndSaveAsyncPreservesTrailingCommentsAfterPropertyAsync()
    {
        CancellationToken cancellationToken = this.CancellationToken();
        string sourceTempFile = Path.GetTempFileName();
        string destTempFile = Path.GetTempFileName();

        try
        {
            const string original =
                "dotnet_diagnostic.CA1002.severity = error\n\n# TODO: revisit these suppressions when analyzer X is fixed\n# See discussion in PR 123\n";
            await File.WriteAllTextAsync(
                path: sourceTempFile,
                contents: original,
                encoding: Encoding.UTF8,
                cancellationToken: cancellationToken
            );

            ISettings settings = await IniFile.LoadAsync(
                fileName: sourceTempFile,
                cancellationToken: cancellationToken
            );
            await IniFile.SaveAsync(fileName: destTempFile, settings: settings, cancellationToken: cancellationToken);

            string saved = await File.ReadAllTextAsync(path: destTempFile, cancellationToken: cancellationToken);
            int propertyIndex = saved.IndexOf("dotnet_diagnostic.CA1002.severity", StringComparison.Ordinal);
            int firstCommentIndex = saved.IndexOf(
                "TODO: revisit these suppressions when analyzer X is fixed",
                StringComparison.Ordinal
            );
            int secondCommentIndex = saved.IndexOf("See discussion in PR 123", StringComparison.Ordinal);

            Assert.True(propertyIndex >= 0, "property line missing from saved content");
            Assert.True(
                firstCommentIndex > propertyIndex,
                "trailing comment must appear after the property, not before or attached to it"
            );
            Assert.True(secondCommentIndex > firstCommentIndex, "second trailing comment line must follow the first");
            Assert.True(
                saved.TrimEnd().EndsWith("# See discussion in PR 123", StringComparison.Ordinal),
                "trailing comment block must be the last content in the saved file"
            );
        }
        finally
        {
            File.Delete(sourceTempFile);
            File.Delete(destTempFile);
        }
    }

    [Fact]
    public async Task RoundTripLoadAsyncAndSaveAsyncPreservesTrailingCommentsInCommentOnlyFileAsync()
    {
        CancellationToken cancellationToken = this.CancellationToken();
        string sourceTempFile = Path.GetTempFileName();
        string destTempFile = Path.GetTempFileName();

        try
        {
            const string original = "# Just a comment\n# Another comment line\n";
            await File.WriteAllTextAsync(
                path: sourceTempFile,
                contents: original,
                encoding: Encoding.UTF8,
                cancellationToken: cancellationToken
            );

            ISettings settings = await IniFile.LoadAsync(
                fileName: sourceTempFile,
                cancellationToken: cancellationToken
            );
            await IniFile.SaveAsync(fileName: destTempFile, settings: settings, cancellationToken: cancellationToken);

            string saved = await File.ReadAllTextAsync(path: destTempFile, cancellationToken: cancellationToken);
            Assert.False(
                string.IsNullOrEmpty(saved),
                "comment-only file must not round-trip to an empty save (pre-fix behaviour)"
            );

            int firstCommentIndex = saved.IndexOf("Just a comment", StringComparison.Ordinal);
            int secondCommentIndex = saved.IndexOf("Another comment line", StringComparison.Ordinal);

            Assert.True(firstCommentIndex >= 0, "first comment missing from saved content");
            Assert.True(secondCommentIndex > firstCommentIndex, "second comment line must follow the first");
        }
        finally
        {
            File.Delete(sourceTempFile);
            File.Delete(destTempFile);
        }
    }

    [Fact]
    public async Task RoundTripLoadAsyncAndSaveAsyncPreservesTrailingCommentsAfterNamedSectionAsync()
    {
        CancellationToken cancellationToken = this.CancellationToken();
        string sourceTempFile = Path.GetTempFileName();
        string destTempFile = Path.GetTempFileName();

        try
        {
            const string original = "[MySect]\nkey = val\n\n# trailing comment\n";
            await File.WriteAllTextAsync(
                path: sourceTempFile,
                contents: original,
                encoding: Encoding.UTF8,
                cancellationToken: cancellationToken
            );

            ISettings settings = await IniFile.LoadAsync(
                fileName: sourceTempFile,
                cancellationToken: cancellationToken
            );
            await IniFile.SaveAsync(fileName: destTempFile, settings: settings, cancellationToken: cancellationToken);

            string saved = await File.ReadAllTextAsync(path: destTempFile, cancellationToken: cancellationToken);
            int sectionIndex = saved.IndexOf("[MySect]", StringComparison.Ordinal);
            int commentIndex = saved.IndexOf("trailing comment", StringComparison.Ordinal);

            Assert.True(sectionIndex >= 0, "named section missing from saved content");
            Assert.True(commentIndex > sectionIndex, "trailing comment must appear after the named section");
            Assert.True(
                saved.TrimEnd().EndsWith("# trailing comment", StringComparison.Ordinal),
                "trailing comment block must be the last content, separated from the section by exactly one blank line"
            );
        }
        finally
        {
            File.Delete(sourceTempFile);
            File.Delete(destTempFile);
        }
    }

    [Fact]
    public async Task LoadAsyncWithNamedSectionReturnsCorrectSectionAsync()
    {
        CancellationToken cancellationToken = this.CancellationToken();
        string tempFile = Path.GetTempFileName();

        try
        {
            const string content = "[MySect]\nkey = val\n";
            await File.WriteAllTextAsync(
                path: tempFile,
                contents: content,
                encoding: Encoding.UTF8,
                cancellationToken: cancellationToken
            );

            ISettings settings = await IniFile.LoadAsync(fileName: tempFile, cancellationToken: cancellationToken);

            INamedSection? section = settings.GetSection("MySect");
            Assert.NotNull(section);
            Assert.Equal(expected: "val", actual: section.Get("key"));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
