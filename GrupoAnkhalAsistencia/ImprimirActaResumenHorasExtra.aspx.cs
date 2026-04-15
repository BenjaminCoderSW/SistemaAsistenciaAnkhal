using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace GrupoAnkhalAsistencia
{
    public partial class ImprimirActaResumenHorasExtra : System.Web.UI.Page
    {
        private dbAsistenciaDataContext db = new dbAsistenciaDataContext(
            ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (SesionState.usuario == null) { Response.Redirect("login.aspx"); return; }

            if (!IsPostBack)
            {
                if (!int.TryParse(Request.QueryString["idAprobador"], out int idAprobador) ||
                    !DateTime.TryParse(Request.QueryString["fi"], out DateTime fi) ||
                    !DateTime.TryParse(Request.QueryString["ff"], out DateTime ff))
                {
                    Response.Write("<script>alert('Par\\u00e1metros inv\\u00e1lidos.'); window.close();</script>");
                    return;
                }

                int.TryParse(Request.QueryString["idPlanta"], out int idPlanta);
                CargarResumen(idAprobador, fi, ff, idPlanta);
            }
        }

        private void CargarResumen(int idAprobador, DateTime fi, DateTime ff, int idPlanta)
        {
            var jefe = db.tUsuario.FirstOrDefault(u => u.IdUsuario == idAprobador);
            if (jefe == null)
            {
                Response.Write("<script>alert('Aprobador no encontrado.'); window.close();</script>");
                return;
            }

            string plantaNombre;
            if (idPlanta == 0)
                plantaNombre = "Todas";
            else
            {
                var planta = db.tPlanta.FirstOrDefault(p => p.IdPlanta == idPlanta);
                plantaNombre = planta?.Planta ?? "N/A";
            }

            string jefeNombre = jefe.Nombre + " " + jefe.ApellidoPaterno + " " + jefe.ApellidoMaterno;

            litJefePlanta.Text = jefeNombre;
            litPlanta.Text = plantaNombre;
            litPeriodo.Text = fi.ToString("dd/MM/yyyy") + " &mdash; " + ff.ToString("dd/MM/yyyy");
            litFechaEmision.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            litFirmaNombre.Text = jefeNombre;

            // Registros aprobados agrupados por empleado con horas sumadas
            var registros = (from ap in db.tAprobacionHorasExtra
                             join a in db.tAsistencia on ap.IdAsistencia equals a.IdAsistencia
                             join u in db.tUsuario on a.IdUsuario equals u.IdUsuario
                             join pl in db.tPlanta on u.IdPlanta equals pl.IdPlanta into plGroup
                             from pl in plGroup.DefaultIfEmpty()
                             where ap.IdAprobador == idAprobador
                                && ap.EstatusAprobacion == 2
                                && a.Fecha >= fi.Date
                                && a.Fecha <= ff.Date
                                && (idPlanta == 0 ? true : u.IdPlanta == idPlanta)
                             select new
                             {
                                 Empleado = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,
                                 Planta = pl != null ? pl.Planta : "Sin planta",
                                 HorasExtras = a.HorasExtras ?? 0
                             }).ToList();

            if (!registros.Any())
            {
                litTablaResumen.Text = "<div class='sin-datos'>No hay horas extra aprobadas para el per&iacute;odo seleccionado.</div>";
                litTotales.Text = "";
                return;
            }

            // Agrupar en memoria por empleado+planta y sumar horas
            var resumen = registros
                .GroupBy(r => new { r.Empleado, r.Planta })
                .Select(g => new
                {
                    g.Key.Empleado,
                    g.Key.Planta,
                    TotalHoras = g.Sum(x => x.HorasExtras)
                })
                .OrderBy(r => r.Empleado)
                .ToList();

            var sb = new StringBuilder();
            sb.Append(@"<table>
  <thead>
    <tr>
      <th>#</th>
      <th>Empleado</th>
      <th>Planta</th>
      <th>Total Horas Extra</th>
    </tr>
  </thead>
  <tbody>");

            int num = 1;
            decimal totalGeneral = 0;

            foreach (var r in resumen)
            {
                totalGeneral += r.TotalHoras;

                sb.AppendFormat(@"
    <tr class='row-empleado'>
      <td style='text-align:center;'>{0}</td>
      <td>{1}</td>
      <td>{2}</td>
      <td style='text-align:center;font-weight:bold;'>{3}</td>
    </tr>",
                    num++,
                    r.Empleado,
                    r.Planta,
                    FormatearHoras(r.TotalHoras));
            }

            sb.Append("\n  </tbody>\n</table>");
            litTablaResumen.Text = sb.ToString();

            litTotales.Text = string.Format(
                "<div class='totales-section'>" +
                "<span>Total de empleados: {0}</span>" +
                "<span>Total de horas aprobadas: {1}</span>" +
                "</div>",
                resumen.Count,
                FormatearHoras(totalGeneral));
        }

        private string FormatearHoras(decimal horas)
        {
            if (horas <= 0) return "00:00";
            var ts = TimeSpan.FromHours((double)horas);
            return string.Format("{0:D2}:{1:D2}", (int)ts.TotalHours, ts.Minutes);
        }
    }
}
