namespace Osel.Core;

public class OselException : Exception
{
    public OselException(string message) : base(message) { }
    public OselException(string message, Exception innerException) : base(message, innerException) { }
}
