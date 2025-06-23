using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web;
using System.Web.Http.Filters;
using System.Web.Mvc;
using Autofac.Integration.WebApi;
using GCloud.Models.Domain;
using GCloud.Repository;
using GCloud.Shared.Exceptions;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;

namespace GCloud.ExceptionHandlers
{
    public class GlobalApiExceptionFilter : ExceptionFilterAttribute, IAutofacExceptionFilter
    {
        private readonly IErrorRepository _errorRepository;

        public GlobalApiExceptionFilter(IErrorRepository errorRepository)
        {
            _errorRepository = errorRepository;
        }

        public override async void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var exception = actionExecutedContext.Exception;

            var requestContentTask = actionExecutedContext.ActionContext.Request.Content.ReadAsStringAsync();

            var error = new Error
            {
                CreationDateTime = DateTime.Now,
                ExceptionType = exception.GetType().FullName,
                Message = exception.Message,
                StackTrace = exception.StackTrace,
                Url = actionExecutedContext.ActionContext.Request.RequestUri.ToString(),
                HttpMethod = actionExecutedContext.ActionContext.Request.Method.ToString(),
                UserId = actionExecutedContext.ActionContext.RequestContext.Principal?.Identity?.GetUserId(),
                IpAddress = GetIpAddress(actionExecutedContext.Request)
            };

            try
            {
                error.ParseExceptionData(exception);
            }
            catch
            {
                // ignored
            }

            if (exception is IGustavException gustavException)
            {
                var response = actionExecutedContext.Request.CreateResponse(gustavException.HttpStatusCode);
                response.Content = new ObjectContent<ExceptionHandlerResult>(new ExceptionHandlerResult
                {
                    Message = gustavException.HumanReadableMessage,
                    ErrorCode = gustavException.ErrorCode,
                    ExceptionType = gustavException.GetType().Name
                }, new JsonMediaTypeFormatter());
                actionExecutedContext.Response = response;

                foreach (var keyValuePair in gustavException.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(p => p.Name, p => p.GetValue(gustavException)))
                {
                    error.AddData(keyValuePair.Key, keyValuePair.Value);
                }
            }

            error.RequestContent = await requestContentTask;

            _errorRepository.Add(error);
        }

        private string GetIpAddress(HttpRequestMessage request)
        {
            string HttpContext = "MS_HttpContext";
            string RemoteEndpointMessage = "System.ServiceModel.Channels.RemoteEndpointMessageProperty";

            if (request.Properties.ContainsKey(HttpContext))
            {
                dynamic ctx = request.Properties[HttpContext];
                if (ctx != null)
                {
                    return ctx.Request.UserHostAddress;
                }
            }

            if (request.Properties.ContainsKey(RemoteEndpointMessage))
            {
                dynamic remoteEndpoint = request.Properties[RemoteEndpointMessage];
                if (remoteEndpoint != null)
                {
                    return remoteEndpoint.Address;
                }
            }

            return null;
        }
    }
}