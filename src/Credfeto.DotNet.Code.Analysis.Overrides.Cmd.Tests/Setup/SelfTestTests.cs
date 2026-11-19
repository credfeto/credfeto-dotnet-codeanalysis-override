using System;
using System.IO;
using System.Threading.Tasks;
using Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Constants;
using Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Setup;
using FunFair.Test.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Tests.Setup;

public sealed class SelfTestTests : IntegrationTestBase
{
    public SelfTestTests(ITestOutputHelper output)
        : base(output) { }

    [Theory]
    [InlineData("--self-test", true)]
    [InlineData("ruleset --self-test", true)]
    [InlineData("--SELF-TEST", false)]
    [InlineData("self-test", false)]
    [InlineData("ruleset", false)]
    [InlineData("", false)]
    public static void IsRequestedDetectsSwitch(string commandLine, bool expected)
    {
        bool requested = SelfTest.IsRequested(commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        Assert.Equal(expected: expected, actual: requested);
    }

    [Fact]
    public async Task RunWithHealthyAppReturnsSuccessAndWritesNothingToErrorAsync()
    {
        await using (StringWriter error = new())
        {
            int exitCode = await SelfTest.RunAsync(
                createServices: Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Program.CreateServices,
                error: error,
                cancellationToken: this.CancellationToken()
            );

            Assert.Equal(expected: ExitCodes.Success, actual: exitCode);
            Assert.Empty(error.ToString());
        }
    }

    [Fact]
    public async Task RunWithFailingStartupReturnsErrorAndWritesOneLineReasonAsync()
    {
        await using (StringWriter error = new())
        {
            int exitCode = await SelfTest.RunAsync(
                createServices: () =>
                    throw new InvalidOperationException("First line" + Environment.NewLine + "second line"),
                error: error,
                cancellationToken: this.CancellationToken()
            );

            Assert.Equal(expected: ExitCodes.Error, actual: exitCode);
            string[] lines = error.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            string line = Assert.Single(lines);
            Assert.Equal(expected: "Self-test failed: InvalidOperationException: First line second line", actual: line);
        }
    }

    [Fact]
    public async Task RunWithUnresolvableServiceReturnsErrorAsync()
    {
        await using (StringWriter error = new())
        {
            int exitCode = await SelfTest.RunAsync(
                createServices: CreateServicesWithoutLogging,
                error: error,
                cancellationToken: this.CancellationToken()
            );

            Assert.Equal(expected: ExitCodes.Error, actual: exitCode);
            Assert.StartsWith(
                expectedStartString: "Self-test failed: ",
                actualString: error.ToString(),
                comparisonType: StringComparison.Ordinal
            );
        }
    }

    [Fact]
    public void VerifyWithMissingServicesThrows()
    {
        using (ServiceProvider services = new ServiceCollection().BuildServiceProvider())
        {
            Assert.Throws<InvalidOperationException>(() => SelfTest.Verify(services));
        }
    }

    [Fact]
    public void VerifyWithLoggingButNoCommandLoggerThrows()
    {
        using (
            ServiceProvider services = new ServiceCollection()
                .AddSingleton(Substitute.For<ILoggerFactory>())
                .BuildServiceProvider()
        )
        {
            Assert.Throws<InvalidOperationException>(() => SelfTest.Verify(services));
        }
    }

    private static ServiceProvider CreateServicesWithoutLogging()
    {
        return new ServiceCollection()
            .AddSingleton<ILoggerFactory>(_ => throw new InvalidOperationException("Logging is unavailable"))
            .BuildServiceProvider();
    }
}
