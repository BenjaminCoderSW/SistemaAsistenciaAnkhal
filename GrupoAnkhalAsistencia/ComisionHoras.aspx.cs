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
    public partial class ComisionHoras : System.Web.UI.Page
    {
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
           ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        public int UsuarioSesion;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (SesionState.usuario != null)
            {
                UsuarioSesion = SesionState.usuario.IdUsuario;
                txtNombreEmpleado.Text = SesionState.usuario.Nombre + " " +
                                         SesionState.usuario.ApellidoPaterno + " " +
                                         SesionState.usuario.ApellidoMaterno;
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
            // ── Validaciones de campos obligatorios ───────────────────────────────

            if (string.IsNullOrWhiteSpace(txtNombreEmpleado.Text))
            {
                MostrarAlerta("warning", "Campo obligatorio", "El campo empleado es obligatorio.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtHoraSalida.Text))
            {
                MostrarAlerta("warning", "Campo obligatorio", "La hora de salida es obligatoria.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtHoraRegreso.Text))
            {
                MostrarAlerta("warning", "Campo obligatorio", "La hora de regreso es obligatoria.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDia.Text))
            {
                MostrarAlerta("warning", "Campo obligatorio", "La fecha es obligatoria.");
                return;
            }

            if (string.IsNullOrWhiteSpace(ddlMotivo.SelectedValue))
            {
                MostrarAlerta("warning", "Campo obligatorio", "El motivo es obligatorio.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDestino.Text))
            {
                MostrarAlerta("warning", "Campo obligatorio", "El destino es obligatorio.");
                return;
            }

            // ── Parsear horas ─────────────────────────────────────────────────────

            TimeSpan horaInicio, horaFin;

            if (!TimeSpan.TryParse(txtHoraSalida.Text.Trim(), out horaInicio))
            {
                MostrarAlerta("error", "Hora inválida", "La hora de salida no tiene un formato válido.");
                return;
            }

            if (!TimeSpan.TryParse(txtHoraRegreso.Text.Trim(), out horaFin))
            {
                MostrarAlerta("error", "Hora inválida", "La hora de regreso no tiene un formato válido.");
                return;
            }

            // ── VALIDACIÓN CLAVE: hora de regreso no puede ser menor o igual a salida ──
            if (horaFin <= horaInicio)
            {
                MostrarAlerta("warning", "Horas incorrectas",
                    "La hora de regreso no puede ser menor o igual a la hora de salida. Corrija las horas antes de registrar.");
                return;   // ← CORTE TOTAL: no continúa al INSERT
            }

            // ── Parsear fecha ─────────────────────────────────────────────────────

            DateTime dia;
            if (!DateTime.TryParse(txtDia.Text.Trim(), out dia))
            {
                MostrarAlerta("error", "Fecha inválida", "La fecha ingresada no tiene un formato válido.");
                return;
            }

            // ── Validar duplicado ─────────────────────────────────────────────────

            bool existe = db.tComisionHoras.Any(p =>
                p.Fecha == dia && p.IdUsuario == UsuarioSesion);

            if (existe)
            {
                string script = @"
                    Swal.fire({
                        icon: 'error',
                        title: 'Duplicado',
                        text: 'Ya existe una comisión registrada para este día.'
                    }).then(() => {
                        document.getElementById('" + txtDia.ClientID + @"').value = '';
                    });";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "SweetAlert", script, true);
                return;
            }

            // ── Calcular horas ────────────────────────────────────────────────────

            decimal totalHoras = (decimal)(horaFin - horaInicio).TotalHours;

            // ── Insertar registro ─────────────────────────────────────────────────

            tComisionHoras pue = new tComisionHoras
            {
                IdUsuario = UsuarioSesion,
                IdJefe = Convert.ToInt32(ddlNombreJefe.SelectedValue),
                CorreoJefe = txtEmail.Text.Trim(),
                Motivo = ddlMotivo.Text.Trim(),
                Destino = txtDestino.Text.Trim(),
                Fecha = dia,
                HoraSalida = horaInicio,
                HoraRegreso = horaFin,
                Horas = totalHoras,
                Observaciones = txtObservaciones.Text.Trim(),
                Estatus = 1
            };

            db.tComisionHoras.InsertOnSubmit(pue);
            db.SubmitChanges();

            // ── Enviar correo ─────────────────────────────────────────────────────

            try
            {
                string empleado = txtNombreEmpleado.Text.Trim();
                string correoJefe = txtEmail.Text.Trim();
                string correoEmpleado = SesionState.usuario.Email;
                string motivo = ddlMotivo.SelectedItem.Text.Trim();
                string destino = txtDestino.Text.Trim();
                string diaPermiso = dia.ToString("dd/MM/yyyy");
                string horaInicioTxt = horaInicio.ToString(@"hh\:mm");
                string horaFinTxt = horaFin.ToString(@"hh\:mm");
                string horasTotal = totalHoras.ToString("0.##");

                string cuerpoHtml = $@"
<div style='font-family: Arial; font-size: 15px; color: #333;'>
    <h2 style='color:#D81B60;'>Solicitud de Comisión por Horas</h2>
    <p>El empleado <strong>{empleado}</strong> ha registrado una solicitud de comisión.</p>
    <table style='border-collapse: collapse; width: 100%; margin-top: 10px;'>
        <tr style='background-color: #F8BBD0;'>
            <th style='padding: 8px; border: 1px solid #ccc;'>Campo</th>
            <th style='padding: 8px; border: 1px solid #ccc;'>Valor</th>
        </tr>
        <tr><td style='padding:8px;border:1px solid #ccc;'>Empleado</td>      <td style='padding:8px;border:1px solid #ccc;'>{empleado}</td></tr>
        <tr><td style='padding:8px;border:1px solid #ccc;'>Fecha</td>         <td style='padding:8px;border:1px solid #ccc;'>{diaPermiso}</td></tr>
        <tr><td style='padding:8px;border:1px solid #ccc;'>Hora Salida</td>   <td style='padding:8px;border:1px solid #ccc;'>{horaInicioTxt}</td></tr>
        <tr><td style='padding:8px;border:1px solid #ccc;'>Hora Regreso</td>  <td style='padding:8px;border:1px solid #ccc;'>{horaFinTxt}</td></tr>
        <tr><td style='padding:8px;border:1px solid #ccc;'>Horas totales</td> <td style='padding:8px;border:1px solid #ccc;'>{horasTotal}</td></tr>
        <tr><td style='padding:8px;border:1px solid #ccc;'>Motivo</td>        <td style='padding:8px;border:1px solid #ccc;'>{motivo}</td></tr>
        <tr><td style='padding:8px;border:1px solid #ccc;'>Destino</td>       <td style='padding:8px;border:1px solid #ccc;'>{destino}</td></tr>
    </table>
    <p style='margin-top:20px;'>Favor de dar seguimiento.</p>
    <p style='font-size:13px; color:#777;'>(Correo generado automáticamente)</p>
</div>";

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("tucorreo@dominio.com", "Sistema de Permisos");
                mail.To.Add(correoJefe);

                if (!string.IsNullOrWhiteSpace(correoEmpleado))
                    mail.CC.Add(correoEmpleado);

                mail.Subject = "Solicitud de comisión por horas";
                mail.Body = cuerpoHtml;
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient("smtp.tudominio.com", 587);
                smtp.Credentials = new NetworkCredential("tucorreo@dominio.com", "tu_password");
                smtp.EnableSsl = true;
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "emailError",
                    $"Swal.fire('Advertencia', 'El registro se guardó, pero el correo no se pudo enviar: {ex.Message}', 'warning');",
                    true);
            }

            limpiar();

            string successScript = @"
                Swal.fire({
                    icon: 'success',
                    title: 'Guardado',
                    text: 'La comisión se registró correctamente.',
                    showConfirmButton: false,
                    timer: 2000
                });";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "alertSuccess", successScript, true);
        }

        // ── Calcular horas en AutoPostBack ───────────────────────────────────────
        protected void CalcularHoras(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtHoraSalida.Text) ||
                string.IsNullOrWhiteSpace(txtHoraRegreso.Text))
            {
                txtHoras.Text = "";
                return;
            }

            TimeSpan horaInicio, horaFin;

            if (!TimeSpan.TryParse(txtHoraSalida.Text, out horaInicio) ||
                !TimeSpan.TryParse(txtHoraRegreso.Text, out horaFin))
            {
                txtHoras.Text = "";
                return;
            }

            if (horaFin <= horaInicio)
            {
                txtHoras.Text = "";

                // Avisa al usuario pero NO bloquea el botón por sí solo;
                // el verdadero bloqueo está en btnRegistrar_Click (server-side).
                string warningScript = @"
                    Swal.fire({
                        icon: 'warning',
                        title: 'Alerta',
                        text: 'La hora de regreso no puede ser menor o igual a la hora de salida.',
                        showConfirmButton: false,
                        timer: 2500
                    });";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertHoras", warningScript, true);
                return;
            }

            double horas = (horaFin - horaInicio).TotalHours;
            txtHoras.Text = horas.ToString("0.##");
        }

        // ── AutoPostBack del dropdown de jefe ────────────────────────────────────
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

        // ── Limpiar formulario ───────────────────────────────────────────────────
        public void limpiar()
        {
            txtNombreEmpleado.Text = "";
            ddlNombreJefe.SelectedIndex = 0;
            txtEmail.Text = "";
            ddlMotivo.SelectedIndex = 0;
            txtHoraSalida.Text = "";
            txtHoraRegreso.Text = "";
            txtHoras.Text = "";
            txtDia.Text = "";
            txtObservaciones.Text = "";
            txtDestino.Text = "";
        }

        // ── Helper alerta ────────────────────────────────────────────────────────
        private void MostrarAlerta(string icono, string titulo, string mensaje)
        {
            string script = $@"
                Swal.fire({{
                    icon: '{icono}',
                    title: '{titulo}',
                    text: '{mensaje}',
                    showConfirmButton: false,
                    timer: 2500
                }});";
            ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(), script, true);
        }
    }
}