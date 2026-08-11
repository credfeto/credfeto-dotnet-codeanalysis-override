using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Benchmark.Tests;

public sealed class IniFileLoadBenchmarkTests : LoggingTestBase
{
    public IniFileLoadBenchmarkTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void LoadLargeLeadingCommentBlock_AllocatesLinearlyInCommentCount()
    {
        (Summary summary, AccumulationLogger logger) = Benchmark<IniFileLoadBenchmark>();

        this.Output.WriteLine(logger.GetLog());

        // Measured locally at ~1.34 MB allocated for 2,000 comment lines with the
        // ImmutableArray<string>.Builder accumulation. The previous ImmutableArray<string>.Add
        // accumulation was O(N^2) in copies - roughly 16 MB of copying alone for this input size.
        // 4 MB leaves generous headroom above the measured linear cost while still failing if the
        // quadratic accumulation regresses. The CI test job excludes this project
        // (--filter-not-namespace "*.Benchmark.Tests" in .github/actions/dotnet/action.yml); the
        // pre-commit hook runs it when staged changes affect it.
        summary.AssertAllocationsAtMost(maximumBytes: 4_000_000);
    }
}
