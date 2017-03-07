
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Alexa.Demo.Web.Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            //TODO
            //document the the certificatin handler is in the SDK now
            config.MessageHandlers.Add(new Amazon.Alexa.SDK.Handlers.CertificateValidationHandler());
        }
    }
}
