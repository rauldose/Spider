namespace Spider.ConnectionManagement.Domain.Exceptions;

public class ConnectionException : Exception
{
    public ConnectionException(string message) : base(message) { }
    public ConnectionException(string message, Exception innerException) : base(message, innerException) { }
}

public class ConnectionNotFoundException : ConnectionException
{
    public ConnectionNotFoundException(Guid connectionId) 
        : base($"Connection with ID '{connectionId}' was not found") { }
}

public class InvalidConnectionParametersException : ConnectionException
{
    public InvalidConnectionParametersException(string message) : base(message) { }
}