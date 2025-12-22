using System;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini.Exceptions;

public sealed class InvalidSectionNameException : Exception
{
    public InvalidSectionNameException()
        : this("Invalid section name")
    {
    }

    public InvalidSectionNameException(string message)
        : base(message)
    {
    }

    public InvalidSectionNameException(string message, Exception innerException)
        : base(message: message, innerException: innerException)
    {
    }
}