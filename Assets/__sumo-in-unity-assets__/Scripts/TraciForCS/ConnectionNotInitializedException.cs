using System;
using System.Runtime.Serialization;

[Serializable]
internal class ConnectionNotInitializedException : Exception
{
    public ConnectionNotInitializedException()
    {
    }

    public ConnectionNotInitializedException(string message) : base(message)
    {
    }

    public ConnectionNotInitializedException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected ConnectionNotInitializedException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}