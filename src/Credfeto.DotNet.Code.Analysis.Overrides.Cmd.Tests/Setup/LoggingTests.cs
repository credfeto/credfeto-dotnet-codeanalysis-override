using System.Collections.Generic;
using System.Linq;
using Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Setup;
using FunFair.Test.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Xunit;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Tests.Setup;

public sealed class LoggingTests : IntegrationTestBase
{
    public LoggingTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void AddConsoleLoggingRegistersSingleLineConsoleProvider()
    {
        using (
            ServiceProvider services = new ServiceCollection()
                .AddLogging(builder => builder.ClearProviders().AddConsoleLogging())
                .BuildServiceProvider()
        )
        {
            IReadOnlyList<ILoggerProvider> providers = [.. services.GetServices<ILoggerProvider>()];
            SimpleConsoleFormatterOptions options = services
                .GetRequiredService<IOptionsMonitor<SimpleConsoleFormatterOptions>>()
                .CurrentValue;

            Assert.Equal(expected: 1, actual: providers.Count(provider => provider is ConsoleLoggerProvider));
            Assert.True(condition: options.SingleLine, userMessage: "Each log entry should be a single line");
            Assert.Equal(expected: "HH:mm:ss ", actual: options.TimestampFormat);
        }
    }
}
