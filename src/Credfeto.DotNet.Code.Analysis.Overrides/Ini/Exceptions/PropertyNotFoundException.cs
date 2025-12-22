using System;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini.Exceptions;

public sealed class PropertyNotFoundException : Exception
{
    public PropertyNotFoundException()
        : this("Property not found")
    {
    }

    public PropertyNotFoundException(string message)
        : base(message)
    {
    }

    public PropertyNotFoundException(string message, Exception innerException)
        : base(message: message, innerException: innerException)
    {
    }
}