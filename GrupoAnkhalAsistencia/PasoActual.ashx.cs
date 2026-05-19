using GrupoAnkhalAsistencia.Modelo;
using System;
using System.Configuration;
using System.Linq;
using System.Web;

namespace GrupoAnkhalAsistencia
{
    // Devuelve el paso actual de asistencia para un empleado dado su NumeroEmpleado.
    // Usado por Checar.aspx para decidir si mostrar la cámara selfie.
    public class PasoActual : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);

            string num = (context.Request.QueryString["num"] ?? "").Trim();
            if (string.IsNullOrEmpty(num))
            {
                context.Response.Write("error");
                return;
            }

            try
            {
                var db = new dbAsistenciaDataContext(
                    ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

                var usuario = db.tUsuario.FirstOrDefault(u => u.NumeroEmpleado == num && u.Estatus == 1);
                if (usuario == null)
                {
                    context.Response.Write("error");
                    return;
                }

                DateTime hoy = DateTime.Now.Date;
                var registro = db.tAsistencia.FirstOrDefault(x => x.IdUsuario == usuario.IdUsuario && x.Fecha == hoy);

                string paso;
                if (registro == null)
                    paso = "entrada";
                else if (registro.HoraSalidaComer == null)
                    paso = "salidaComer";
                else if (registro.HoraEntradaComer == null)
                    paso = "entradaComer";
                else if (registro.HoraSalida == null)
                    paso = "salida";
                else
                    paso = "completo";

                context.Response.Write(paso);
            }
            catch
            {
                context.Response.Write("error");
            }
        }

        public bool IsReusable => false;
    }
}
