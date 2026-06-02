using Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Constants;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Tests.Constants;

public sealed class ExitCodesTests : IntegrationTestBase
{
    public ExitCodesTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void SuccessIsZero()
    {
        Assert.Equal(expected: 0, actual: ExitCodes.Success);
    }

    [Fact]
    public void ErrorIsOne()
    {
        Assert.Equal(expected: 1, actual: ExitCodes.Error);
    }
}
