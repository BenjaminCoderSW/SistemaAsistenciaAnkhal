using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace GrupoAnkhalAsistencia
{
    public partial class ImprimirActaHorasExtra : System.Web.UI.Page
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
                CargarActa(idAprobador, fi, ff, idPlanta);
            }
        }

        private class ActaRow
        {
            public string Empleado { get; set; }
            public string Planta { get; set; }
            public DateTime? Fecha { get; set; }
            public decimal? HorasExtras { get; set; }
            public string Tipo { get; set; }
            public string Motivo { get; set; }
        }

        private void CargarActa(int idAprobador, DateTime fi, DateTime ff, int idPlanta)
        {
            // Datos del aprobador
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

            // ── Automáticas aprobadas ─────────────────────────────────────────
            var automaticas = (from ap in db.tAprobacionHorasExtra
                               join a in db.tAsistencia on ap.IdAsistencia equals a.IdAsistencia
                               join u in db.tUsuario on a.IdUsuario equals u.IdUsuario
                               join pl in db.tPlanta on u.IdPlanta equals pl.IdPlanta into plGroup
                               from pl in plGroup.DefaultIfEmpty()
                               where ap.IdAprobador == idAprobador
                                  && ap.EstatusAprobacion == 2
                                  && a.Fecha >= fi.Date
                                  && a.Fecha <= ff.Date
                                  && (idPlanta == 0 ? true : u.IdPlanta == idPlanta)
                               orderby a.Fecha ascending
                               select new ActaRow
                               {
                                   Empleado = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,
                                   Planta = pl != null ? pl.Planta : "Sin planta",
                                   Fecha = a.Fecha,
                                   HorasExtras = a.HorasExtras,
                                   Tipo = a.EstatusHorasExtras ?? "Automático",
                                   Motivo = ap.Motivo ?? ""
                               }).ToList();

            // ── Manuales aprobadas ────────────────────────────────────────────
            var manuales = (from m in db.tHorasExtraManual
                            join u in db.tUsuario on m.IdUsuario equals u.IdUsuario
                            join pl in db.tPlanta on m.IdPlanta equals pl.IdPlanta into plGroup
                            from pl in plGroup.DefaultIfEmpty()
                            where m.IdAprobador == idAprobador
                               && m.EstatusAprobacion == 2
                               && m.Fecha >= fi.Date
                               && m.Fecha <= ff.Date
                               && (idPlanta == 0 ? true : m.IdPlanta == idPlanta)
                            orderby m.Fecha ascending
                            select new ActaRow
                            {
                                Empleado = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,
                                Planta = pl != null ? pl.Planta : "Sin planta",
                                Fecha = (DateTime?)m.Fecha,
                                HorasExtras = (decimal?)m.HorasExtras,
                                Tipo = "Manual",
                                Motivo = m.Descripcion ?? ""
                            }).ToList();

            // ── Unión ordenada por fecha ──────────────────────────────────────
            var todos = automaticas.Concat(manuales)
                .OrderBy(x => x.Fecha)
                .ToList();

            if (!todos.Any())
            {
                litTablaDecisiones.Text = "<div class='sin-datos'>No hay horas extra aprobadas para el per&iacute;odo seleccionado.</div>";
                litTotales.Text = "";
                return;
            }

            // Tabla
            var sb = new StringBuilder();
            sb.Append(@"<table>
  <thead>
    <tr>
      <th>#</th>
      <th>Empleado</th>
      <th>Planta</th>
      <th>Fecha</th>
      <th>Horas Extra</th>
      <th>Tipo</th>
      <th>Motivo / Descripci&oacute;n</th>
    </tr>
  </thead>
  <tbody>");

            int num = 1;
            decimal totalHorasAprobadas = 0;
            int totalRegistros = 0;

            foreach (var d in todos)
            {
                decimal redondeadas = RedondearA30Min(d.HorasExtras);
                if (redondeadas <= 0) continue;

                totalHorasAprobadas += redondeadas;
                totalRegistros++;

                sb.AppendFormat(@"
    <tr class='row-aprobado'>
      <td style='text-align:center;'>{0}</td>
      <td>{1}</td>
      <td>{2}</td>
      <td style='text-align:center;'>{3}</td>
      <td style='text-align:center;'>{4}</td>
      <td style='text-align:center;'>{5}</td>
      <td>{6}</td>
    </tr>",
                    num++,
                    d.Empleado,
                    d.Planta,
                    d.Fecha.HasValue ? d.Fecha.Value.ToString("dd/MM/yyyy") : "",
                    FormatearHoras(redondeadas),
                    d.Tipo,
                    d.Motivo);
            }

            sb.Append("\n  </tbody>\n</table>");
            litTablaDecisiones.Text = sb.ToString();

            litTotales.Text = string.Format(
                "<div class='totales-section'>" +
                "<span>Total de registros aprobados: {0}</span>" +
                "<span>Total de horas aprobadas: {1}</span>" +
                "</div>",
                totalRegistros,
                FormatearHoras(totalHorasAprobadas));
        }

        private decimal RedondearA30Min(decimal? horas)
        {
            if (horas == null || horas <= 0) return 0;
            return (decimal)(Math.Floor((double)horas * 2) / 2);
        }

        private string FormatearHoras(decimal horas)
        {
            if (horas <= 0) return "00:00";
            var ts = TimeSpan.FromHours((double)horas);
            return string.Format("{0:D2}:{1:D2}", (int)ts.TotalHours, ts.Minutes);
        }
    }
}
