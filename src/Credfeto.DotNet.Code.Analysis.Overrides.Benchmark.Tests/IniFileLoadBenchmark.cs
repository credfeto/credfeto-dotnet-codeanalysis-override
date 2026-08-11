using System.Diagnostics.CodeAnalysis;
using System.Text;
using BenchmarkDotNet.Attributes;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Benchmark.Tests;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
[SuppressMessage(
    category: "FunFair.CodeAnalysis",
    checkId: "FFS0012: Classes should be static, sealed or abstract",
    Justification = "BenchmarkDotNet requires an unsealed class to generate its benchmark harness subclass"
)]
public class IniFileLoadBenchmark
{
    private const int CommentLineCount = 2_000;

    private string _content = string.Empty;

    [GlobalSetup]
    public void GlobalSetup()
    {
        StringBuilder builder = new();

        for (int i = 0; i < CommentLineCount; ++i)
        {
            builder.Append("; comment line ").Append(i).Append('\n');
        }

        builder.Append("key = value\n");

        this._content = builder.ToString();
    }

    [Benchmark]
    public ISettings LoadLargeLeadingCommentBlock()
    {
        return IniFile.Load(this._content);
    }
}
