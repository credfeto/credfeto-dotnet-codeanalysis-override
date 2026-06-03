using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Credfeto.DotNet.Code.Analysis.Overrides.Helpers;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Tests.Helpers;

public sealed class RuleSetTests : IntegrationTestBase
{
    public RuleSetTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task LoadAsyncReturnsXmlDocumentAsync()
    {
        CancellationToken cancellationToken = this.CancellationToken();
        string tempFile = Path.GetTempFileName();

        try
        {
            const string xml = """
                <RuleSet>
                  <Rules AnalyzerId="MyAnalyzer">
                    <Rule Id="MA0001" Action="error" />
                  </Rules>
                </RuleSet>
                """;

            await File.WriteAllTextAsync(
                path: tempFile,
                contents: xml,
                encoding: Encoding.UTF8,
                cancellationToken: cancellationToken
            );

            XmlDocument doc = await RuleSet.LoadAsync(tempFile);

            Assert.NotNull(doc);
            XmlElement? element =
                doc.SelectSingleNode("//RuleSet/Rules[@AnalyzerId='MyAnalyzer']/Rule[@Id='MA0001']") as XmlElement;
            Assert.NotNull(element);
            Assert.Equal(expected: "error", actual: element.GetAttribute("Action"));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task SaveAsyncWritesXmlDocumentToFileAsync()
    {
        CancellationToken cancellationToken = this.CancellationToken();
        string tempFile = Path.GetTempFileName();

        try
        {
            XmlDocument doc = new();
            doc.LoadXml(
                """
                <RuleSet>
                  <Rules AnalyzerId="MyAnalyzer">
                    <Rule Id="MA0001" Action="suggestion" />
                  </Rules>
                </RuleSet>
                """
            );

            await RuleSet.SaveAsync(project: tempFile, doc: doc);

            Assert.True(File.Exists(tempFile), "File should exist after SaveAsync");
            string content = await File.ReadAllTextAsync(path: tempFile, cancellationToken: cancellationToken);
            Assert.Contains("MA0001", content, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task RoundTripLoadAndSavePreservesRulesAsync()
    {
        CancellationToken cancellationToken = this.CancellationToken();
        string sourceTempFile = Path.GetTempFileName();
        string destTempFile = Path.GetTempFileName();

        try
        {
            const string xml = """
                <RuleSet>
                  <Rules AnalyzerId="AnalyzerX">
                    <Rule Id="AX001" Action="none" />
                  </Rules>
                </RuleSet>
                """;

            await File.WriteAllTextAsync(
                path: sourceTempFile,
                contents: xml,
                encoding: Encoding.UTF8,
                cancellationToken: cancellationToken
            );

            XmlDocument doc = await RuleSet.LoadAsync(sourceTempFile);
            await RuleSet.SaveAsync(project: destTempFile, doc: doc);

            string saved = await File.ReadAllTextAsync(path: destTempFile, cancellationToken: cancellationToken);
            Assert.Contains("AX001", saved, StringComparison.Ordinal);
            Assert.Contains("none", saved, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(sourceTempFile);
            File.Delete(destTempFile);
        }
    }
}
