using System.Net;

namespace FactorioNexus.ModPortal;

public class RequestException : Exception
{
    public readonly HttpStatusCode? HttpStatusCode;

    public RequestException(string message)
        : base(message) { }

    public RequestException(string message, Exception innerException)
        : base(message, innerException) { }

    public RequestException(string message, HttpStatusCode httpStatusCode)
        : base(message) => HttpStatusCode = httpStatusCode;

    public RequestException(string message, HttpStatusCode httpStatusCode, Exception? innerException)
        : base(message, innerException) => HttpStatusCode = httpStatusCode;
}