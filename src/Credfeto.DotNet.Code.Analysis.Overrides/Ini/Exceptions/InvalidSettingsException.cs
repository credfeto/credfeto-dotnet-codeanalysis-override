using System;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Ini.Exceptions;

public sealed class InvalidSettingsException : Exception
{
    public InvalidSettingsException()
        : this("Invalid settings")
    {
    }

    public InvalidSettingsException(string message)
        : base(message)
    {
    }

    public InvalidSettingsException(string message, Exception innerException)
        : base(message: message, innerException: innerException)
    {
    }
}