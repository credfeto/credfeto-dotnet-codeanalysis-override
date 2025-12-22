using System;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini.Exceptions;

public sealed class InvalidPropertyValueException : Exception
{
    public InvalidPropertyValueException()
        : this("Invalid property value")
    {
    }

    public InvalidPropertyValueException(string message)
        : base(message)
    {
    }

    public InvalidPropertyValueException(string message, Exception innerException)
        : base(message: message, innerException: innerException)
    {
    }
}