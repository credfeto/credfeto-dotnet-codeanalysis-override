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
