using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
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
                CargarPlantasFiltro();
                CargarReporte();
            }
        }

        private void CargarPlantasFiltro()
        {
            var plantas = db.tPlanta
                .OrderBy(p => p.Planta)
                .Select(p => new { p.IdPlanta, p.Planta })
                .ToList();

            ddlPlanta.Items.Clear();
            ddlPlanta.Items.Add(new System.Web.UI.WebControls.ListItem("-- Todas --", "0"));
            foreach (var pl in plantas)
                ddlPlanta.Items.Add(new System.Web.UI.WebControls.ListItem(pl.Planta, pl.IdPlanta.ToString()));
        }

        private void CargarReporte()
        {
            if (!DateTime.TryParse(txtFechaInicio.Text, out DateTime fi) ||
                !DateTime.TryParse(txtFechaFin.Text, out DateTime ff))
                return;

            string empleadoFiltro = txtEmpleado.Text.Trim();
            int estatusFiltro = Convert.ToInt32(ddlEstatus.SelectedValue);
            int plantaFiltro = Convert.ToInt32(ddlPlanta.SelectedValue);

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
                            && (plantaFiltro == 0 || a.IdPlanta == plantaFiltro)
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

            var result = step1
                .Where(x => RedondearA30Min(x.HorasExtras) > 0)
                .Select(x => new
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

        private decimal RedondearA30Min(decimal? horas)
        {
            if (horas == null || horas <= 0) return 0;
            return (decimal)(Math.Floor((double)horas * 2) / 2);
        }

        private string FormatearHoras(decimal? horas)
        {
            decimal redondeadas = RedondearA30Min(horas);
            if (redondeadas <= 0) return "00:00";
            var ts = TimeSpan.FromHours((double)redondeadas);
            return string.Format("{0:D2}:{1:D2}", (int)ts.TotalHours, ts.Minutes);
        }

        protected void btnFiltrar_Click(object sender, EventArgs e) => CargarReporte();

        protected void btnLimpiarFiltros_Click(object sender, EventArgs e)
        {
            txtFechaInicio.Text = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
            txtFechaFin.Text = DateTime.Now.ToString("yyyy-MM-dd");
            txtEmpleado.Text = "";
            ddlEstatus.SelectedIndex = 0;
            ddlPlanta.SelectedIndex = 0;
            CargarReporte();
        }

        protected void gvReporteRH_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvReporteRH.PageIndex = e.NewPageIndex;
            CargarReporte();
        }

        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            if (!DateTime.TryParse(txtFechaInicio.Text, out DateTime fi) ||
                !DateTime.TryParse(txtFechaFin.Text, out DateTime ff))
                return;

            string empleadoFiltro = txtEmpleado.Text.Trim();
            int estatusFiltro = Convert.ToInt32(ddlEstatus.SelectedValue);
            int plantaFiltro = Convert.ToInt32(ddlPlanta.SelectedValue);

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
                            && (plantaFiltro == 0 || a.IdPlanta == plantaFiltro)
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

            var aprobadorIds = step1.Where(x => x.IdAprobador.HasValue)
                .Select(x => x.IdAprobador.Value).Distinct().ToList();
            var aprobadores = db.tUsuario
                .Where(u => aprobadorIds.Contains(u.IdUsuario))
                .ToDictionary(u => u.IdUsuario, u => u.Nombre + " " + u.ApellidoPaterno);

            var datos = step1
                .Where(x => RedondearA30Min(x.HorasExtras) > 0)
                .Select(x => new
                {
                    x.Empleado,
                    x.Planta,
                    Fecha = x.Fecha.HasValue ? x.Fecha.Value.ToString("dd/MM/yyyy") : "",
                    HorasExtras = x.HorasExtras.HasValue ? x.HorasExtras.Value.ToString("N2") : "",
                    HorasExtraFormato = FormatearHoras(x.HorasExtras),
                    x.TipoHorasExtra,
                    x.Motivo,
                    x.EstatusTexto,
                    Aprobador = x.IdAprobador.HasValue && aprobadores.ContainsKey(x.IdAprobador.Value)
                        ? aprobadores[x.IdAprobador.Value] : "-",
                    FechaAprobacion = x.FechaAprobacion.HasValue
                        ? x.FechaAprobacion.Value.ToString("dd/MM/yyyy HH:mm") : ""
                });

            if (!string.IsNullOrEmpty(empleadoFiltro))
                datos = datos.Where(x => x.Empleado.ToLower().Contains(empleadoFiltro.ToLower()));
            if (estatusFiltro > 0)
                datos = datos.Where(x =>
                    (estatusFiltro == 1 && x.EstatusTexto == "Pendiente") ||
                    (estatusFiltro == 2 && x.EstatusTexto == "Aprobado") ||
                    (estatusFiltro == 3 && x.EstatusTexto == "Rechazado"));

            var lista = datos.OrderByDescending(x => x.Fecha).ToList();

            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition",
                string.Format("attachment;filename=HorasExtra_{0}.xls",
                    DateTime.Now.ToString("yyyyMMdd")));
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-excel";

            GridView gvExport = new GridView();
            gvExport.AutoGenerateColumns = false;
            gvExport.EnableViewState = false;
            gvExport.GridLines = GridLines.Both;
            gvExport.HeaderStyle.Font.Bold = true;

            string[] campos = { "Empleado", "Planta", "Fecha",
                "HorasExtraFormato", "TipoHorasExtra", "Motivo", "EstatusTexto",
                "Aprobador", "FechaAprobacion" };
            string[] encabezados = { "Empleado", "Planta", "Fecha",
                "Horas Extra", "Tipo", "Motivo", "Estatus",
                "Jefe Aprobador", "Fecha Aprobacion" };

            for (int i = 0; i < campos.Length; i++)
                gvExport.Columns.Add(new BoundField { DataField = campos[i], HeaderText = encabezados[i] });

            gvExport.DataSource = lista;
            gvExport.DataBind();

            StringWriter sw = new StringWriter();
            HtmlTextWriter hw = new HtmlTextWriter(sw);
            gvExport.RenderControl(hw);
            Response.Write(sw.ToString());
            Response.End();
        }

        protected void btnExportExcelResumen_Click(object sender, EventArgs e)
        {
            if (!DateTime.TryParse(txtFechaInicio.Text, out DateTime fi) ||
                !DateTime.TryParse(txtFechaFin.Text, out DateTime ff))
                return;

            string empleadoFiltro = txtEmpleado.Text.Trim();
            int estatusFiltro = Convert.ToInt32(ddlEstatus.SelectedValue);
            int plantaFiltro = Convert.ToInt32(ddlPlanta.SelectedValue);
            string periodo = fi.ToString("dd/MM/yyyy") + " - " + ff.ToString("dd/MM/yyyy");

            var raw = (from a in db.tAsistencia
                       join u in db.tUsuario on a.IdUsuario equals u.IdUsuario
                       join p in db.tPlanta on a.IdPlanta equals p.IdPlanta
                       join ap in db.tAprobacionHorasExtra on a.IdAsistencia equals ap.IdAsistencia into apg
                       from ap in apg.DefaultIfEmpty()
                       where a.HorasExtras > 0
                          && a.Fecha >= fi.Date
                          && a.Fecha <= ff.Date
                          && (plantaFiltro == 0 || a.IdPlanta == plantaFiltro)
                       select new
                       {
                           Empleado = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,
                           Planta = p.Planta,
                           a.HorasExtras,
                           EstatusAprobacion = ap != null ? ap.EstatusAprobacion : 1,
                           EstatusTexto = ap == null || ap.EstatusAprobacion == 1 ? "Pendiente"
                                        : ap.EstatusAprobacion == 2 ? "Aprobado" : "Rechazado"
                       }).ToList()
                       .Where(x => RedondearA30Min(x.HorasExtras) > 0)
                       .ToList();

            if (!string.IsNullOrEmpty(empleadoFiltro))
                raw = raw.Where(x => x.Empleado.ToLower().Contains(empleadoFiltro.ToLower())).ToList();

            if (estatusFiltro > 0)
                raw = raw.Where(x => x.EstatusAprobacion == estatusFiltro).ToList();

            var resumen = raw
                .GroupBy(x => new { x.Empleado, x.Planta })
                .Select(g => new
                {
                    g.Key.Empleado,
                    g.Key.Planta,
                    Periodo = periodo,
                    TotalHorasExtra = FormatearHoras(g.Sum(x => RedondearA30Min(x.HorasExtras)))
                })
                .Where(g => g.TotalHorasExtra != "00:00")
                .OrderBy(g => g.Empleado)
                .ToList();

            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition",
                string.Format("attachment;filename=HorasExtra_Resumen_{0}.xls",
                    DateTime.Now.ToString("yyyyMMdd")));
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-excel";

            GridView gvExport = new GridView();
            gvExport.AutoGenerateColumns = false;
            gvExport.EnableViewState = false;
            gvExport.GridLines = GridLines.Both;
            gvExport.HeaderStyle.Font.Bold = true;

            string[] campos = { "Empleado", "Planta", "Periodo", "TotalHorasExtra" };
            string[] encabezados = { "Empleado", "Planta", "Periodo", "Total Horas Extra" };

            for (int i = 0; i < campos.Length; i++)
                gvExport.Columns.Add(new BoundField { DataField = campos[i], HeaderText = encabezados[i] });

            gvExport.DataSource = resumen;
            gvExport.DataBind();

            StringWriter sw = new StringWriter();
            HtmlTextWriter hw = new HtmlTextWriter(sw);
            gvExport.RenderControl(hw);
            Response.Write(sw.ToString());
            Response.End();
        }

        public override void VerifyRenderingInServerForm(Control control) { }
    }
}
