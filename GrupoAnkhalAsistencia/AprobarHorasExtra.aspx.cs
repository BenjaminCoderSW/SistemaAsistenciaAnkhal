using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class AprobarHorasExtra : System.Web.UI.Page
    {
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
            ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        protected int IdAprobadorActual => SesionState.usuario?.IdUsuario ?? 0;

        private DateTime FechaInicioVS
        {
            get => ViewState["fi"] is DateTime d ? d : DateTime.Now.AddDays(-7);
            set => ViewState["fi"] = value;
        }
        private DateTime FechaFinVS
        {
            get => ViewState["ff"] is DateTime d ? d : DateTime.Now;
            set => ViewState["ff"] = value;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (SesionState.usuario == null) { Response.Redirect("login.aspx"); return; }

            string rol = SesionState.usuario.tRol.Rol;
            if (rol != "Jefe de Planta" && rol != "Administrador" && rol != "Rh")
            {
                Response.Redirect("login.aspx"); return;
            }

            btnImprimirActa.Visible = (rol == "Jefe de Planta");
            btnImprimirResumen.Visible = (rol == "Jefe de Planta");

            if (IsPostBack)
                hfMostrarModal.Value = "0";

            if (!IsPostBack)
            {
                txtFechaInicio.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
                txtFechaFin.Text = DateTime.Now.ToString("yyyy-MM-dd");
                CargarFiltroPlanta();
                CargarHorasExtra();
                CargarHorasExtraManual();
            }
        }

        private void CargarFiltroPlanta()
        {
            ddlFiltroPlanta.Items.Clear();
            ddlFiltroPlanta.Items.Add(new ListItem("Todas", "0"));
            var plantas = db.tPlanta.OrderBy(p => p.Planta).ToList();
            foreach (var p in plantas)
                ddlFiltroPlanta.Items.Add(new ListItem(p.Planta, p.IdPlanta.ToString()));

            int? idPlantaUsuario = SesionState.usuario.IdPlanta;
            if (idPlantaUsuario != null)
            {
                var item = ddlFiltroPlanta.Items.FindByValue(idPlantaUsuario.ToString());
                if (item != null) item.Selected = true;
            }
        }

        private void CargarHorasExtra()
        {
            if (!DateTime.TryParse(txtFechaInicio.Text, out DateTime fi) ||
                !DateTime.TryParse(txtFechaFin.Text, out DateTime ff))
            {
                MostrarAlerta("warning", "Alerta", "Seleccione un rango de fechas valido.");
                return;
            }

            FechaInicioVS = fi;
            FechaFinVS = ff;

            string buscarEmpleado = txtBuscarEmpleado.Text.Trim();
            int filtroPlantaId = int.TryParse(ddlFiltroPlanta.SelectedValue, out int pid) ? pid : 0;
            int filtroEstatus = int.TryParse(ddlFiltroEstatus.SelectedValue, out int est) ? est : 0;

            var rawRows = (from a in db.tAsistencia
                           join u in db.tUsuario on a.IdUsuario equals u.IdUsuario
                           join pl in db.tPlanta on u.IdPlanta equals pl.IdPlanta into plg
                           from pl in plg.DefaultIfEmpty()
                           join ap in db.tAprobacionHorasExtra on a.IdAsistencia equals ap.IdAsistencia into apg
                           from ap in apg.DefaultIfEmpty()
                           where a.HorasExtras > 0
                              && a.Fecha >= fi.Date
                              && a.Fecha <= ff.Date
                              && (filtroPlantaId == 0 || u.IdPlanta == filtroPlantaId)
                           select new
                           {
                               a.IdUsuario,
                               Empleado = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,
                               Planta = pl != null ? pl.Planta : "Sin planta",
                               a.HorasExtras,
                               TipoHorasExtra = a.EstatusHorasExtras,
                               EstatusAprobacion = ap != null ? ap.EstatusAprobacion : 1,
                               Motivo = ap != null ? ap.Motivo : ""
                           }).ToList();

            if (!string.IsNullOrEmpty(buscarEmpleado))
                rawRows = rawRows.Where(x => x.Empleado.ToLower().Contains(buscarEmpleado.ToLower())).ToList();

            var agrupado = rawRows
                .GroupBy(x => new { x.IdUsuario, x.Empleado, x.Planta })
                .Select(g =>
                {
                    decimal totalRedondeado = g.Sum(x => RedondearA30Min(x.HorasExtras));
                    var estatuses = g.Select(x => x.EstatusAprobacion).ToList();
                    string estatusTexto = DeterminarEstatusGrupo(estatuses);
                    int estatusDecision = DeterminarDecisionValor(estatuses);
                    string motivoActual = g.Where(x => !string.IsNullOrEmpty(x.Motivo))
                                          .Select(x => x.Motivo).FirstOrDefault() ?? "";
                    string tipo = g.Select(x => x.TipoHorasExtra).Distinct().Count() == 1
                                  ? g.First().TipoHorasExtra : "Mixto";
                    return new
                    {
                        g.Key.IdUsuario,
                        g.Key.Empleado,
                        g.Key.Planta,
                        TotalHorasFormato = FormatearHoras(totalRedondeado),
                        TipoHorasExtra = tipo,
                        EstatusTexto = estatusTexto,
                        EstatusDecision = estatusDecision,
                        MotivoActual = motivoActual,
                        TotalRedondeado = totalRedondeado
                    };
                })
                .Where(x => x.TotalRedondeado > 0)
                .ToList();

            if (filtroEstatus != 0)
            {
                if (filtroEstatus == 1)
                    agrupado = agrupado.Where(x => x.EstatusDecision == 0 || x.EstatusTexto == "Pendiente").ToList();
                else if (filtroEstatus == 2)
                    agrupado = agrupado.Where(x => x.EstatusTexto == "Aprobado").ToList();
                else if (filtroEstatus == 3)
                    agrupado = agrupado.Where(x => x.EstatusTexto == "Rechazado").ToList();
            }

            gvHorasExtra.DataSource = agrupado.OrderBy(x => x.Empleado).ToList();
            gvHorasExtra.DataBind();
        }

        protected void gvHorasExtra_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            var ddl = (DropDownList)e.Row.FindControl("ddlDecision");
            var txt = (TextBox)e.Row.FindControl("txtMotivo");

            int estatusDecision = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "EstatusDecision"));
            string motivo = DataBinder.Eval(e.Row.DataItem, "MotivoActual")?.ToString() ?? "";

            var item = ddl.Items.FindByValue(estatusDecision.ToString());
            if (item != null) item.Selected = true;
            txt.Text = motivo;

            if (estatusDecision == 2)
                e.Row.CssClass = "table-success";
            else if (estatusDecision == 3)
                e.Row.CssClass = "table-danger";
        }

        protected void gvDetalle_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            int estatus = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "EstatusNum"));
            if (estatus == 2)
                e.Row.CssClass = "table-success";
            else if (estatus == 3)
                e.Row.CssClass = "table-danger";
            else
                e.Row.CssClass = "table-warning";
        }

        protected void gvHorasExtra_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "VerDetalle") return;

            int idUsuario = Convert.ToInt32(e.CommandArgument);
            CargarDetalle(idUsuario);
            hfMostrarModal.Value = "1";
        }

        private void CargarDetalle(int idUsuario)
        {
            DateTime fi = FechaInicioVS;
            DateTime ff = FechaFinVS;

            var empleado = db.tUsuario.FirstOrDefault(u => u.IdUsuario == idUsuario);
            litNombreEmpleadoDetalle.Text = empleado != null
                ? empleado.Nombre + " " + empleado.ApellidoPaterno + " " + empleado.ApellidoMaterno
                : "";

            var registros = (from a in db.tAsistencia
                             join ap in db.tAprobacionHorasExtra on a.IdAsistencia equals ap.IdAsistencia into apg
                             from ap in apg.DefaultIfEmpty()
                             where a.IdUsuario == idUsuario
                                && a.HorasExtras > 0
                                && a.Fecha >= fi.Date
                                && a.Fecha <= ff.Date
                             orderby a.Fecha ascending
                             select new
                             {
                                 a.Fecha,
                                 a.HorasExtras,
                                 EstatusAprobacion = ap != null ? ap.EstatusAprobacion : 1
                             }).ToList();

            var detalle = registros
                .Select(r => new
                {
                    FechaFormato = r.Fecha.HasValue ? r.Fecha.Value.ToString("dd/MM/yyyy") : "",
                    HorasFormato = FormatearHoras(RedondearA30Min(r.HorasExtras)),
                    Descripcion = "",
                    HorasRedondeadas = RedondearA30Min(r.HorasExtras),
                    EstatusTexto = r.EstatusAprobacion == 2 ? "Aprobado"
                                 : r.EstatusAprobacion == 3 ? "Rechazado"
                                 : "Pendiente",
                    EstatusNum = r.EstatusAprobacion
                })
                .Where(r => r.HorasRedondeadas > 0)
                .ToList();

            gvDetalle.DataSource = detalle;
            gvDetalle.DataBind();

            decimal totalPeriodo = detalle.Sum(r => r.HorasRedondeadas);
            lblTotalDetalle.Text = string.Format(
                "Total del periodo ({0} al {1}): <span class='text-info'>{2}</span> horas extra",
                fi.ToString("dd/MM/yyyy"), ff.ToString("dd/MM/yyyy"), FormatearHoras(totalPeriodo));
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            gvHorasExtra.PageIndex = 0;
            gvHorasExtraManual.PageIndex = 0;
            CargarHorasExtra();
            CargarHorasExtraManual();
        }

        protected void btnLimpiarFiltros_Click(object sender, EventArgs e)
        {
            txtFechaInicio.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            txtFechaFin.Text = DateTime.Now.ToString("yyyy-MM-dd");
            txtBuscarEmpleado.Text = "";
            ddlFiltroPlanta.SelectedIndex = 0;
            ddlFiltroEstatus.SelectedIndex = 0;
            gvHorasExtra.PageIndex = 0;
            gvHorasExtraManual.PageIndex = 0;
            CargarHorasExtra();
            CargarHorasExtraManual();
        }

        protected void gvHorasExtra_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvHorasExtra.PageIndex = e.NewPageIndex;
            CargarHorasExtra();
            CargarHorasExtraManual();
        }

        protected void gvHorasExtraManual_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvHorasExtraManual.PageIndex = e.NewPageIndex;
            CargarHorasExtra();
            CargarHorasExtraManual();
        }

        private void CargarHorasExtraManual()
        {
            if (!DateTime.TryParse(txtFechaInicio.Text, out DateTime fi) ||
                !DateTime.TryParse(txtFechaFin.Text, out DateTime ff))
                return;

            int filtroPlantaId = int.TryParse(ddlFiltroPlanta.SelectedValue, out int pid) ? pid : 0;
            string buscarEmpleado = txtBuscarEmpleado.Text.Trim();
            int filtroEstatus = int.TryParse(ddlFiltroEstatus.SelectedValue, out int est) ? est : 0;

            var raw = (from m in db.tHorasExtraManual
                       join u in db.tUsuario on m.IdUsuario equals u.IdUsuario
                       join pl in db.tPlanta on m.IdPlanta equals pl.IdPlanta
                       where m.Fecha >= fi.Date && m.Fecha <= ff.Date
                          && (filtroPlantaId == 0 || m.IdPlanta == filtroPlantaId)
                       select new
                       {
                           m.IdUsuario,
                           Empleado = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,
                           Planta = pl.Planta,
                           m.HorasExtras,
                           m.EstatusAprobacion,
                           m.MotivoAprobacion
                       }).ToList();

            if (!string.IsNullOrEmpty(buscarEmpleado))
                raw = raw.Where(x => x.Empleado.ToLower().Contains(buscarEmpleado.ToLower())).ToList();

            var agrupado = raw
                .GroupBy(x => new { x.IdUsuario, x.Empleado, x.Planta })
                .Select(g =>
                {
                    decimal totalRedondeado = g.Sum(x => RedondearA30Min(x.HorasExtras));
                    var estatuses = g.Select(x => x.EstatusAprobacion).ToList();
                    string estatusTexto = DeterminarEstatusGrupo(estatuses);
                    int estatusDecision = DeterminarDecisionValor(estatuses);
                    string motivoActual = g.Where(x => !string.IsNullOrEmpty(x.MotivoAprobacion))
                                          .Select(x => x.MotivoAprobacion).FirstOrDefault() ?? "";
                    return new
                    {
                        g.Key.IdUsuario,
                        g.Key.Empleado,
                        g.Key.Planta,
                        TotalHorasFormato = FormatearHoras(totalRedondeado),
                        TipoHorasExtra = "Manual",
                        EstatusTexto = estatusTexto,
                        EstatusDecision = estatusDecision,
                        MotivoActual = motivoActual,
                        TotalRedondeado = totalRedondeado
                    };
                })
                .Where(x => x.TotalRedondeado > 0)
                .ToList();

            if (filtroEstatus != 0)
            {
                if (filtroEstatus == 1)
                    agrupado = agrupado.Where(x => x.EstatusDecision == 0 || x.EstatusTexto == "Pendiente").ToList();
                else if (filtroEstatus == 2)
                    agrupado = agrupado.Where(x => x.EstatusTexto == "Aprobado").ToList();
                else if (filtroEstatus == 3)
                    agrupado = agrupado.Where(x => x.EstatusTexto == "Rechazado").ToList();
            }

            int pendientes = agrupado.Count(x => x.EstatusDecision == 0 || x.EstatusTexto == "Pendiente");
            lblBadgeManuales.Text = pendientes > 0 ? pendientes.ToString() : "";
            lblBadgeManuales.Visible = pendientes > 0;

            gvHorasExtraManual.DataSource = agrupado.OrderBy(x => x.Empleado).ToList();
            gvHorasExtraManual.DataBind();
        }

        protected void gvHorasExtraManual_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            var ddl = (DropDownList)e.Row.FindControl("ddlDecisionManual");
            var txt = (TextBox)e.Row.FindControl("txtMotivoManual");

            int estatusDecision = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "EstatusDecision"));
            string motivo = DataBinder.Eval(e.Row.DataItem, "MotivoActual")?.ToString() ?? "";

            if (ddl != null)
            {
                var item = ddl.Items.FindByValue(estatusDecision.ToString());
                if (item != null) item.Selected = true;
            }
            if (txt != null) txt.Text = motivo;

            if (estatusDecision == 2)
                e.Row.CssClass = "table-success";
            else if (estatusDecision == 3)
                e.Row.CssClass = "table-danger";
        }

        protected void gvHorasExtraManual_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "VerDetalleManual") return;

            int idUsuario = Convert.ToInt32(e.CommandArgument);
            CargarDetalleManual(idUsuario);
            hfMostrarModal.Value = "1";
        }

        private void CargarDetalleManual(int idUsuario)
        {
            DateTime fi = FechaInicioVS;
            DateTime ff = FechaFinVS;

            var empleado = db.tUsuario.FirstOrDefault(u => u.IdUsuario == idUsuario);
            litNombreEmpleadoDetalle.Text = empleado != null
                ? empleado.Nombre + " " + empleado.ApellidoPaterno + " " + empleado.ApellidoMaterno
                : "";

            var raw = (from m in db.tHorasExtraManual
                       where m.IdUsuario == idUsuario
                          && m.Fecha >= fi.Date
                          && m.Fecha <= ff.Date
                       orderby m.Fecha ascending
                       select new
                       {
                           m.Fecha,
                           m.HorasExtras,
                           m.Descripcion,
                           m.EstatusAprobacion
                       }).ToList();

            var detalle = raw.Select(r => new
            {
                FechaFormato = r.Fecha.ToString("dd/MM/yyyy"),
                HorasFormato = FormatearHoras(RedondearA30Min(r.HorasExtras)),
                Descripcion = r.Descripcion ?? "",
                HorasRedondeadas = RedondearA30Min(r.HorasExtras),
                EstatusTexto = r.EstatusAprobacion == 2 ? "Aprobado"
                             : r.EstatusAprobacion == 3 ? "Rechazado"
                             : "Pendiente",
                EstatusNum = r.EstatusAprobacion
            })
            .Where(r => r.HorasRedondeadas > 0)
            .ToList();

            gvDetalle.DataSource = detalle;
            gvDetalle.DataBind();

            decimal totalPeriodo = detalle.Sum(r => r.HorasRedondeadas);
            lblTotalDetalle.Text = string.Format(
                "Total del periodo ({0} al {1}): <span class='text-info'>{2}</span> horas extra",
                fi.ToString("dd/MM/yyyy"), ff.ToString("dd/MM/yyyy"), FormatearHoras(totalPeriodo));
        }

        protected void btnGuardarManuales_Click(object sender, EventArgs e)
        {
            try
            {
                if (!DateTime.TryParse(txtFechaInicio.Text, out DateTime fi) ||
                    !DateTime.TryParse(txtFechaFin.Text, out DateTime ff))
                {
                    MostrarAlerta("warning", "Alerta", "Rango de fechas inválido.");
                    return;
                }

                bool huboDecision = false;
                DateTime ahora = DateTime.Now;
                int idAprobador = SesionState.usuario.IdUsuario;

                foreach (GridViewRow row in gvHorasExtraManual.Rows)
                {
                    if (row.RowType != DataControlRowType.DataRow) continue;

                    int idUsuario = Convert.ToInt32(gvHorasExtraManual.DataKeys[row.RowIndex].Value);
                    var ddl = (DropDownList)row.FindControl("ddlDecisionManual");
                    var txtMot = (TextBox)row.FindControl("txtMotivoManual");

                    int decision = Convert.ToInt32(ddl.SelectedValue);
                    if (decision == 0) continue;

                    huboDecision = true;
                    string motivo = txtMot.Text.Trim();

                    // Aplicar la decisión a todos los registros manuales del empleado en el período
                    var registros = db.tHorasExtraManual
                        .Where(m => m.IdUsuario == idUsuario
                                 && m.Fecha >= fi.Date
                                 && m.Fecha <= ff.Date)
                        .ToList();

                    foreach (var reg in registros)
                    {
                        reg.EstatusAprobacion = decision;
                        reg.MotivoAprobacion = motivo;
                        reg.IdAprobador = idAprobador;
                        reg.FechaAprobacion = ahora;
                    }
                }

                if (!huboDecision)
                {
                    MostrarAlerta("warning", "Sin cambios", "Seleccione al menos una decisión antes de guardar.");
                    return;
                }

                db.SubmitChanges();
                CargarHorasExtra();
                CargarHorasExtraManual();
                MostrarAlerta("success", "Guardado", "Las decisiones de horas extra manuales se guardaron correctamente.");
            }
            catch (Exception ex)
            {
                MostrarAlerta("error", "Error", ex.Message);
            }
        }

        protected void btnEnviarRH_Click(object sender, EventArgs e)
        {
            try
            {
                if (!DateTime.TryParse(txtFechaInicio.Text, out DateTime fi) ||
                    !DateTime.TryParse(txtFechaFin.Text, out DateTime ff))
                {
                    MostrarAlerta("warning", "Alerta", "Seleccione un rango de fechas valido.");
                    return;
                }

                int idAprobador = SesionState.usuario.IdUsuario;
                DateTime ahora = DateTime.Now;
                bool huboDecision = false;

                foreach (GridViewRow row in gvHorasExtra.Rows)
                {
                    if (row.RowType != DataControlRowType.DataRow) continue;

                    int idUsuario = Convert.ToInt32(gvHorasExtra.DataKeys[row.RowIndex].Value);
                    var ddl = (DropDownList)row.FindControl("ddlDecision");
                    var txtMot = (TextBox)row.FindControl("txtMotivo");

                    int decision = Convert.ToInt32(ddl.SelectedValue);
                    if (decision == 0) continue;

                    huboDecision = true;
                    string motivo = txtMot.Text.Trim();

                    var idsAsistencia = db.tAsistencia
                        .Where(a => a.IdUsuario == idUsuario
                                 && a.HorasExtras > 0
                                 && a.Fecha >= fi.Date
                                 && a.Fecha <= ff.Date)
                        .Select(a => a.IdAsistencia)
                        .ToList();

                    foreach (int idAsis in idsAsistencia)
                    {
                        var existente = db.tAprobacionHorasExtra.FirstOrDefault(x => x.IdAsistencia == idAsis);
                        if (existente != null)
                        {
                            existente.EstatusAprobacion = decision;
                            existente.Motivo = motivo;
                            existente.FechaAprobacion = ahora;
                            existente.IdAprobador = idAprobador;
                        }
                        else
                        {
                            db.tAprobacionHorasExtra.InsertOnSubmit(new tAprobacionHorasExtra
                            {
                                IdAsistencia = idAsis,
                                IdAprobador = idAprobador,
                                EstatusAprobacion = decision,
                                Motivo = motivo,
                                FechaAprobacion = ahora
                            });
                        }
                    }
                }

                if (!huboDecision)
                {
                    MostrarAlerta("warning", "Sin cambios", "Seleccione al menos una decision antes de enviar.");
                    return;
                }

                db.SubmitChanges();
                EnviarCorreoARH();
                CargarHorasExtra();

                MostrarAlerta("success", "Enviado", "Las decisiones se guardaron y el reporte fue enviado a RH.");
            }
            catch (Exception ex)
            {
                MostrarAlerta("error", "Error", ex.Message);
            }
        }

        private void EnviarCorreoARH()
        {
            try
            {
                var cfg = db.ConfigCorreo.FirstOrDefault();
                if (cfg == null) return;

                var usuarioRH = db.tUsuario.FirstOrDefault(u => u.IdRol == 3 && u.Estatus == 1);
                if (usuarioRH == null || string.IsNullOrEmpty(usuarioRH.Email)) return;

                int idAprobador = SesionState.usuario.IdUsuario;
                string aprobador = SesionState.usuario.Nombre + " " + SesionState.usuario.ApellidoPaterno;

                var planta = db.tPlanta.FirstOrDefault(p => p.IdPlanta == SesionState.usuario.IdPlanta);
                string plantaNombre = planta?.Planta ?? "N/A";

                var rawDecisiones = (from ap in db.tAprobacionHorasExtra
                                     join a in db.tAsistencia on ap.IdAsistencia equals a.IdAsistencia
                                     join u in db.tUsuario on a.IdUsuario equals u.IdUsuario
                                     where ap.IdAprobador == idAprobador
                                     orderby u.ApellidoPaterno, u.Nombre, a.Fecha descending
                                     select new
                                     {
                                         Empleado = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,
                                         a.Fecha,
                                         a.HorasExtras,
                                         Tipo = a.EstatusHorasExtras,
                                         ap.Motivo,
                                         Estatus = ap.EstatusAprobacion == 2 ? "Aprobado" : "Rechazado"
                                     }).ToList();

                var sb = new StringBuilder();
                sb.AppendFormat(@"
<div style='font-family:Arial;font-size:14px;'>
  <h2 style='color:#003366;'>Reporte de Horas Extra &mdash; GRUPO ANKHAL</h2>
  <p><strong>Jefe de Planta:</strong> {0}</p>
  <p><strong>Planta:</strong> {1}</p>
  <p><strong>Fecha de envio:</strong> {2}</p>
  <br/>
  <table border='1' cellpadding='6' cellspacing='0' style='border-collapse:collapse;width:100%;'>
    <thead style='background-color:#003366;color:white;'>
      <tr>
        <th>Empleado</th><th>Fecha</th><th>Horas Extra</th>
        <th>Tipo</th><th>Motivo</th><th>Estatus</th>
      </tr>
    </thead>
    <tbody>", aprobador, plantaNombre, DateTime.Now.ToString("dd/MM/yyyy HH:mm"));

                foreach (var d in rawDecisiones)
                {
                    decimal redondeadas = RedondearA30Min(d.HorasExtras);
                    if (redondeadas <= 0) continue;
                    string color = d.Estatus == "Aprobado" ? "#d4edda" : "#f8d7da";
                    sb.AppendFormat(@"
      <tr style='background-color:{0};'>
        <td>{1}</td>
        <td>{2}</td>
        <td>{3}</td>
        <td>{4}</td>
        <td>{5}</td>
        <td><strong>{6}</strong></td>
      </tr>", color, d.Empleado,
                        d.Fecha.HasValue ? d.Fecha.Value.ToString("dd/MM/yyyy") : "",
                        FormatearHoras(redondeadas),
                        d.Tipo ?? "", d.Motivo ?? "", d.Estatus);
                }

                sb.Append(@"
    </tbody>
  </table>
  <br/>
  <p>Atentamente,<br/>Sistema de Asistencia<br/><strong>GRUPO ANKHAL</strong></p>
</div>");

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(cfg.CorreoEmisor, "Sistema Asistencia GRUPO ANKHAL");
                mail.To.Add(usuarioRH.Email);
                mail.Subject = string.Format("Horas Extra - {0} - {1}", plantaNombre, DateTime.Now.ToString("dd/MM/yyyy"));
                mail.Body = sb.ToString();
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient(cfg.SmtpHost);
                smtp.Port = cfg.Puerto;
                smtp.EnableSsl = cfg.UsaSSL;
                smtp.Credentials = new NetworkCredential(cfg.CorreoEmisor, cfg.PasswordCorreo);
                smtp.Send(mail);
            }
            catch { }
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

        private string DeterminarEstatusGrupo(List<int> estatuses)
        {
            if (estatuses.All(e => e == 2)) return "Aprobado";
            if (estatuses.Any(e => e == 3)) return "Rechazado";
            return "Pendiente";
        }

        private int DeterminarDecisionValor(List<int> estatuses)
        {
            if (estatuses.All(e => e == 2)) return 2;
            if (estatuses.Any(e => e == 3)) return 3;
            return 0;
        }

        private void MostrarAlerta(string icon, string titulo, string mensaje)
        {
            string safe = mensaje.Replace("'", "\\'");
            string script = string.Format(@"
                Swal.fire({{
                    icon: '{0}',
                    title: '{1}',
                    text: '{2}',
                    showConfirmButton: true
                }});", icon, titulo, safe);
            ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), script, true);
        }
    }
}
