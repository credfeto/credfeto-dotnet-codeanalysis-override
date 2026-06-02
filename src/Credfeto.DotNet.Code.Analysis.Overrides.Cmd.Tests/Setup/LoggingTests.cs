using Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Setup;
using FunFair.Test.Common;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Tests.Setup;

public sealed class LoggingTests : IntegrationTestBase
{
    public LoggingTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void InitializeLoggingDoesNotThrow()
    {
        ILoggerFactory loggerFactory = GetSubstitute<ILoggerFactory>();

        Logging.InitializeLogging(loggerFactory);
    }
}
