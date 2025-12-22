using System;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini.Exceptions;

public sealed class InvalidPropertyNameException : Exception
{
    public InvalidPropertyNameException()
        : this("Invalid property name")
    {
    }

    public InvalidPropertyNameException(string message)
        : base(message)
    {
    }

    public InvalidPropertyNameException(string message, Exception innerException)
        : base(message: message, innerException: innerException)
    {
    }
}