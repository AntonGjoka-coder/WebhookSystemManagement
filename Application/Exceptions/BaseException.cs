using System.Net;
using System.Runtime.Serialization;

namespace Application.Exceptions;

public class BaseException : Exception
{
    public List<string>? ErrorMessages { get; set; }
    public HttpStatusCode StatusCode { get; set; }

    protected BaseException(SerializationInfo info, StreamingContext context) : base(info,context)
    {
    }

    public BaseException(string message, List<string>? errors = default,
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message)
    {
        ErrorMessages = errors;
        statusCode = statusCode;
    }
}