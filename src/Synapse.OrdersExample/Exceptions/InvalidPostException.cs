namespace Synapse.OrdersExample.Exceptions;

[Serializable]
public class InvalidPostException : Exception
{

    /// <summary>
    /// Default CTOR
    /// </summary>
    public InvalidPostException() : base()
    {
    }

    /// <summary>
    /// CTOR with message
    /// </summary>
    /// <param name="message">exception message</param>
    public InvalidPostException(string? message) : base(message)
    {
    }

    /// <summary>
    /// CTOR with message and inner exception
    /// </summary>
    /// <param name="message">exception message </param>
    /// <param name="innerException">inner exception </param>
    public InvalidPostException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}