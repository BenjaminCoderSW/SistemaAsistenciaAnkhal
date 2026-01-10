using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class PermisoDias : System.Web.UI.Page
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
                txtNombreEmpleado.Text = SesionState.usuario.Nombre + " " + SesionState.usuario.ApellidoPaterno + " " + SesionState.usuario.ApellidoMaterno;

            }
            else
            {
                SesionState.usuario = null; // limpiar sesión
                Response.Redirect("login.aspx"); // redirigir al login
                return;
            }
            if (!IsPostBack)
            {
                CargarJefe();  // SOLO LA PRIMERA VEZ
            }
        }

        private void CargarJefe()
        {
            var jefe = db.tJefe
                .Select(t => new { t.IdJefe, t.Jefe, t.Correo })
                .ToList();

            // DropDown principal
            ddlNombreJefe.DataSource = jefe;
            ddlNombreJefe.DataTextField = "Jefe";
            ddlNombreJefe.DataValueField = "IdJefe";
            ddlNombreJefe.DataBind();
            ddlNombreJefe.Items.Insert(0, new ListItem("-- Seleccione --", "0"));

        }

     

        protected void btnRegistrar_Click(object sender, EventArgs e)
        {
            // ---------------- VALIDACIÓN USUARIO ----------------
            if (SesionState.usuario == null)
            {
                Response.Redirect("login.aspx");
                return;
            }

            int usuarioSesion = SesionState.usuario.IdUsuario;

            // ---------------- VALIDAR CAMPOS ----------------
            if (string.IsNullOrWhiteSpace(txtNombreEmpleado.Text))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alertNombre",
                    "Swal.fire('Campo obligatorio','Debes seleccionar un empleado.','warning');", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtFechaInicio.Text))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alertInicio",
                    "Swal.fire('Campo obligatorio','La fecha de inicio es obligatoria.','warning');", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtFechaFin.Text))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alertFin",
                    "Swal.fire('Campo obligatorio','La fecha de fin es obligatoria.','warning');", true);
                return;
            }

            // ----------- PARSEAR FECHAS (FORMATO DEL INPUT DATE) ----------
            DateTime fechaInicio;
            DateTime fechaFin;

            if (!DateTime.TryParseExact(
                 txtFechaInicio.Text.Trim(),
                 "dd/MM/yyyy",
                 System.Globalization.CultureInfo.InvariantCulture,
                 System.Globalization.DateTimeStyles.None,
                 out fechaInicio))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "errorFechaInicio",
                    "Swal.fire('Error','La fecha de inicio no es válida.','error');", true);
                return;
            }

            if (!DateTime.TryParseExact(txtFechaFin.Text.Trim(),
                                        "dd/MM/yyyy",
                                        System.Globalization.CultureInfo.InvariantCulture,
                                        System.Globalization.DateTimeStyles.None,
                                        out fechaFin))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "errorFechaFin",
                    "Swal.fire('Error','La fecha de fin no es válida.','error');", true);
                return;
            }

            // ----------- VALIDAR REGISTRO DUPLICADO ----------
            bool existe = db.tPermisoDias.Any(p =>
                p.IdUsuario == usuarioSesion &&
                p.FechaInicio == fechaInicio &&
                p.FechaFin == fechaFin
            );

            if (existe)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "duplicado",
                    "Swal.fire('Duplicado','Ya registraste un permiso con esas fechas.','error');", true);
                return;
            }

            // ----------- GUARDAR EN BASE DE DATOS ----------
            tPermisoDias pue = new tPermisoDias
            {
                IdUsuario = usuarioSesion,
                IdJefe = Convert.ToInt32(ddlNombreJefe.SelectedValue),
                CorreoJefe = txtEmail.Text.Trim(),
                Motivo = ddlMotivo.Text.Trim(),
                TipoPermiso = txtTipoPermiso.Text.Trim(),
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                Dias = string.IsNullOrWhiteSpace(hdnDias.Value) ? 0 : Convert.ToInt32(hdnDias.Value),
                Observaciones = txtObservaciones.Text.Trim(),
                Estatus = 1
            };

            db.tPermisoDias.InsertOnSubmit(pue);
            db.SubmitChanges();

            // -------------------- ENVIAR CORREO --------------------
            try
            {
                string empleado = txtNombreEmpleado.Text.Trim();
                string correoJefe = txtEmail.Text.Trim();
                string correoEmpleado = SesionState.usuario.Email;

                // 👉 OBTENER CONFIGURACIÓN DESDE BD (igual que en la otra clase)
                var configService = new PermisoDias();
                var cfg = configService.ObtenerConfig();

                if (cfg == null)
                    throw new Exception("No existe configuración de correo.");

                string cuerpoHtml = $@"
                <div style='font-family: Arial; font-size: 15px;'>
                    <h2 style='color:#D81B60;'>Solicitud de Permiso por Días</h2>
                    <p>El empleado <strong>{empleado}</strong> ha registrado una solicitud de permiso.</p>
                    <p><strong>Fecha inicio:</strong> {fechaInicio:yyyy-MM-dd}</p>
                    <p><strong>Fecha fin:</strong> {fechaFin:yyyy-MM-dd}</p>
                    <p><strong>Días:</strong> {pue.Dias}</p>
                    <p><strong>Motivo:</strong> {pue.Motivo}</p>
                    <p><strong>Tipo permiso:</strong> {pue.TipoPermiso}</p>
                    <br>
                    <small>Correo generado automáticamente</small>
                </div>";

                // 👉 ARMAR CORREO
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(cfg.CorreoEmisor, "Sistema de Permisos GRUPO ANKAHL");
                mail.To.Add(correoJefe);

                if (!string.IsNullOrWhiteSpace(correoEmpleado))
                    mail.CC.Add(correoEmpleado);

                mail.Subject = "Solicitud de Permiso por Días GRUPO ANKHAL";
                mail.Body = cuerpoHtml;
                mail.IsBodyHtml = true;

                // 👉 SMTP DESDE BD (según tu tabla)
                SmtpClient smtp = new SmtpClient(cfg.SmtpHost);
                smtp.Port = cfg.Puerto;              // 587
                smtp.EnableSsl = cfg.UsaSSL;         // True
                smtp.Credentials = new NetworkCredential(cfg.CorreoEmisor, cfg.PasswordCorreo);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "emailError",
                    $"Swal.fire('Advertencia','El permiso se guardó, pero el correo no se pudo enviar: {ex.Message}','warning');", true);
            }

            // ---------------- LIMPIAR CAMPOS ----------------
            limpiar();

            ScriptManager.RegisterStartupScript(this, GetType(), "success",
                "Swal.fire('Guardado','El permiso fue registrado correctamente.','success');", true);
        }

        public void limpiar()
        {
          
            ddlNombreJefe.SelectedIndex = 0;
            txtEmail.Text = "";
            ddlMotivo.SelectedIndex = 0;
            txtFechaInicio.Text = "";
            txtFechaFin.Text = "";
            txtDias.Text = "";
            txtObservaciones.Text = "";
            // txtTipoPermiso NO se limpia porque es readonly y siempre "Permiso por horas"
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
    }
}