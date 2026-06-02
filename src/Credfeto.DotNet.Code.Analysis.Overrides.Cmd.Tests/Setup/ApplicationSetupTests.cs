using Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Setup;
using FunFair.Test.Common;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Tests.Setup;

public sealed class ApplicationSetupTests : IntegrationTestBase
{
    public ApplicationSetupTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void AddServicesReturnsSameCollection()
    {
        IServiceCollection services = new ServiceCollection();

        IServiceCollection result = services.AddServices();

        Assert.Same(expected: services, actual: result);
    }
}
