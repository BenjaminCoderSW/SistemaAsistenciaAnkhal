using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class ComisionDias : System.Web.UI.Page
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
                SesionState.usuario = null;
                Response.Redirect("login.aspx");
            }
            if (!IsPostBack)
            {
                CargarJefe();
            }
        }

        private void CargarJefe()
        {
            var jefe = db.tJefe
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
            tComisionDia pue = new tComisionDia();

            if (string.IsNullOrWhiteSpace(txtNombreEmpleado.Text))
            {
                string script = "Swal.fire({ icon: 'warning', title: 'Campo obligatorio', text: 'El campo empleado es obligatorio.' });";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertEmpleado", script, true);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtFechaSalida.Text))
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertInicio",
                    "Swal.fire({ icon: 'warning', title: 'Campo obligatorio', text: 'La fecha de salida es obligatoria.' });",
                    true);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtFechaRegreso.Text))
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertFin",
                    "Swal.fire({ icon: 'warning', title: 'Campo obligatorio', text: 'La fecha de regreso es obligatoria.' });",
                    true);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDestino.Text))
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertDestino",
                    "Swal.fire({ icon: 'warning', title: 'Campo obligatorio', text: 'El destino es obligatorio.' });",
                    true);
                return;
            }

            if (string.IsNullOrWhiteSpace(ddlTransporte.SelectedValue))
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertTransporte",
                    "Swal.fire({ icon: 'warning', title: 'Campo obligatorio', text: 'El transporte es obligatorio.' });",
                    true);
                return;
            }

            DateTime fechaSalida = Convert.ToDateTime(txtFechaSalida.Text.Trim());
            DateTime fechaRegreso = Convert.ToDateTime(txtFechaRegreso.Text.Trim());

            bool existe = db.tComisionDia.Any(p =>
                p.IdUsuario == UsuarioSesion &&
                p.FechaSalida == fechaSalida &&
                p.FechaRegreso == fechaRegreso
            );

            if (existe)
            {
                string script = @"
        Swal.fire({
            icon: 'error',
            title: 'Duplicado',
            text: 'Ya registraste una comisión con el mismo día y horas.'
        }).then(() => {
            document.getElementById('" + txtFechaSalida.ClientID + @"').value = '';
        });
    ";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "SweetAlert", script, true);
                return;
            }

            // Número de viajes: solo aplica cuando el motivo es "Viajes"
            int? numViajes = null;
            if (ddlMotivo.SelectedValue == "Viajes" && !string.IsNullOrWhiteSpace(txtViajes.Text))
            {
                int viajesParsed;
                if (int.TryParse(txtViajes.Text.Trim(), out viajesParsed))
                    numViajes = viajesParsed;
            }

            pue.IdUsuario = UsuarioSesion;
            pue.IdJefe = Convert.ToInt32(ddlNombreJefe.SelectedValue);
            pue.CorreoJefe = txtEmail.Text.Trim();
            pue.Motivo = ddlMotivo.Text.Trim();
            pue.Destino = txtDestino.Text.Trim();
            pue.FechaSalida = fechaSalida;
            pue.FechaRegreso = fechaRegreso;
            pue.Dias = string.IsNullOrWhiteSpace(hdnDias.Value) ? 0 : Convert.ToInt32(hdnDias.Value);
            pue.Hospedaje = ddlHospedaje.Text.Trim();
            pue.Transporte = ddlTransporte.Text.Trim();
            pue.Observaciones = txtObservaciones.Text.Trim();
            pue.Viajes = numViajes;   // INT NULL — nueva columna en tComisionDia
            pue.Estatus = 1;

            db.tComisionDia.InsertOnSubmit(pue);
            db.SubmitChanges();

            try
            {
                string empleado = txtNombreEmpleado.Text.Trim();
                string correoJefe = txtEmail.Text.Trim();
                string correoEmpleado = SesionState.usuario.Email;
                string motivoTxt = ddlMotivo.SelectedItem.Text.Trim();
                string destino = txtDestino.Text.Trim();
                string fechaInicioTxt = fechaSalida.ToString("yyyy-MM-dd");
                string fechaFinTxt = fechaRegreso.ToString("yyyy-MM-dd");
                string diasTotal = txtDias.Text.Trim();

                string filaViajes = "";
                if (numViajes.HasValue)
                {
                    filaViajes = $@"
                        <tr>
                            <td style='padding: 8px; border: 1px solid #ccc;'>Número de viajes</td>
                            <td style='padding: 8px; border: 1px solid #ccc;'>{numViajes.Value}</td>
                        </tr>";
                }

                var configService = new ComisionDias();
                var cfg = configService.ObtenerConfig();

                if (cfg == null)
                    throw new Exception("No existe configuración de correo.");

                string cuerpoHtml = $@"
                <div style='font-family: Arial; font-size: 15px; color: #333;'>
                    <h2 style='color:#D81B60;'>Solicitud de Comisión por días</h2>
                    <p>El empleado <strong>{empleado}</strong> ha registrado una solicitud de comisión.</p>
                    <table style='border-collapse: collapse; width: 100%; margin-top: 10px;'>
                        <tr style='background-color: #F8BBD0;'>
                            <th style='padding: 8px; border: 1px solid #ccc;'>Campo</th>
                            <th style='padding: 8px; border: 1px solid #ccc;'>Valor</th>
                        </tr>
                        <tr><td style='padding: 8px; border: 1px solid #ccc;'>Empleado</td>      <td style='padding: 8px; border: 1px solid #ccc;'>{empleado}</td></tr>
                        <tr><td style='padding: 8px; border: 1px solid #ccc;'>Fecha salida</td>  <td style='padding: 8px; border: 1px solid #ccc;'>{fechaInicioTxt}</td></tr>
                        <tr><td style='padding: 8px; border: 1px solid #ccc;'>Fecha regreso</td> <td style='padding: 8px; border: 1px solid #ccc;'>{fechaFinTxt}</td></tr>
                        <tr><td style='padding: 8px; border: 1px solid #ccc;'>Días totales</td>  <td style='padding: 8px; border: 1px solid #ccc;'>{diasTotal}</td></tr>
                        <tr><td style='padding: 8px; border: 1px solid #ccc;'>Motivo</td>        <td style='padding: 8px; border: 1px solid #ccc;'>{motivoTxt}</td></tr>
                        <tr><td style='padding: 8px; border: 1px solid #ccc;'>Destino</td>       <td style='padding: 8px; border: 1px solid #ccc;'>{destino}</td></tr>
                        {filaViajes}
                    </table>
                    <p style='margin-top:20px;'>Favor de dar seguimiento.</p>
                    <p style='font-size:13px; color:#777;'>(Correo generado automáticamente)</p>
                </div>";

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(cfg.CorreoEmisor, "Sistema de Comisión GRUPO ANKHAL");
                mail.To.Add(correoJefe);

                if (!string.IsNullOrWhiteSpace(correoEmpleado))
                    mail.CC.Add(correoEmpleado);

                mail.Subject = "Solicitud de comisión por días GRUPO ANKHAL";
                mail.Body = cuerpoHtml;
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient(cfg.SmtpHost);
                smtp.Port = cfg.Puerto;
                smtp.EnableSsl = cfg.UsaSSL;
                smtp.Credentials = new NetworkCredential(cfg.CorreoEmisor, cfg.PasswordCorreo);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "emailError",
                    $"Swal.fire('Advertencia','La comisión se guardó, pero el correo no se pudo enviar: {ex.Message}', 'warning');", true);
            }

            limpiar();

            string successScript = @"
        Swal.fire({
            icon: 'success',
            title: 'Guardado',
            text: 'La comisión se registró correctamente.',
            showConfirmButton: false,
            timer: 2000
        });
    ";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "alertSuccess", successScript, true);
        }

        public void limpiar()
        {
            txtNombreEmpleado.Text = "";
            ddlNombreJefe.SelectedIndex = 0;
            txtEmail.Text = "";
            ddlMotivo.SelectedIndex = 0;
            txtFechaSalida.Text = "";
            txtFechaRegreso.Text = "";
            txtDias.Text = "";
            hdnDias.Value = "";
            txtViajes.Text = "1";
            ddlHospedaje.SelectedIndex = 0;
            ddlTransporte.SelectedIndex = 0;
            txtObservaciones.Text = "";
        }

        protected void ddlNombreJefe_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idJefe = Convert.ToInt32(ddlNombreJefe.SelectedValue);

            if (idJefe > 0)
            {
                var jefe = db.tJefe.FirstOrDefault(j => j.IdJefe == idJefe);
                if (jefe != null)
                    txtEmail.Text = jefe.Correo;
            }
            else
            {
                txtEmail.Text = "";
            }
        }
    }
}