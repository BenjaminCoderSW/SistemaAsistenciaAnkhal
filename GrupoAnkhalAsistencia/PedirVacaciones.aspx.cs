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
    public partial class PedirVacaciones : System.Web.UI.Page
    {
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
            ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        public int UsuarioSesion;

        public ConfigCorreo ObtenerConfig()
        {
            return db.ConfigCorreo.FirstOrDefault();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (SesionState.usuario != null)
            {
                UsuarioSesion = SesionState.usuario.IdUsuario;
                txtNombreEmpleado.Text = SesionState.usuario.Nombre + " " +
                    SesionState.usuario.ApellidoPaterno + " " +
                    SesionState.usuario.ApellidoMaterno;

                // Calcular días autorizados según antigüedad
                CalcularDiasAutorizados();
            }
            else
            {
                SesionState.usuario = null;
                Response.Redirect("login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                CargarJefe();
            }
        }

        private void CalcularDiasAutorizados()
        {
            if (SesionState.usuario.FechaIngreso.HasValue)
            {
                DateTime fechaIngreso = SesionState.usuario.FechaIngreso.Value;
                int antiguedad = DateTime.Now.Year - fechaIngreso.Year;

                // Lógica según Ley Federal del Trabajo
                int diasAutorizados = 0;
                if (antiguedad >= 1) diasAutorizados = 12;
                if (antiguedad >= 2) diasAutorizados = 14;
                if (antiguedad >= 3) diasAutorizados = 16;
                if (antiguedad >= 4) diasAutorizados = 18;
                if (antiguedad >= 5) diasAutorizados = 20;
                if (antiguedad >= 10) diasAutorizados = 22;
                if (antiguedad >= 15) diasAutorizados = 24;
                if (antiguedad >= 20) diasAutorizados = 26;
                if (antiguedad >= 25) diasAutorizados = 28;
                if (antiguedad >= 30) diasAutorizados = 30;

                txtDiasAutorizados.Text = diasAutorizados.ToString();
            }
        }

        private void CargarJefe()
        {
            var jefe = db.tJefe
                .Where(t => t.Estatus == 1)
                .Select(t => new { t.IdJefe, t.Jefe, t.Correo })
                .ToList();

            ddlNombreJefe.DataSource = jefe;
            ddlNombreJefe.DataTextField = "Jefe";
            ddlNombreJefe.DataValueField = "IdJefe";
            ddlNombreJefe.DataBind();
            ddlNombreJefe.Items.Insert(0, new ListItem("-- Seleccione --", "0"));
        }

        protected void btnRegistrar_Click(object sender, EventArgs e)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(txtFechaInicio.Text))
            {
                MostrarSwal("warning", "Campo obligatorio", "La fecha de inicio es obligatoria.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtFechaFin.Text))
            {
                MostrarSwal("warning", "Campo obligatorio", "La fecha de fin es obligatoria.");
                return;
            }

            if (ddlNombreJefe.SelectedValue == "0")
            {
                MostrarSwal("warning", "Campo obligatorio", "Debe seleccionar un jefe.");
                return;
            }

            DateTime fechaInicio, fechaFin;

            if (!DateTime.TryParseExact(txtFechaInicio.Text.Trim(), "dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out fechaInicio))
            {
                MostrarSwal("error", "Error", "La fecha de inicio no es válida.");
                return;
            }

            if (!DateTime.TryParseExact(txtFechaFin.Text.Trim(), "dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out fechaFin))
            {
                MostrarSwal("error", "Error", "La fecha de fin no es válida.");
                return;
            }

            // Validar que la fecha fin sea mayor o igual a fecha inicio
            if (fechaFin < fechaInicio)
            {
                MostrarSwal("warning", "Fechas inválidas", "La fecha de fin no puede ser anterior a la fecha de inicio.");
                return;
            }

            // Validar duplicado
            bool existe = db.tVacaciones.Any(v =>
                v.IdUsuario == UsuarioSesion &&
                v.FechaInicio == fechaInicio &&
                v.FechaFin == fechaFin
            );

            if (existe)
            {
                MostrarSwal("error", "Duplicado", "Ya registraste una solicitud con esas fechas.");
                return;
            }

            // Guardar con Try-Catch
            try
            {
                tVacaciones vacacion = new tVacaciones
                {
                    IdUsuario = UsuarioSesion,
                    IdJefe = Convert.ToInt32(ddlNombreJefe.SelectedValue),
                    CorreoJefe = txtEmail.Text.Trim(),
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    Dias = string.IsNullOrWhiteSpace(hdnDias.Value) ? 0 : Convert.ToInt32(hdnDias.Value),
                    Estatus = 1 // Pendiente
                };

                db.tVacaciones.InsertOnSubmit(vacacion);
                db.SubmitChanges();

                // Enviar correo
                bool correoEnviado = true;
                try
                {
                    var cfg = ObtenerConfig();
                    if (cfg != null)
                    {
                        string empleado = txtNombreEmpleado.Text.Trim();
                        string correoJefe = txtEmail.Text.Trim();
                        string correoEmpleado = SesionState.usuario.Email;

                        string cuerpoHtml = $@"
                    <div style='font-family: Arial; font-size: 15px;'>
                        <h2 style='color:#003366;'>Solicitud de Vacaciones</h2>
                        <p>El empleado <strong>{empleado}</strong> ha solicitado vacaciones.</p>
                        <p><strong>Fecha inicio:</strong> {fechaInicio:dd/MM/yyyy}</p>
                        <p><strong>Fecha fin:</strong> {fechaFin:dd/MM/yyyy}</p>
                        <p><strong>Días:</strong> {vacacion.Dias}</p>
                        <br>
                        <small>Correo generado automáticamente</small>
                    </div>";

                        MailMessage mail = new MailMessage();
                        mail.From = new MailAddress(cfg.CorreoEmisor, "Sistema de Vacaciones GRUPO ANKHAL");
                        mail.To.Add(correoJefe);

                        if (!string.IsNullOrWhiteSpace(correoEmpleado))
                            mail.CC.Add(correoEmpleado);

                        mail.Subject = "Solicitud de Vacaciones GRUPO ANKHAL";
                        mail.Body = cuerpoHtml;
                        mail.IsBodyHtml = true;

                        SmtpClient smtp = new SmtpClient(cfg.SmtpHost);
                        smtp.Port = cfg.Puerto;
                        smtp.EnableSsl = cfg.UsaSSL;
                        smtp.Credentials = new NetworkCredential(cfg.CorreoEmisor, cfg.PasswordCorreo);

                        smtp.Send(mail);
                    }
                }
                catch (Exception)
                {
                    correoEnviado = false;
                }

                // Limpiar ANTES de mostrar alerta
                Limpiar();

                // Mostrar alerta según resultado
                if (correoEnviado)
                {
                    // TODO EXITOSO
                    string script = @"
                Swal.fire({
                    icon: 'success',
                    title: '¡Solicitud enviada!',
                    text: 'Tu solicitud fue registrada correctamente y se notificó a tu jefe.',
                    showConfirmButton: false,
                    timer: 2500
                });";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "alertExito", script, true);
                }
                else
                {
                    // GUARDADO PERO SIN CORREO
                    string script = @"
                Swal.fire({
                    icon: 'warning',
                    title: 'Solicitud guardada',
                    text: 'La solicitud se guardó correctamente, pero no se pudo enviar el correo de notificación.',
                    showConfirmButton: true,
                    confirmButtonText: 'Entendido'
                });";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "alertAdvertencia", script, true);
                }
            }
            catch (Exception ex)
            {
                // ERROR AL GUARDAR
                MostrarSwal("error", "Error al guardar", $"Ocurrió un error: {ex.Message}");
            }
        }

        public void Limpiar()
        {
            ddlNombreJefe.SelectedIndex = 0;
            txtEmail.Text = "";
            txtFechaInicio.Text = "";
            txtFechaFin.Text = "";
            txtDias.Text = "";
            hdnDias.Value = "";
        }

        protected void ddlNombreJefe_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idJefe = Convert.ToInt32(ddlNombreJefe.SelectedValue);

            if (idJefe > 0)
            {
                var jefe = db.tJefe.FirstOrDefault(j => j.IdJefe == idJefe);
                if (jefe != null)
                {
                    txtEmail.Text = jefe.Correo;
                }
            }
            else
            {
                txtEmail.Text = "";
            }
        }

        private void MostrarSwal(string tipo, string titulo, string mensaje)
        {
            string script = $@"
                Swal.fire({{
                    icon: '{tipo}',
                    title: '{titulo}',
                    text: '{mensaje}',
                    timer: 2000,
                    showConfirmButton: false
                }});";

            ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(), script, true);
        }
    }
}