using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class ReporteHorasExtraRH : System.Web.UI.Page
    {
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
            ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        private class ReporteHERow
        {
            public string Empleado        { get; set; }
            public string Planta          { get; set; }
            public string Fecha           { get; set; }
            public decimal? HorasExtras   { get; set; }
            public string HorasExtraFormato { get; set; }
            public string TipoHorasExtra  { get; set; }
            public string Descripcion     { get; set; }
            public string Motivo          { get; set; }
            public int EstatusAprobacion  { get; set; }
            public string EstatusTexto    { get; set; }
            public string Origen          { get; set; }
            public string Aprobador       { get; set; }
            public string FechaAprobacion { get; set; }
        }

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

        private List<ReporteHERow> ObtenerDatos(DateTime fi, DateTime ff, int plantaFiltro, string empleadoFiltro, int estatusFiltro)
        {
            // ── Automáticas ──────────────────────────────────────────────────────
            var automaticosRaw = (from a in db.tAsistencia
                                  join u in db.tUsuario on a.IdUsuario equals u.IdUsuario
                                  join p in db.tPlanta on a.IdPlanta equals p.IdPlanta
                                  join ap in db.tAprobacionHorasExtra
                                      on a.IdAsistencia equals ap.IdAsistencia into apGroup
                                  from ap in apGroup.DefaultIfEmpty()
                                  where a.HorasExtras > 0
                                     && a.Fecha >= fi.Date
                                     && a.Fecha <= ff.Date
                                     && (plantaFiltro == 0 || a.IdPlanta == plantaFiltro)
                                  select new
                                  {
                                      Empleado = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,
                                      Planta = p.Planta,
                                      Fecha = a.Fecha,
                                      a.HorasExtras,
                                      TipoHorasExtra = a.EstatusHorasExtras ?? "",
                                      Descripcion = "",
                                      Motivo = ap != null ? ap.Motivo : "",
                                      EstatusAprobacion = ap != null ? ap.EstatusAprobacion : 1,
                                      IdAprobador = ap != null ? (int?)ap.IdAprobador : null,
                                      FechaAprobacion = ap != null ? ap.FechaAprobacion : (DateTime?)null
                                  }).ToList();

            var aprobadorIdsAuto = automaticosRaw
                .Where(x => x.IdAprobador.HasValue).Select(x => x.IdAprobador.Value).Distinct().ToList();
            var aprobadoresAuto = db.tUsuario
                .Where(u => aprobadorIdsAuto.Contains(u.IdUsuario))
                .ToDictionary(u => u.IdUsuario, u => u.Nombre + " " + u.ApellidoPaterno);

            var automaticos = automaticosRaw
                .Where(x => RedondearA30Min(x.HorasExtras) > 0)
                .Select(x => new ReporteHERow
                {
                    Empleado = x.Empleado,
                    Planta = x.Planta,
                    Fecha = x.Fecha.HasValue ? x.Fecha.Value.ToString("dd/MM/yyyy") : "",
                    HorasExtras = x.HorasExtras,
                    HorasExtraFormato = FormatearHoras(x.HorasExtras),
                    TipoHorasExtra = x.TipoHorasExtra,
                    Descripcion = "",
                    Motivo = x.Motivo ?? "",
                    EstatusAprobacion = x.EstatusAprobacion,
                    EstatusTexto = x.EstatusAprobacion == 2 ? "Aprobado"
                                 : x.EstatusAprobacion == 3 ? "Rechazado" : "Pendiente",
                    Origen = "Automático",
                    Aprobador = x.IdAprobador.HasValue && aprobadoresAuto.ContainsKey(x.IdAprobador.Value)
                        ? aprobadoresAuto[x.IdAprobador.Value] : "-",
                    FechaAprobacion = x.FechaAprobacion.HasValue
                        ? x.FechaAprobacion.Value.ToString("dd/MM/yyyy HH:mm") : ""
                }).ToList();

            // ── Manuales ─────────────────────────────────────────────────────────
            var manualesRaw = (from m in db.tHorasExtraManual
                               join u in db.tUsuario on m.IdUsuario equals u.IdUsuario
                               join p in db.tPlanta on m.IdPlanta equals p.IdPlanta
                               where m.Fecha >= fi.Date
                                  && m.Fecha <= ff.Date
                                  && (plantaFiltro == 0 || m.IdPlanta == plantaFiltro)
                               select new
                               {
                                   Empleado = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,
                                   Planta = p.Planta,
                                   Fecha = m.Fecha,
                                   m.HorasExtras,
                                   m.Descripcion,
                                   m.MotivoAprobacion,
                                   m.EstatusAprobacion,
                                   m.IdAprobador,
                                   m.FechaAprobacion
                               }).ToList();

            var aprobadorIdsManuales = manualesRaw
                .Where(x => x.IdAprobador.HasValue).Select(x => x.IdAprobador.Value).Distinct().ToList();
            var aprobadoresManuales = db.tUsuario
                .Where(u => aprobadorIdsManuales.Contains(u.IdUsuario))
                .ToDictionary(u => u.IdUsuario, u => u.Nombre + " " + u.ApellidoPaterno);

            var manuales = manualesRaw
                .Where(x => x.HorasExtras > 0)
                .Select(x => new ReporteHERow
                {
                    Empleado = x.Empleado,
                    Planta = x.Planta,
                    Fecha = x.Fecha.ToString("dd/MM/yyyy"),
                    HorasExtras = x.HorasExtras,
                    HorasExtraFormato = FormatearHoras(x.HorasExtras),
                    TipoHorasExtra = "Manual",
                    Descripcion = x.Descripcion ?? "",
                    Motivo = x.MotivoAprobacion ?? "",
                    EstatusAprobacion = x.EstatusAprobacion,
                    EstatusTexto = x.EstatusAprobacion == 2 ? "Aprobado"
                                 : x.EstatusAprobacion == 3 ? "Rechazado" : "Pendiente",
                    Origen = "Manual",
                    Aprobador = x.IdAprobador.HasValue && aprobadoresManuales.ContainsKey(x.IdAprobador.Value)
                        ? aprobadoresManuales[x.IdAprobador.Value] : "-",
                    FechaAprobacion = x.FechaAprobacion.HasValue
                        ? x.FechaAprobacion.Value.ToString("dd/MM/yyyy HH:mm") : ""
                }).ToList();

            // ── Unión y filtros ───────────────────────────────────────────────────
            var todos = automaticos.Concat(manuales).ToList();

            if (!string.IsNullOrEmpty(empleadoFiltro))
                todos = todos.Where(x => x.Empleado.ToLower().Contains(empleadoFiltro.ToLower())).ToList();

            if (estatusFiltro > 0)
                todos = todos.Where(x => x.EstatusAprobacion == estatusFiltro).ToList();

            return todos.OrderByDescending(x => x.Fecha).ThenBy(x => x.Empleado).ToList();
        }

        private void CargarReporte()
        {
            if (!DateTime.TryParse(txtFechaInicio.Text, out DateTime fi) ||
                !DateTime.TryParse(txtFechaFin.Text, out DateTime ff))
                return;

            var datos = ObtenerDatos(fi, ff,
                Convert.ToInt32(ddlPlanta.SelectedValue),
                txtEmpleado.Text.Trim(),
                Convert.ToInt32(ddlEstatus.SelectedValue));

            gvReporteRH.DataSource = datos;
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

            var datos = ObtenerDatos(fi, ff,
                Convert.ToInt32(ddlPlanta.SelectedValue),
                txtEmpleado.Text.Trim(),
                Convert.ToInt32(ddlEstatus.SelectedValue));

            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition",
                string.Format("attachment;filename=HorasExtra_{0}.xls", DateTime.Now.ToString("yyyyMMdd")));
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-excel";

            GridView gvExport = new GridView();
            gvExport.AutoGenerateColumns = false;
            gvExport.EnableViewState = false;
            gvExport.GridLines = GridLines.Both;
            gvExport.HeaderStyle.Font.Bold = true;

            string[] campos = { "Empleado", "Planta", "Fecha", "HorasExtraFormato",
                "TipoHorasExtra", "Descripcion", "Motivo", "EstatusTexto", "Origen",
                "Aprobador", "FechaAprobacion" };
            string[] encabezados = { "Empleado", "Planta", "Fecha", "Horas Extra",
                "Tipo", "Descripción", "Motivo Aprobación", "Estatus", "Origen",
                "Jefe Aprobador", "Fecha Aprobación" };

            for (int i = 0; i < campos.Length; i++)
                gvExport.Columns.Add(new BoundField { DataField = campos[i], HeaderText = encabezados[i] });

            gvExport.DataSource = datos;
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

            string periodo = fi.ToString("dd/MM/yyyy") + " - " + ff.ToString("dd/MM/yyyy");

            var datos = ObtenerDatos(fi, ff,
                Convert.ToInt32(ddlPlanta.SelectedValue),
                txtEmpleado.Text.Trim(),
                Convert.ToInt32(ddlEstatus.SelectedValue));

            var resumen = datos
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
                string.Format("attachment;filename=HorasExtra_Resumen_{0}.xls", DateTime.Now.ToString("yyyyMMdd")));
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
