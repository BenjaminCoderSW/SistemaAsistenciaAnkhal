using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GrupoAnkhalAsistencia.Helpers
{
    /// <summary>
    /// Valida Bearer token en todas las rutas /api/.
    /// El token se configura en Web.config > appSettings > ApiKey.
    /// </summary>
    public class ApiAuthHandler : DelegatingHandler
    {
        private static readonly string _apiKey =
            ConfigurationManager.AppSettings["ApiKey"] ?? "";

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var auth = request.Headers.Authorization;

            if (auth == null || auth.Scheme != "Bearer" || auth.Parameter != _apiKey)
            {
                var resp = request.CreateResponse(
                    HttpStatusCode.Unauthorized,
                    new { message = "Token de acceso inválido o ausente." });
                return Task.FromResult(resp);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
