using System;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini.Exceptions;

public sealed class SectionAlreadyExistsException : Exception
{
    public SectionAlreadyExistsException()
        : this("Section already exists")
    {
    }

    public SectionAlreadyExistsException(string message)
        : base(message)
    {
    }

    public SectionAlreadyExistsException(string message, Exception innerException)
        : base(message: message, innerException: innerException)
    {
    }
}