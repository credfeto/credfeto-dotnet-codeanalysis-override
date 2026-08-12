using Cocona;
using FunFair.Test.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Tests;

public sealed class ProgramTests : IntegrationTestBase
{
    public ProgramTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void CreateAppBuildsHostWithLoggingConfigured()
    {
        // Fully qualified: unqualified "Program" does not resolve to Cmd.Program here.
        using (CoconaApp host = Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Program.CreateApp([]))
        {
            ILoggerFactory loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();

            Assert.NotNull(loggerFactory);
        }
    }
}
