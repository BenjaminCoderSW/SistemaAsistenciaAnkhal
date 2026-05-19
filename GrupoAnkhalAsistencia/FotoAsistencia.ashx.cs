using MedicaMedens.Sesion;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.SessionState;

namespace GrupoAnkhalAsistencia
{
    public class FotoAsistencia : IHttpHandler, IReadOnlySessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            var usuario = SesionState.usuario;
            if (usuario == null)
            {
                context.Response.StatusCode = 401;
                return;
            }

            string rol = usuario.tRol?.Rol ?? "";
            if (rol != "Administrador" && rol != "Rh")
            {
                context.Response.StatusCode = 403;
                return;
            }

            if (!int.TryParse(context.Request.QueryString["id"], out int idAsistencia) || idAsistencia <= 0)
            {
                context.Response.StatusCode = 400;
                return;
            }

            string tipo = (context.Request.QueryString["tipo"] ?? "").ToLower().Trim();
            if (tipo != "entrada" && tipo != "salida")
            {
                context.Response.StatusCode = 400;
                return;
            }

            string colName = tipo == "entrada" ? "FotoEntrada" : "FotoSalida";
            string connStr = ConfigurationManager
                .ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString;

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                string sql = $"SELECT {colName} FROM tAsistencia WHERE IdAsistencia = @id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idAsistencia);
                    object result = cmd.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                    {
                        context.Response.StatusCode = 404;
                        return;
                    }

                    byte[] fotoBytes = (byte[])result;
                    context.Response.ContentType = "image/jpeg";
                    context.Response.Cache.SetCacheability(HttpCacheability.Private);
                    context.Response.Cache.SetMaxAge(TimeSpan.FromMinutes(5));
                    context.Response.BinaryWrite(fotoBytes);
                }
            }
        }

        public bool IsReusable => false;
    }
}
