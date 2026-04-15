using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;

namespace GrupoAnkhalAsistencia
{
    /// <summary>
    /// Handler invocado por cron-job.org cada noche a las 23:48 para registrar
    /// automáticamente las faltas del día sin depender de usuarios activos en el sistema.
    ///
    /// URL de uso:
    ///   https://grupoankhal.somee.com/AutoRegistrarFaltas.ashx?token=ankhal-internal-api-2026
    ///
    /// Respuestas posibles:
    ///   200 OK  — ejecución exitosa (con conteo de faltas insertadas)
    ///   401     — token incorrecto o ausente
    ///   500     — error interno al ejecutar el SP
    /// </summary>
    public class AutoRegistrarFaltas : IHttpHandler
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
            DateTime ahora      = TimeZoneInfo.ConvertTime(DateTime.UtcNow, zonaMexico);
            DateTime fechaHoy   = ahora.Date;

            // ── 3. Ejecutar SP ────────────────────────────────────────────────
            try
            {
                string connStr = ConfigurationManager
                    .ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString;

                int faltasInsertadas = 0;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_RegistrarFaltasDelDia", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Fecha", fechaHoy);
                        cmd.CommandTimeout = 60;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                                faltasInsertadas = reader.GetInt32(0);
                        }
                    }
                }

                // ── 4. Respuesta exitosa ──────────────────────────────────────
                context.Response.StatusCode = 200;
                context.Response.Write(
                    $"OK | {fechaHoy:dd/MM/yyyy} | Faltas registradas: {faltasInsertadas}");
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.Write($"ERROR | {fechaHoy:dd/MM/yyyy} | {ex.Message}");
            }
        }

        // El handler no se puede reutilizar entre requests (estado interno por request)
        public bool IsReusable => false;
    }
}
