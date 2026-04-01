using GrupoAnkhalAsistencia.Helpers;
using System.Web.Http;

namespace GrupoAnkhalAsistencia
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Token de autenticación en todas las rutas /api/  
            config.MessageHandlers.Add(new ApiAuthHandler());

            // Rutas con atributos ([Route], [RoutePrefix])
            config.MapHttpAttributeRoutes();

            // Ruta por defecto (fallback)
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Solo JSON — eliminar XML formatter
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling =
                Newtonsoft.Json.NullValueHandling.Ignore;
        }
    }
}
