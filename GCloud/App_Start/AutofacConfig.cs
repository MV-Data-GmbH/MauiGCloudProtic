using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using FcmSharp.Settings;
using GCloud.ExceptionHandlers;
using GCloud.Models.Domain;
using GCloud.Repository;
using GCloud.Repository.Impl;
using GCloud.Service;
using GCloud.Service.Impl;

namespace GCloud.App_Start
{
    public static class AutofacConfig
    {
        public static void Init()
        {
            var builder = new ContainerBuilder();
            var config = GlobalConfiguration.Configuration;

            builder.RegisterWebApiFilterProvider(config);

            builder.RegisterApiControllers((typeof(WebApiApplication)).Assembly);
            builder.RegisterControllers((typeof(WebApiApplication)).Assembly);

            builder.RegisterAssemblyTypes((typeof(WebApiApplication)).Assembly).Where(x => x.Name.EndsWith("Service")).AsImplementedInterfaces().InstancePerRequest();
            builder.RegisterAssemblyTypes((typeof(WebApiApplication)).Assembly).Where(x => x.Name.EndsWith("Repository")).AsImplementedInterfaces().InstancePerRequest();
            builder.RegisterType<GCloudContext>().As<DbContext>().InstancePerRequest();
            builder.RegisterInstance(LoadFirebasePrivateKey()).As<FcmClientSettings>().SingleInstance();
            builder.RegisterType<GlobalApiExceptionFilter>().AsWebApiExceptionFilterFor<ApiController>();
            builder.RegisterType<GlobalMvcExceptionFilter>().AsExceptionFilterOverrideFor<Controller>();

            var container = builder.Build();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }

        private static FcmClientSettings LoadFirebasePrivateKey()
        {
            var path = HttpContext.Current.Server.MapPath("~/firebasePrivateKey.json");

            return FileBasedFcmClientSettings.CreateFromFile(@"gcloudprotic", path);
        }
    }
}