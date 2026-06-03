using System;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini.Exceptions;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Tests.Ini.Exceptions;

public sealed class InvalidPropertyValueExceptionTests : IntegrationTestBase
{
    public InvalidPropertyValueExceptionTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void DefaultConstructorHasExpectedMessage()
    {
        InvalidPropertyValueException exception = new();

        Assert.Equal(expected: "Invalid property value", actual: exception.Message);
    }

    [Fact]
    public void MessageConstructorHasExpectedMessage()
    {
        InvalidPropertyValueException exception = new("Custom message");

        Assert.Equal(expected: "Custom message", actual: exception.Message);
    }

    [Fact]
    public void MessageAndInnerExceptionConstructorHasExpectedMessageAndInner()
    {
        InvalidOperationException inner = new("Inner");
        InvalidPropertyValueException exception = new("Custom message", inner);

        Assert.Equal(expected: "Custom message", actual: exception.Message);
        Assert.Same(expected: inner, actual: exception.InnerException);
    }
}
