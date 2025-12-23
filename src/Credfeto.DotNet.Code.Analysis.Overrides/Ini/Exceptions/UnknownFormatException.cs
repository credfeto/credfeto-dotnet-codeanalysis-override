using System;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini.Exceptions;

public sealed class UnknownFormatException : Exception
{
    public UnknownFormatException()
        : this("Unknown line format")
    {
    }

    public UnknownFormatException(string message)
        : base(message)
    {
    }

    public UnknownFormatException(string message, Exception innerException)
        : base(message: message, innerException: innerException)
    {
    }
}