using System;
using System.Web.Http;
using System.Web.Http.Cors;

namespace WSFB
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Configuración y servicios de API web
            // Uso de cors para permitir el uso de AJAX con las peticiones a la api y CROSS ORIGIN
            var cors = new EnableCorsAttribute("*", "*", "*");
            config.EnableCors(cors);
            //Formateo de datos para la comunicacion con el API
            config.Formatters.JsonFormatter.MediaTypeMappings.Add(new System.Net.Http.Formatting.RequestHeaderMapping("Accept",
                              "text/html",
                              StringComparison.InvariantCultureIgnoreCase,
                              true,
                              "application/json"));
            // Configuración y servicios de API web

            // Rutas de API web
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
