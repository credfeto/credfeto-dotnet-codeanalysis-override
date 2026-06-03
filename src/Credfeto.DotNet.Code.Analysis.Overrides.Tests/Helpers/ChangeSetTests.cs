using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.DotNet.Code.Analysis.Overrides.Helpers;
using Credfeto.DotNet.Code.Analysis.Overrides.Models;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Tests.Helpers;

public sealed class ChangeSetTests : IntegrationTestBase
{
    public ChangeSetTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task LoadAsyncReturnsEmptyListForEmptyArrayAsync()
    {
        CancellationToken cancellationToken = this.CancellationToken();
        string tempFile = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(
                path: tempFile,
                contents: "[]",
                encoding: Encoding.UTF8,
                cancellationToken: cancellationToken
            );

            IReadOnlyList<RuleChange> result = await ChangeSet.LoadAsync(
                changesFileName: tempFile,
                cancellationToken: cancellationToken
            );

            Assert.NotNull(result);
            Assert.Empty(result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadAsyncReturnsSingleItemFromJsonAsync()
    {
        CancellationToken cancellationToken = this.CancellationToken();
        string tempFile = Path.GetTempFileName();

        try
        {
            const string json = """
                [
                    {
                        "ruleSet": "MyAnalyzer",
                        "rule": "MA0001",
                        "state": "ERROR",
                        "description": "Some description"
                    }
                ]
                """;

            await File.WriteAllTextAsync(
                path: tempFile,
                contents: json,
                encoding: Encoding.UTF8,
                cancellationToken: cancellationToken
            );

            IReadOnlyList<RuleChange> result = await ChangeSet.LoadAsync(
                changesFileName: tempFile,
                cancellationToken: cancellationToken
            );

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(expected: "MyAnalyzer", actual: result[0].RuleSet);
            Assert.Equal(expected: "MA0001", actual: result[0].Rule);
            Assert.Equal(expected: "ERROR", actual: result[0].State);
            Assert.Equal(expected: "Some description", actual: result[0].Description);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadAsyncReturnsMultipleItemsFromJsonAsync()
    {
        CancellationToken cancellationToken = this.CancellationToken();
        string tempFile = Path.GetTempFileName();

        try
        {
            const string json = """
                [
                    {
                        "ruleSet": "AnalyzerA",
                        "rule": "AA0001",
                        "state": "ERROR",
                        "description": "First rule"
                    },
                    {
                        "ruleSet": "AnalyzerB",
                        "rule": "AB0002",
                        "state": "NONE",
                        "description": "Second rule"
                    }
                ]
                """;

            await File.WriteAllTextAsync(
                path: tempFile,
                contents: json,
                encoding: Encoding.UTF8,
                cancellationToken: cancellationToken
            );

            IReadOnlyList<RuleChange> result = await ChangeSet.LoadAsync(
                changesFileName: tempFile,
                cancellationToken: cancellationToken
            );

            Assert.NotNull(result);
            Assert.Equal(expected: 2, actual: result.Count);
            Assert.Equal(expected: "AnalyzerA", actual: result[0].RuleSet);
            Assert.Equal(expected: "AnalyzerB", actual: result[1].RuleSet);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
