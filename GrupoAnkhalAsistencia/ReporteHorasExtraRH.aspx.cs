using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Configuration;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class ReporteHorasExtraRH : System.Web.UI.Page
    {
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
            ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (SesionState.usuario == null) { Response.Redirect("login.aspx"); return; }

            string rol = SesionState.usuario.tRol.Rol;
            if (rol != "Administrador" && rol != "Rh") { Response.Redirect("login.aspx"); return; }

            if (!IsPostBack)
            {
                txtFechaInicio.Text = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
                txtFechaFin.Text = DateTime.Now.ToString("yyyy-MM-dd");
                CargarReporte();
            }
        }

        private void CargarReporte()
        {
            if (!DateTime.TryParse(txtFechaInicio.Text, out DateTime fi) ||
                !DateTime.TryParse(txtFechaFin.Text, out DateTime ff))
                return;

            string empleadoFiltro = txtEmpleado.Text.Trim();
            int estatusFiltro = Convert.ToInt32(ddlEstatus.SelectedValue);

            var step1 = (from a in db.tAsistencia
                         join u in db.tUsuario on a.IdUsuario equals u.IdUsuario
                         join p in db.tPlanta on a.IdPlanta equals p.IdPlanta
                         join ap in db.tAprobacionHorasExtra
                             on a.IdAsistencia equals ap.IdAsistencia
                             into apGroup
                         from ap in apGroup.DefaultIfEmpty()
                         where a.HorasExtras > 0
                            && a.Fecha >= fi.Date
                            && a.Fecha <= ff.Date
                         select new
                         {
                             Empleado = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,
                             Planta = p.Planta,
                             a.Fecha,
                             a.HorasExtras,
                             TipoHorasExtra = a.EstatusHorasExtras,
                             Motivo = ap != null ? ap.Motivo : "",
                             EstatusAprobacion = ap != null ? ap.EstatusAprobacion : 1,
                             EstatusTexto = ap == null || ap.EstatusAprobacion == 1 ? "Pendiente"
                                          : ap.EstatusAprobacion == 2 ? "Aprobado"
                                          : "Rechazado",
                             IdAprobador = ap != null ? (int?)ap.IdAprobador : null,
                             FechaAprobacion = ap != null ? ap.FechaAprobacion : (DateTime?)null
                         }).ToList();

            var aprobadorIds = step1
                .Where(x => x.IdAprobador.HasValue)
                .Select(x => x.IdAprobador.Value)
                .Distinct()
                .ToList();

            var aprobadores = db.tUsuario
                .Where(u => aprobadorIds.Contains(u.IdUsuario))
                .ToDictionary(u => u.IdUsuario, u => u.Nombre + " " + u.ApellidoPaterno);

            var result = step1.Select(x => new
            {
                x.Empleado,
                x.Planta,
                x.Fecha,
                x.HorasExtras,
                HorasExtraFormato = FormatearHoras(x.HorasExtras),
                x.TipoHorasExtra,
                x.Motivo,
                x.EstatusAprobacion,
                x.EstatusTexto,
                Aprobador = x.IdAprobador.HasValue && aprobadores.ContainsKey(x.IdAprobador.Value)
                    ? aprobadores[x.IdAprobador.Value]
                    : "-",
                x.FechaAprobacion
            });

            if (!string.IsNullOrEmpty(empleadoFiltro))
                result = result.Where(x => x.Empleado.ToLower().Contains(empleadoFiltro.ToLower()));

            if (estatusFiltro > 0)
                result = result.Where(x => x.EstatusAprobacion == estatusFiltro);

            gvReporteRH.DataSource = result.OrderByDescending(x => x.Fecha).ToList();
            gvReporteRH.DataBind();
        }

        private string FormatearHoras(decimal? horas)
        {
            if (horas == null || horas <= 0) return "00:00";
            var ts = TimeSpan.FromHours((double)horas);
            return string.Format("{0:D2}:{1:D2}", (int)ts.TotalHours, ts.Minutes);
        }

        protected void btnFiltrar_Click(object sender, EventArgs e) => CargarReporte();

        protected void btnLimpiarFiltros_Click(object sender, EventArgs e)
        {
            txtFechaInicio.Text = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
            txtFechaFin.Text = DateTime.Now.ToString("yyyy-MM-dd");
            txtEmpleado.Text = "";
            ddlEstatus.SelectedIndex = 0;
            CargarReporte();
        }

        protected void gvReporteRH_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvReporteRH.PageIndex = e.NewPageIndex;
            CargarReporte();
        }
    }
}
