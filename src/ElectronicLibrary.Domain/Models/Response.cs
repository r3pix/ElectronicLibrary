using System.Net;

namespace ElectronicLibrary.Domain.Models;

public class Response<T>(T result)
{
    public T Result { get; set; } = result;
    public HttpStatusCode Code { get; set; } = HttpStatusCode.OK;
    public string Message { get; set; } = string.Empty;
    public bool IsError { get; set; }
}
