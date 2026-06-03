using System;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini.Exceptions;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Tests.Ini.Exceptions;

public sealed class UnknownFormatExceptionTests : IntegrationTestBase
{
    public UnknownFormatExceptionTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void DefaultConstructorHasExpectedMessage()
    {
        UnknownFormatException exception = new();

        Assert.Equal(expected: "Unknown line format", actual: exception.Message);
    }

    [Fact]
    public void MessageConstructorHasExpectedMessage()
    {
        UnknownFormatException exception = new("Custom message");

        Assert.Equal(expected: "Custom message", actual: exception.Message);
    }

    [Fact]
    public void MessageAndInnerExceptionConstructorHasExpectedMessageAndInner()
    {
        InvalidOperationException inner = new("Inner");
        UnknownFormatException exception = new("Custom message", inner);

        Assert.Equal(expected: "Custom message", actual: exception.Message);
        Assert.Same(expected: inner, actual: exception.InnerException);
    }
}
