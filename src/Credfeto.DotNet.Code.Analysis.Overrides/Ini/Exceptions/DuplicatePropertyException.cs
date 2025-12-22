using System;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini.Exceptions;

public sealed class DuplicatePropertyException : Exception
{
    public DuplicatePropertyException()
        : this("Property already exists")
    {
    }

    public DuplicatePropertyException(string message)
        : base(message)
    {
    }

    public DuplicatePropertyException(string message, Exception innerException)
        : base(message: message, innerException: innerException)
    {
    }
}