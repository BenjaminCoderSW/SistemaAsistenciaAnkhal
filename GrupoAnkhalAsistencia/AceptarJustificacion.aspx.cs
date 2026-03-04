using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class AceptarJustificacion : System.Web.UI.Page
    {
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
            ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        // -------------------------------------------------------
        // Helper para badge (llamado desde ASPX)
        // -------------------------------------------------------
        public string ObtenerBadge(string estatus)
        {
            switch (estatus)
            {
                case "1": return "<span class='badge-pendiente'>Pendiente</span>";
                case "2": return "<span class='badge-aceptada'>Aceptada</span>";
                case "3": return "<span class='badge-rechazada'>Rechazada</span>";
                default: return "<span class='badge-pendiente'>Pendiente</span>";
            }
        }

        // -------------------------------------------------------
        // Ciclo de vida
        // -------------------------------------------------------
        protected void Page_Load(object sender, EventArgs e)
        {
            if (SesionState.usuario == null)
            {
                Response.Redirect("login.aspx"); return;
            }

            string rol = SesionState.usuario.tRol.Rol;
            if (rol != "Administrador" && rol != "Rh")
            {
                Response.Redirect("login.aspx"); return;
            }

            if (!IsPostBack)
            {
                ddlEstatus.SelectedValue = "1"; // Por defecto: pendientes
                CargarJustificaciones();
            }
        }

        // -------------------------------------------------------
        // Carga de datos
        // -------------------------------------------------------
        private void CargarJustificaciones(string busqueda = "")
        {
            int estatusFiltro;
            int.TryParse(ddlEstatus.SelectedValue, out estatusFiltro);

            var query = from j in db.tJustificacion
                        join a in db.tAsistencia on j.IdAsistencia equals a.IdAsistencia
                        join u in db.tUsuario on j.IdUsuario equals u.IdUsuario
                        join ah in db.tAsignarHorario on a.IdAsignarHorario equals ah.IdAsignarHorario into ahJoin
                        from ah in ahJoin.DefaultIfEmpty()
                        join h in db.tHorario on (ah != null ? ah.IdHorario : 0) equals h.IdHorario into hJoin
                        from h in hJoin.DefaultIfEmpty()
                        select new
                        {
                            j.IdJustificacion,
                            j.IdAsistencia,
                            j.IdUsuario,
                            NombreCompleto = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,
                            CorreoEmpleado = u.Email,
                            FechaAsistencia = a.Fecha,
                            EstatusEntrada = a.EstatusEntrada,
                            HoraEntrada = a.HoraEntrada,
                            HoraInicio = h != null ? h.HoraInicio : (TimeSpan?)null,
                            j.Motivo,
                            j.Observaciones,
                            FechaJustificacion = j.Fecha,
                            Estatus = j.Estatus
                        };

            // Filtro por estatus (0 = todos)
            if (estatusFiltro > 0)
                query = query.Where(x => x.Estatus == estatusFiltro);

            // Filtro por nombre
            if (!string.IsNullOrEmpty(busqueda))
                query = query.Where(x =>
                    System.Data.Linq.SqlClient.SqlMethods.Like(x.NombreCompleto, "%" + busqueda + "%"));

            dvgJustificaion.DataSource = query.OrderByDescending(x => x.FechaJustificacion).ToList();
            dvgJustificaion.DataBind();
        }

        // -------------------------------------------------------
        // Eventos de filtros
        // -------------------------------------------------------
        protected void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            dvgJustificaion.PageIndex = 0;
            CargarJustificaciones(txtBuscar.Text.Trim());
        }

        protected void ddlEstatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            dvgJustificaion.PageIndex = 0;
            CargarJustificaciones(txtBuscar.Text.Trim());
        }

        protected void dvgJustificaion_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dvgJustificaion.PageIndex = e.NewPageIndex;
            CargarJustificaciones(txtBuscar.Text.Trim());
        }

        // -------------------------------------------------------
        // AUTORIZAR
        // -------------------------------------------------------
        protected void btnAutorizar_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string[] partes = btn.CommandArgument.Split('|');

            int idJustificacion = Convert.ToInt32(partes[0]);
            int idAsistencia = Convert.ToInt32(partes[1]);

            // HoraInicio puede venir como "HH:MM:SS" o vacío
            TimeSpan? horaInicio = null;
            if (partes.Length > 2 && !string.IsNullOrWhiteSpace(partes[2]))
            {
                TimeSpan ts;
                if (TimeSpan.TryParse(partes[2], out ts))
                    horaInicio = ts;
            }

            try
            {
                // 1. Actualizar tJustificacion → Estatus = 2 (Aceptada)
                var justif = db.tJustificacion.FirstOrDefault(j => j.IdJustificacion == idJustificacion);
                if (justif == null) { MostrarAlerta("error", "Error", "Justificación no encontrada."); return; }

                justif.Estatus = 2;

                // 2. Actualizar tAsistencia
                var asistencia = db.tAsistencia.FirstOrDefault(a => a.IdAsistencia == idAsistencia);
                if (asistencia != null)
                {
                    asistencia.IdJustificacion = idJustificacion;
                    asistencia.Justificacion = "Aceptada";
                    asistencia.EstatusEntrada = "A tiempo";

                    // Ajustar hora de entrada a la hora programada si se tiene
                    if (horaInicio.HasValue)
                        asistencia.HoraEntrada = horaInicio;
                }

                db.SubmitChanges();

                // 3. Enviar correo al empleado
                if (justif.IdUsuario.HasValue)
                    EnviarCorreo(justif.IdUsuario.Value, idAsistencia, "Aceptada", justif.Motivo);

                CargarJustificaciones(txtBuscar.Text.Trim());

                MostrarAlerta("success", "Autorizada",
                    "La justificación fue autorizada y el estatus del empleado fue actualizado a 'A tiempo'.");
            }
            catch (Exception ex)
            {
                MostrarAlerta("error", "Error", "No se pudo autorizar: " + ex.Message);
            }
        }

        // -------------------------------------------------------
        // RECHAZAR
        // -------------------------------------------------------
        protected void btnRechazar_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string[] partes = btn.CommandArgument.Split('|');

            int idJustificacion = Convert.ToInt32(partes[0]);
            int idAsistencia = Convert.ToInt32(partes[1]);

            try
            {
                // 1. Actualizar tJustificacion → Estatus = 3 (Rechazada)
                var justif = db.tJustificacion.FirstOrDefault(j => j.IdJustificacion == idJustificacion);
                if (justif == null) { MostrarAlerta("error", "Error", "Justificación no encontrada."); return; }

                justif.Estatus = 3;

                // 2. Actualizar campo Justificacion en tAsistencia → "Rechazada"
                var asistencia = db.tAsistencia.FirstOrDefault(a => a.IdAsistencia == idAsistencia);
                if (asistencia != null)
                    asistencia.Justificacion = "Rechazada";

                db.SubmitChanges();

                // 3. Enviar correo al empleado
                if (justif.IdUsuario.HasValue)
                    EnviarCorreo(justif.IdUsuario.Value, idAsistencia, "Rechazada", justif.Motivo);

                CargarJustificaciones(txtBuscar.Text.Trim());

                MostrarAlerta("success", "Rechazada",
                    "La justificación fue rechazada. El empleado podrá enviar una nueva solicitud.");
            }
            catch (Exception ex)
            {
                MostrarAlerta("error", "Error", "No se pudo rechazar: " + ex.Message);
            }
        }

        // -------------------------------------------------------
        // Correo
        // -------------------------------------------------------
        private void EnviarCorreo(int idUsuario, int idAsistencia, string resultado, string motivo)
        {
            try
            {
                var cfg = db.ConfigCorreo.FirstOrDefault();
                var usuario = db.tUsuario.FirstOrDefault(u => u.IdUsuario == idUsuario);

                if (cfg == null || usuario == null || string.IsNullOrEmpty(usuario.Email))
                    return;

                var asistencia = db.tAsistencia.FirstOrDefault(a => a.IdAsistencia == idAsistencia);
                string nombre = usuario.Nombre + " " + usuario.ApellidoPaterno + " " + usuario.ApellidoMaterno;
                string fecha = asistencia?.Fecha?.ToString("dd/MM/yyyy") ?? "";
                string color = resultado == "Aceptada" ? "#28a745" : "#dc3545";

                string cuerpo = $@"
                    <div style='font-family: Arial; font-size: 15px;'>
                        <h2 style='color:{color};'>Justificación {resultado}</h2>
                        <p>Hola <strong>{nombre}</strong>,</p>
                        <p>Tu solicitud de justificación ha sido <strong>{resultado}</strong>.</p>
                        <p><strong>Fecha del registro:</strong> {fecha}</p>
                        <p><strong>Motivo enviado:</strong> {motivo}</p>
                        {(resultado == "Rechazada" ? "<p>Puedes volver a enviar tu solicitud desde el sistema si lo consideras necesario.</p>" : "<p>Tu asistencia fue actualizada a <strong>A tiempo</strong>.</p>")}
                        <br/>
                        <p>Atentamente,<br>Departamento de Recursos Humanos<br>GRUPO ANKHAL</p>
                    </div>";

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(cfg.CorreoEmisor, "Recursos Humanos GRUPO ANKHAL");
                mail.To.Add(usuario.Email);
                mail.Subject = $"Justificación {resultado} - GRUPO ANKHAL";
                mail.Body = cuerpo;
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient(cfg.SmtpHost);
                smtp.Port = cfg.Puerto;
                smtp.EnableSsl = cfg.UsaSSL;
                smtp.Credentials = new NetworkCredential(cfg.CorreoEmisor, cfg.PasswordCorreo);
                smtp.Send(mail);
            }
            catch { /* No interrumpir el flujo si el correo falla */ }
        }

        // -------------------------------------------------------
        // Helper UI
        // -------------------------------------------------------
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