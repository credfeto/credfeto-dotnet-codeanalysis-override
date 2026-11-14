using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Cocona;
using Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Constants;
using FunFair.Test.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
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

    [Fact]
    public async Task MainRunsCommandWithEmptyChangesAndLeavesSuccessExitCode()
    {
        string changesFile = Path.GetTempFileName();
        int originalExitCode = Environment.ExitCode;

        try
        {
            await File.WriteAllTextAsync(
                path: changesFile,
                contents: "[]",
                encoding: Encoding.UTF8,
                cancellationToken: this.CancellationToken()
            );
            Environment.ExitCode = ExitCodes.Error;

            await Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Program.Main([
                "ruleset",
                "-r",
                "non-existent.ruleset",
                "-c",
                changesFile,
            ]);

            Assert.Equal(expected: ExitCodes.Success, actual: Environment.ExitCode);
        }
        finally
        {
            Environment.ExitCode = originalExitCode;
            File.Delete(changesFile);
        }
    }

    [Fact]
    public async Task RunGuardedAsyncHandlesThrownExceptionAndSetsErrorExitCode()
    {
        ILoggerFactory loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(this.GetTypedLogger<ProgramTests>());
        int originalExitCode = Environment.ExitCode;

        try
        {
            Environment.ExitCode = ExitCodes.Success;

            await Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Program.RunGuardedAsync(
                run: () => throw new IOException("Test failure"),
                loggerFactory: loggerFactory
            );

            Assert.Equal(expected: ExitCodes.Error, actual: Environment.ExitCode);
            loggerFactory.Received(1).CreateLogger("Program");
        }
        finally
        {
            Environment.ExitCode = originalExitCode;
        }
    }

    [Theory]
    [InlineData(nameof(FileNotFoundException))]
    [InlineData(nameof(DirectoryNotFoundException))]
    [InlineData(nameof(IOException))]
    [InlineData(nameof(UnauthorizedAccessException))]
    [InlineData(nameof(XmlException))]
    [InlineData(nameof(OperationCanceledException))]
    [InlineData(nameof(InvalidOperationException))]
    public void HandleUnhandledExceptionReturnsErrorExitCode(string exceptionType)
    {
        Exception exception = CreateException(exceptionType);

        int exitCode = Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Program.HandleUnhandledException(
            exception: exception,
            logger: this.GetTypedLogger<ProgramTests>()
        );

        Assert.Equal(expected: ExitCodes.Error, actual: exitCode);
    }

    [Fact]
    public void HandleUnhandledExceptionWithLoggerFactoryCreatesProgramLoggerAndReturnsErrorExitCode()
    {
        ILoggerFactory loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(this.GetTypedLogger<ProgramTests>());

        int exitCode = Credfeto.DotNet.Code.Analysis.Overrides.Cmd.Program.HandleUnhandledException(
            exception: new InvalidOperationException("Test failure"),
            loggerFactory: loggerFactory
        );

        Assert.Equal(expected: ExitCodes.Error, actual: exitCode);
        loggerFactory.Received(1).CreateLogger("Program");
    }

    // Exceptions are not xunit-serializable, so the theory is keyed by type name instead.
    private static Exception CreateException(string exceptionType)
    {
        const string message = "Test failure";

        return exceptionType switch
        {
            nameof(FileNotFoundException) => new FileNotFoundException(message),
            nameof(DirectoryNotFoundException) => new DirectoryNotFoundException(message),
            nameof(IOException) => new IOException(message),
            nameof(UnauthorizedAccessException) => new UnauthorizedAccessException(message),
            nameof(XmlException) => new XmlException(message),
            nameof(OperationCanceledException) => new OperationCanceledException(message),
            nameof(InvalidOperationException) => new InvalidOperationException(message),
            _ => throw new ArgumentOutOfRangeException(
                nameof(exceptionType),
                actualValue: exceptionType,
                message: "Unknown exception type"
            ),
        };
    }
}
