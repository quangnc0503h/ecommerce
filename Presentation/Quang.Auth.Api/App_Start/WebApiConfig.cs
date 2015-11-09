using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Quang.Auth.Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
           // config.EnableCors();
          OwinHttpConfigurationExtensions.SuppressDefaultHostAuthentication(config);
      config.Filters.Add((IFilter) new HostAuthenticationFilter("Bearer"));
      HttpConfigurationExtensions.MapHttpAttributeRoutes(config);
      HttpRouteCollectionExtensions.MapHttpRoute(config.Routes, "DefaultApi", "api/{controller}/{id}", (object) new
      {
        id = RouteParameter.Optional
      });
      config.DependencyResolver = (IDependencyResolver) new UnityDependencyResolver(UnityConfig.GetConfiguredContainer());

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
