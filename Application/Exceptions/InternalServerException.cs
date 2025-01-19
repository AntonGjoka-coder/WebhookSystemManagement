using System.Net;
using System.Runtime.Serialization;

namespace Application.Exceptions;

public class InternalServerException : BaseException
{
    protected InternalServerException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public InternalServerException(string message, List<string>? errors = default, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message, errors, HttpStatusCode.InternalServerError)
    {
    }
}