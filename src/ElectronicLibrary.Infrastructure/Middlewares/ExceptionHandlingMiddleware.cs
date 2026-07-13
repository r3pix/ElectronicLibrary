using System.Net;
using ElectronicLibrary.Domain.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace ElectronicLibrary.Infrastructure.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await WriteResponseAsync(context, HttpStatusCode.BadRequest,
                string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)));
        }
        catch (KeyNotFoundException ex)
        {
            await WriteResponseAsync(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (Exception ex)
        {
            await WriteResponseAsync(context, HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    private static Task WriteResponseAsync(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new Response<object?>(null)
        {
            Code = statusCode,
            IsError = true,
            Message = message
        };
        return context.Response.WriteAsJsonAsync(response);
    }
}
