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
    public partial class AprobarVacacionesJefe : System.Web.UI.Page
    {
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
            ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (SesionState.usuario == null)
            {
                Response.Redirect("login.aspx");
                return;
            }

            if (SesionState.usuario.tRol.Rol != "Jefe de Planta")
            {
                Response.Redirect("login.aspx");
                return;
            }

            if (!IsPostBack)
                CargarVacaciones();
        }

        private void CargarVacaciones(string filtro = "")
        {
            int? idPlanta = SesionState.usuario.IdPlanta;

            var query = from v in db.tVacaciones
                        join u in db.tUsuario on v.IdUsuario equals u.IdUsuario
                        join j in db.tJefe on v.IdJefe equals j.IdJefe
                        join p in db.tPlanta on u.IdPlanta equals p.IdPlanta into pj
                        from p in pj.DefaultIfEmpty()
                        where v.Estatus == 1
                           && v.EstatusJefe == null
                           && u.IdPlanta == idPlanta
                        orderby v.IdVacaciones
                        select new
                        {
                            v.IdVacaciones,
                            Empleado = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,
                            Planta = p != null ? p.Planta : "N/A",
                            JefeSeleccionado = j.Jefe,
                            v.FechaInicio,
                            v.FechaFin,
                            v.Dias,
                            v.FechaSolicitud
                        };

            if (!string.IsNullOrWhiteSpace(filtro))
                query = query.Where(x =>
                    System.Data.Linq.SqlClient.SqlMethods.Like(x.Empleado, "%" + filtro + "%"));

            gvVacaciones.DataSource = query.ToList();
            gvVacaciones.DataBind();
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int id = Convert.ToInt32(btn.CommandArgument);

            GridViewRow row = (GridViewRow)btn.NamingContainer;
            TextBox txtMotivo = (TextBox)row.FindControl("txtMotivo");
            DropDownList ddlDecision = (DropDownList)row.FindControl("ddlDecision");

            int decision = Convert.ToInt32(ddlDecision.SelectedValue);
            string motivo = txtMotivo.Text.Trim();

            if (decision == 0)
            {
                MostrarAlerta("warning", "Selecciona una decisión", "Debes elegir Aprobar o Rechazar antes de guardar.");
                return;
            }

            if (string.IsNullOrWhiteSpace(motivo))
            {
                MostrarAlerta("warning", "Motivo requerido", "Debes escribir un motivo antes de guardar.");
                return;
            }

            var vacacion = db.tVacaciones.FirstOrDefault(v => v.IdVacaciones == id);
            if (vacacion == null) return;

            try
            {
                TimeZoneInfo zona = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");
                DateTime ahora = TimeZoneInfo.ConvertTime(DateTime.Now, zona);

                vacacion.MotivoJefe = motivo;
                vacacion.IdAprobadorJefe = SesionState.usuario.IdUsuario;
                vacacion.FechaResolucionJefe = ahora;

                if (decision == 1) // Aprobar
                {
                    vacacion.EstatusJefe = 1;
                    db.SubmitChanges();
                    EnviarCorreoRHAprobacion(vacacion);

                    CargarVacaciones(txtBuscar.Text.Trim());
                    MostrarAlerta("success", "Aprobada", "La solicitud fue aprobada. Se notificó a RH para la autorización final.");
                }
                else // Rechazar
                {
                    vacacion.EstatusJefe = 2;
                    vacacion.Estatus = 3;
                    db.SubmitChanges();
                    EnviarCorreoEmpleadoRechazo(vacacion, motivo);

                    CargarVacaciones(txtBuscar.Text.Trim());
                    MostrarAlerta("success", "Rechazada", "La solicitud fue rechazada y se notificó al empleado.");
                }
            }
            catch (Exception ex)
            {
                MostrarAlerta("error", "Error", ex.Message);
            }
        }

        private void EnviarCorreoRHAprobacion(tVacaciones vacacion)
        {
            try
            {
                var cfg = db.ConfigCorreo.FirstOrDefault();
                if (cfg == null) return;

                var usuarioRH = db.tUsuario.FirstOrDefault(u => u.IdRol == 3 && u.Estatus == 1);
                if (usuarioRH == null || string.IsNullOrEmpty(usuarioRH.Email)) return;

                var empleado = db.tUsuario.FirstOrDefault(u => u.IdUsuario == vacacion.IdUsuario);
                string nombreEmpleado = empleado != null
                    ? empleado.Nombre + " " + empleado.ApellidoPaterno + " " + empleado.ApellidoMaterno
                    : "N/A";

                string aprobador = SesionState.usuario.Nombre + " " + SesionState.usuario.ApellidoPaterno;

                var planta = db.tPlanta.FirstOrDefault(p => p.IdPlanta == SesionState.usuario.IdPlanta);
                string plantaNombre = planta?.Planta ?? "N/A";

                string cuerpoHtml = $@"
<div style='font-family:Arial;font-size:15px;'>
  <h2 style='color:#003366;'>Solicitud de Vacaciones Pre-aprobada — GRUPO ANKHAL</h2>
  <p>El <strong>Jefe de Planta {aprobador}</strong> ({plantaNombre}) ha <strong style='color:#28a745;'>aprobado</strong> la siguiente solicitud de vacaciones. Pendiente de tu autorización final.</p>
  <br/>
  <table border='1' cellpadding='8' cellspacing='0' style='border-collapse:collapse;width:100%;'>
    <thead style='background-color:#003366;color:white;'>
      <tr><th>Empleado</th><th>Fecha Inicio</th><th>Fecha Fin</th><th>Días</th><th>Motivo del Jefe</th></tr>
    </thead>
    <tbody>
      <tr style='background-color:#d4edda;'>
        <td>{nombreEmpleado}</td>
        <td>{vacacion.FechaInicio:dd/MM/yyyy}</td>
        <td>{vacacion.FechaFin:dd/MM/yyyy}</td>
        <td>{vacacion.Dias}</td>
        <td>{vacacion.MotivoJefe}</td>
      </tr>
    </tbody>
  </table>
  <br/>
  <p>Atentamente,<br/>Sistema de Asistencia<br/><strong>GRUPO ANKHAL</strong></p>
</div>";

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(cfg.CorreoEmisor, "Sistema Asistencia GRUPO ANKHAL");
                mail.To.Add(usuarioRH.Email);
                mail.Subject = $"Vacaciones pre-aprobadas por Jefe — {nombreEmpleado} — GRUPO ANKHAL";
                mail.Body = cuerpoHtml;
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient(cfg.SmtpHost);
                smtp.Port = cfg.Puerto;
                smtp.EnableSsl = cfg.UsaSSL;
                smtp.Credentials = new NetworkCredential(cfg.CorreoEmisor, cfg.PasswordCorreo);
                smtp.Send(mail);
            }
            catch { }
        }

        private void EnviarCorreoEmpleadoRechazo(tVacaciones vacacion, string motivo)
        {
            try
            {
                var cfg = db.ConfigCorreo.FirstOrDefault();
                if (cfg == null) return;

                var empleado = db.tUsuario.FirstOrDefault(u => u.IdUsuario == vacacion.IdUsuario);
                if (empleado == null || string.IsNullOrEmpty(empleado.Email)) return;

                string nombreEmpleado = empleado.Nombre + " " + empleado.ApellidoPaterno + " " + empleado.ApellidoMaterno;
                string aprobador = SesionState.usuario.Nombre + " " + SesionState.usuario.ApellidoPaterno;

                string cuerpoHtml = $@"
<div style='font-family:Arial;font-size:15px;'>
  <h2 style='color:#dc3545;'>Solicitud de Vacaciones Rechazada — GRUPO ANKHAL</h2>
  <p>Hola <strong>{nombreEmpleado}</strong>,</p>
  <p>Tu solicitud de vacaciones ha sido <strong style='color:#dc3545;'>rechazada</strong> por tu Jefe de Planta.</p>
  <br/>
  <p><strong>Fecha inicio solicitada:</strong> {vacacion.FechaInicio:dd/MM/yyyy}</p>
  <p><strong>Fecha fin solicitada:</strong> {vacacion.FechaFin:dd/MM/yyyy}</p>
  <p><strong>Días solicitados:</strong> {vacacion.Dias}</p>
  <p><strong>Motivo del rechazo:</strong> {motivo}</p>
  <br/>
  <p>Si tienes alguna duda, comunícate con tu Jefe de Planta <strong>{aprobador}</strong>.</p>
  <p>Atentamente,<br/>Departamento de Recursos Humanos<br/><strong>GRUPO ANKHAL</strong></p>
</div>";

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(cfg.CorreoEmisor, "Recursos Humanos GRUPO ANKHAL");
                mail.To.Add(empleado.Email);
                mail.Subject = "Solicitud de Vacaciones Rechazada — GRUPO ANKHAL";
                mail.Body = cuerpoHtml;
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient(cfg.SmtpHost);
                smtp.Port = cfg.Puerto;
                smtp.EnableSsl = cfg.UsaSSL;
                smtp.Credentials = new NetworkCredential(cfg.CorreoEmisor, cfg.PasswordCorreo);
                smtp.Send(mail);
            }
            catch { }
        }

        protected void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            CargarVacaciones(txtBuscar.Text.Trim());
        }

        protected void gvVacaciones_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvVacaciones.PageIndex = e.NewPageIndex;
            CargarVacaciones(txtBuscar.Text.Trim());
        }

        private void MostrarAlerta(string icono, string titulo, string mensaje)
        {
            string script = $@"
                Swal.fire({{
                    icon: '{icono}',
                    title: '{titulo}',
                    text: '{mensaje.Replace("'", "\\'")}',
                    showConfirmButton: true
                }});";
            ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), script, true);
        }
    }
}
