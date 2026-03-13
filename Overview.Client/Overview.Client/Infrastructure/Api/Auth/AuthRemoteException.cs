using System.Net;

namespace Overview.Client.Infrastructure.Api.Auth;

public sealed class AuthRemoteException : Exception
{
    public AuthRemoteException(string message, HttpStatusCode? statusCode = null, string? responseBody = null)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    public HttpStatusCode? StatusCode { get; }

    public string? ResponseBody { get; }
}
