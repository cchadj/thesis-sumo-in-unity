using System;
using System.Runtime.Serialization;

[Serializable]
internal class NoVehicleSelectedException : Exception
{
    public NoVehicleSelectedException()
    {
    }

    public NoVehicleSelectedException(string message) : base(message)
    {
    }

    public NoVehicleSelectedException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected NoVehicleSelectedException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}