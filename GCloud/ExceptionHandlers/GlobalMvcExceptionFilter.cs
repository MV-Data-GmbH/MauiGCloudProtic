using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Autofac.Integration.WebApi;
using GCloud.Models.Domain;
using GCloud.Repository;
using GCloud.Shared.Exceptions;
using Microsoft.AspNet.Identity;

namespace GCloud.ExceptionHandlers
{
    public class GlobalMvcExceptionFilter : IExceptionFilter
    {
        private readonly IErrorRepository _errorRepository;

        public GlobalMvcExceptionFilter(IErrorRepository errorRepository)
        {
            _errorRepository = errorRepository;
        }

        public void OnException(ExceptionContext filterContext)
        {
            var exception = filterContext.Exception;

            var error = new Error
            {
                CreationDateTime = DateTime.Now,
                ExceptionType = exception.GetType().FullName,
                Message = exception.Message,
                StackTrace = exception.StackTrace,
                Url = filterContext.HttpContext.Request.Url?.ToString(),
                HttpMethod = filterContext.HttpContext.Request.HttpMethod,
                UserId = filterContext.RequestContext.HttpContext.User.Identity.GetUserId(),
                IpAddress = filterContext.HttpContext.Request.UserHostAddress
            };

            if (exception is IGustavException gustavException)
            {
                foreach (var keyValuePair in gustavException.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(p => p.Name, p => p.GetValue(gustavException)))
                {
                    error.AddData(keyValuePair.Key, keyValuePair.Value);
                }
            }

            _errorRepository.Add(error);
        }
    }
}