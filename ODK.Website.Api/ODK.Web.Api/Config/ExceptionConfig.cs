using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using ODK.Services.Exceptions;
using ODK.Web.Api.Errors;

namespace ODK.Web.Api.Config
{
    public static class ExceptionConfig
    {
        public static void RegisterExceptionHandling(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(a => a.Run(HandleException));
        }

        private static (ErrorApiResponse response, HttpStatusCode statusCode) CreateErrorResponse(Exception exception)
        {
            if (exception is OdkServiceException odkServiceException)
            {
                return (new ErrorApiResponse
                {
                    Messages = odkServiceException.Messages
                }, HttpStatusCode.BadRequest);
            }

            if (exception is OdkNotFoundException)
            {
                return (new ErrorApiResponse(), HttpStatusCode.NotFound);
            }

            if (exception is OdkNotAuthorizedException)
            {
                return (new ErrorApiResponse(), HttpStatusCode.Forbidden);
            }

            return (new ErrorApiResponse(), HttpStatusCode.InternalServerError);
        }

        private static async Task HandleException(HttpContext context)
        {
            IExceptionHandlerPathFeature exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            Exception exception = exceptionHandlerPathFeature.Error;

            (ErrorApiResponse response, HttpStatusCode statusCode) = CreateErrorResponse(exception);

            string result = JsonConvert.SerializeObject(response, JsonConfig.SerializerSettings);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsync(result);
        }
    }
}
