using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
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

        protected void Page_Load(object sender, EventArgs e)
        {
            if (SesionState.usuario == null) { Response.Redirect("login.aspx"); return; }

            string rol = SesionState.usuario.tRol.Rol;
            if (rol != "Jefe de Planta" && rol != "Administrador" && rol != "Rh")
            {
                Response.Redirect("login.aspx"); return;
            }

            if (rol == "Jefe de Planta" && SesionState.usuario.IdPlanta == null)
            {
                MostrarAlerta("error", "Error de configuracion", "Su usuario no tiene planta asignada. Contacte al administrador.");
                return;
            }

            if (!IsPostBack)
            {
                txtFechaInicio.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
                txtFechaFin.Text = DateTime.Now.ToString("yyyy-MM-dd");
                CargarHorasExtra();
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

            int? idPlanta = SesionState.usuario.IdPlanta;
            string rol = SesionState.usuario.tRol.Rol;
            string buscarEmpleado = txtBuscarEmpleado.Text.Trim();

            var query = from a in db.tAsistencia
                        join u in db.tUsuario on a.IdUsuario equals u.IdUsuario
                        join ap in db.tAprobacionHorasExtra
                            on a.IdAsistencia equals ap.IdAsistencia
                            into apGroup
                        from ap in apGroup.DefaultIfEmpty()
                        where a.HorasExtras > 0
                           && a.Fecha >= fi.Date
                           && a.Fecha <= ff.Date
                           && (rol == "Jefe de Planta" ? u.IdPlanta == idPlanta : true)
                        orderby a.Fecha descending
                        select new
                        {
                            a.IdAsistencia,
                            Empleado = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,
                            a.Fecha,
                            a.HorasExtras,
                            TipoHorasExtra = a.EstatusHorasExtras,
                            EstatusTexto = ap == null || ap.EstatusAprobacion == 1 ? "Pendiente"
                                         : ap.EstatusAprobacion == 2 ? "Aprobado"
                                         : "Rechazado",
                            EstatusDecision = ap == null || ap.EstatusAprobacion == 1 ? 0 : ap.EstatusAprobacion,
                            MotivoActual = ap != null ? ap.Motivo : ""
                        };

            var lista = query.ToList();

            if (!string.IsNullOrEmpty(buscarEmpleado))
                lista = lista.Where(x => x.Empleado.ToLower().Contains(buscarEmpleado.ToLower())).ToList();

            // Calcular formato HH:mm en memoria
            var resultado = lista.Select(x => new
            {
                x.IdAsistencia,
                x.Empleado,
                x.Fecha,
                x.HorasExtras,
                HorasExtraFormato = FormatearHoras(x.HorasExtras),
                x.TipoHorasExtra,
                x.EstatusTexto,
                x.EstatusDecision,
                x.MotivoActual
            }).ToList();

            gvHorasExtra.DataSource = resultado;
            gvHorasExtra.DataBind();
        }

        private string FormatearHoras(decimal? horas)
        {
            if (horas == null || horas <= 0) return "00:00";
            var ts = TimeSpan.FromHours((double)horas);
            return string.Format("{0:D2}:{1:D2}", (int)ts.TotalHours, ts.Minutes);
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
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            CargarHorasExtra();
        }

        protected void btnLimpiarFiltros_Click(object sender, EventArgs e)
        {
            txtFechaInicio.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            txtFechaFin.Text = DateTime.Now.ToString("yyyy-MM-dd");
            txtBuscarEmpleado.Text = "";
            CargarHorasExtra();
        }

        protected void gvHorasExtra_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvHorasExtra.PageIndex = e.NewPageIndex;
            CargarHorasExtra();
        }

        protected void btnEnviarRH_Click(object sender, EventArgs e)
        {
            try
            {
                int idAprobador = SesionState.usuario.IdUsuario;
                DateTime ahora = DateTime.Now;
                bool huboDecision = false;

                foreach (GridViewRow row in gvHorasExtra.Rows)
                {
                    if (row.RowType != DataControlRowType.DataRow) continue;

                    int idAsistencia = Convert.ToInt32(gvHorasExtra.DataKeys[row.RowIndex].Value);
                    var ddl = (DropDownList)row.FindControl("ddlDecision");
                    var txtMot = (TextBox)row.FindControl("txtMotivo");

                    int decision = Convert.ToInt32(ddl.SelectedValue);
                    if (decision == 0) continue;

                    huboDecision = true;
                    string motivo = txtMot.Text.Trim();

                    var existente = db.tAprobacionHorasExtra.FirstOrDefault(x => x.IdAsistencia == idAsistencia);
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
                            IdAsistencia = idAsistencia,
                            IdAprobador = idAprobador,
                            EstatusAprobacion = decision,
                            Motivo = motivo,
                            FechaAprobacion = ahora
                        });
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

                var decisiones = (from ap in db.tAprobacionHorasExtra
                                  join a in db.tAsistencia on ap.IdAsistencia equals a.IdAsistencia
                                  join u in db.tUsuario on a.IdUsuario equals u.IdUsuario
                                  where ap.IdAprobador == idAprobador
                                  orderby a.Fecha descending
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
                sb.Append(string.Format(@"
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
    <tbody>", aprobador, plantaNombre, DateTime.Now.ToString("dd/MM/yyyy HH:mm")));

                foreach (var d in decisiones)
                {
                    string color = d.Estatus == "Aprobado" ? "#d4edda" : "#f8d7da";
                    sb.Append(string.Format(@"
      <tr style='background-color:{0};'>
        <td>{1}</td>
        <td>{2}</td>
        <td>{3:N2} ({4})</td>
        <td>{5}</td>
        <td>{6}</td>
        <td><strong>{7}</strong></td>
      </tr>", color, d.Empleado, d.Fecha.Value.ToString("dd/MM/yyyy"),
                        d.HorasExtras, FormatearHoras(d.HorasExtras), d.Tipo, d.Motivo, d.Estatus));
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
