using System;
using Credfeto.DotNet.Code.Analysis.Overrides.Ini.Exceptions;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Tests.Ini.Exceptions;

public sealed class SectionAlreadyExistsExceptionTests : IntegrationTestBase
{
    public SectionAlreadyExistsExceptionTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void DefaultConstructorHasExpectedMessage()
    {
        SectionAlreadyExistsException exception = new();

        Assert.Equal(expected: "Section already exists", actual: exception.Message);
    }

    [Fact]
    public void MessageConstructorHasExpectedMessage()
    {
        SectionAlreadyExistsException exception = new("Custom message");

        Assert.Equal(expected: "Custom message", actual: exception.Message);
    }

    [Fact]
    public void MessageAndInnerExceptionConstructorHasExpectedMessageAndInner()
    {
        InvalidOperationException inner = new("Inner");
        SectionAlreadyExistsException exception = new("Custom message", inner);

        Assert.Equal(expected: "Custom message", actual: exception.Message);
        Assert.Same(expected: inner, actual: exception.InnerException);
    }
}
