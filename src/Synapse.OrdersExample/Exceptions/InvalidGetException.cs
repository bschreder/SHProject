namespace Synapse.OrdersExample.Exceptions;

[Serializable]
public class InvalidGetException : Exception
{

    /// <summary>
    /// Default CTOR
    /// </summary>
    public InvalidGetException() : base()
    {
    }

    /// <summary>
    /// CTOR with message
    /// </summary>
    /// <param name="message">exception message</param>
    public InvalidGetException(string? message) : base(message)
    {
    }

    /// <summary>
    /// CTOR with message and inner exception
    /// </summary>
    /// <param name="message">exception message </param>
    /// <param name="innerException">inner exception </param>
    public InvalidGetException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
