using System;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini.Exceptions;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Tests.Ini.Exceptions;

public sealed class InvalidSettingsExceptionTests : IntegrationTestBase
{
    public InvalidSettingsExceptionTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void DefaultConstructorHasExpectedMessage()
    {
        InvalidSettingsException exception = new();

        Assert.Equal(expected: "Invalid settings", actual: exception.Message);
    }

    [Fact]
    public void MessageConstructorHasExpectedMessage()
    {
        InvalidSettingsException exception = new("Custom message");

        Assert.Equal(expected: "Custom message", actual: exception.Message);
    }

    [Fact]
    public void MessageAndInnerExceptionConstructorHasExpectedMessageAndInner()
    {
        InvalidOperationException inner = new("Inner");
        InvalidSettingsException exception = new("Custom message", inner);

        Assert.Equal(expected: "Custom message", actual: exception.Message);
        Assert.Same(expected: inner, actual: exception.InnerException);
    }
}
