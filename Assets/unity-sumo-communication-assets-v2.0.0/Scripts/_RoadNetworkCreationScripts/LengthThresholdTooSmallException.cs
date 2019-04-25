using System;

internal class LengthThresholdTooSmallException : Exception
{
    public LengthThresholdTooSmallException(string message) : base(message)
    {
    }

    public LengthThresholdTooSmallException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public LengthThresholdTooSmallException()
    {
    }
}

