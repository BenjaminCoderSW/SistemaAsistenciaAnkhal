using GrupoAnkhalAsistencia.Helpers;
using System;
using System.Configuration;
using System.Web;

namespace GrupoAnkhalAsistencia
{
    /// <summary>
    /// Handler invocado por cron-job.org cada madrugada a las 02:00 para actualizar
    /// automáticamente los días de vacaciones de los empleados según su antigüedad,
    /// sin depender de usuarios activos en el sistema.
    ///
    /// URL de uso:
    ///   https://grupoankhal.somee.com/AutoActualizarVacaciones.ashx?token=ankhal-internal-api-2026
    ///
    /// Respuestas posibles:
    ///   200 OK  — ejecución exitosa
    ///   401     — token incorrecto o ausente
    ///   500     — error interno al ejecutar el SP
    /// </summary>
    public class AutoActualizarVacaciones : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain; charset=utf-8";

            // ── 1. Validar token ──────────────────────────────────────────────
            string tokenEsperado = ConfigurationManager.AppSettings["ApiKey"] ?? string.Empty;
            string tokenRecibido = context.Request.QueryString["token"] ?? string.Empty;

            if (!tokenRecibido.Equals(tokenEsperado, StringComparison.Ordinal))
            {
                context.Response.StatusCode = 401;
                context.Response.Write("401 No autorizado: token incorrecto o ausente.");
                return;
            }

            // ── 2. Fecha de hoy en zona horaria Mexico (Central Standard Time) ─
            TimeZoneInfo zonaMexico = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            DateTime fechaHoy = TimeZoneInfo.ConvertTime(DateTime.UtcNow, zonaMexico).Date;

            // ── 3. Ejecutar SP a través del VacacionesHelper ──────────────────
            try
            {
                VacacionesHelper.ActualizarDiasVacacionesAutomatico();

                // ── 4. Respuesta exitosa ──────────────────────────────────────
                context.Response.StatusCode = 200;
                context.Response.Write(
                    $"OK | {fechaHoy:dd/MM/yyyy} | Vacaciones actualizadas correctamente.");
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.Write($"ERROR | {fechaHoy:dd/MM/yyyy} | {ex.Message}");
            }
        }

        public bool IsReusable => false;
    }
}
