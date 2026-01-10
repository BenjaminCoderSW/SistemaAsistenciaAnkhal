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

                txtNombreEmpleado.Text = SesionState.usuario.Nombre + " " + SesionState.usuario.ApellidoPaterno + " " + SesionState.usuario.ApellidoMaterno;

            }
            else
            {
                SesionState.usuario = null; // limpiar sesión
                Response.Redirect("login.aspx"); // redirigir al login
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

            tComisionHoras pue = new tComisionHoras();

            // Validación
            if (string.IsNullOrWhiteSpace(txtNombreEmpleado.Text))
            {
                string script = "Swal.fire({ icon: 'warning', title: 'Campo obligatorio', text: 'El campo empleado es obligatorio.' });";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertEmpleado", script, true);
                return;
            }

            // Validar campos vacíos
            if (string.IsNullOrWhiteSpace(txtHoraSalida.Text))
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertInicio",
                    "Swal.fire({ icon: 'warning', title: 'Campo obligatorio', text: 'La hora de inicio es obligatoria.' });",
                    true);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtHoraRegreso.Text))
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertFin",
                    "Swal.fire({ icon: 'warning', title: 'Campo obligatorio', text: 'La hora de fin es obligatoria.' });",
                    true);
                return;
            }


            if (string.IsNullOrWhiteSpace(txtDia.Text))
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertFin",
                    "Swal.fire({ icon: 'warning', title: 'Campo obligatorio', text: 'La fecha es obligatoria.' });",
                    true);
                return;
            }

            if (string.IsNullOrWhiteSpace(ddlMotivo.Text))
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertFin",
                    "Swal.fire({ icon: 'warning', title: 'Campo obligatorio', text: 'El motivo es obligatoria.' });",
                    true);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDestino.Text))
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertFin",
                    "Swal.fire({ icon: 'warning', title: 'Campo obligatorio', text: 'El destino es obligatoria.' });",
                    true);
                return;
            }


            DateTime dia = DateTime.Parse(txtDia.Text.Trim());

            // Validar duplicado
            bool existe = db.tComisionHoras.Any(p => p.Fecha == dia);
            if (existe)
            {
                string script = @"
            Swal.fire({
                icon: 'error',
                title: 'Duplicado',
                text: 'Ya existe un permiso registrado para este día.'
            }).then(() => {
                document.getElementById('" + txtDia.ClientID + @"').value = '';
            });
        ";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "SweetAlert", script, true);
                return;
            }

            // Convertir horas
            TimeSpan horaInicio = TimeSpan.Parse(txtHoraSalida.Text.Trim());
            TimeSpan horaFin = TimeSpan.Parse(txtHoraRegreso.Text.Trim());


            // Guardar ✅ YA USANDO EL USUARIO DE LA SESIÓN
            pue.IdUsuario = UsuarioSesion;
            pue.IdJefe = Convert.ToInt32(ddlNombreJefe.SelectedValue);
            pue.CorreoJefe = txtEmail.Text.Trim();
            pue.Motivo = ddlMotivo.Text.Trim();
            pue.Destino = txtDestino.Text.Trim();
            pue.Fecha=dia;
            pue.HoraSalida = horaInicio;
            pue.HoraRegreso = horaFin;
            pue.Horas = Convert.ToDecimal(txtHoras.Text.Trim());
            pue.Observaciones = txtObservaciones.Text.Trim();
            pue.Estatus = 1;

            db.tComisionHoras.InsertOnSubmit(pue);
            db.SubmitChanges();

            // ENVIAR CORREO AL JEFE Y COPIA AL EMPLEADO ✅
            try
            {
                string empleado = txtNombreEmpleado.Text.Trim();
                string correoJefe = txtEmail.Text.Trim();
                string correoEmpleado = SesionState.usuario.Email; // ← correo del usuario logueado
                string motivo = ddlMotivo.SelectedItem.Text.Trim();
                string tipo = txtDestino.Text.Trim();
                string diaPermiso = dia.ToString("dd/MM/yyyy");
                string horaInicioTxt = horaInicio.ToString(@"hh\\:mm");
                string horaFinTxt = horaFin.ToString(@"hh\\:mm");
                string horasTotal = txtHoras.Text.Trim();

                string asunto = "Solicitud de permisos por horas";

                // ------------------ PLANTILLA HTML DEL CORREO -------------------
                string cuerpoHtml = $@"
    <div style='font-family: Arial; font-size: 15px; color: #333;'>
        <h2 style='color:#D81B60;'>Solicitud de Permiso por Horas</h2>
        <p>El empleado <strong>{empleado}</strong> ha registrado una solicitud de permiso.</p>

        <table style='border-collapse: collapse; width: 100%; margin-top: 10px;'>
            <tr style='background-color: #F8BBD0;'>
                <th style='padding: 8px; border: 1px solid #ccc;'>Campo</th>
                <th style='padding: 8px; border: 1px solid #ccc;'>Valor</th>
            </tr>
            <tr>
                <td style='padding: 8px; border: 1px solid #ccc;'>Empleado</td>
                <td style='padding: 8px; border: 1px solid #ccc;'>{empleado}</td>
            </tr>
            <tr>
                <td style='padding: 8px; border: 1px solid #ccc;'>Día solicitado</td>
                <td style='padding: 8px; border: 1px solid #ccc;'>{diaPermiso}</td>
            </tr>
            <tr>
                <td style='padding: 8px; border: 1px solid #ccc;'>Hora Salida</td>
                <td style='padding: 8px; border: 1px solid #ccc;'>{horaInicioTxt}</td>
            </tr>
            <tr>
                <td style='padding: 8px; border: 1px solid #ccc;'>Hora Regreso</td>
                <td style='padding: 8px; border: 1px solid #ccc;'>{horaFinTxt}</td>
            </tr>
            <tr>
                <td style='padding: 8px; border: 1px solid #ccc;'>Horas totales</td>
                <td style='padding: 8px; border: 1px solid #ccc;'>{horasTotal}</td>
            </tr>
            <tr>
                <td style='padding: 8px; border: 1px solid #ccc;'>Motivo</td>
                <td style='padding: 8px; border: 1px solid #ccc;'>{motivo}</td>
            </tr>
            <tr>
                <td style='padding: 8px; border: 1px solid #ccc;'>Destino</td>
                <td style='padding: 8px; border: 1px solid #ccc;'>{tipo}</td>
            </tr>
        </table>

        <p style='margin-top:20px;'>Favor de dar seguimiento.</p>

        <p style='font-size:13px; color:#777;'>(Correo generado automáticamente)</p>
    </div>";
                // -------------------------------------------------------------------

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("tucorreo@dominio.com", "Sistema de Permisos");
                mail.To.Add(correoJefe);

                // Copia al empleado ✅
                if (!string.IsNullOrWhiteSpace(correoEmpleado))
                    mail.CC.Add(correoEmpleado);

                mail.Subject = asunto;
                mail.Body = cuerpoHtml;
                mail.IsBodyHtml = true;

                // Configurar SMTP
                SmtpClient smtp = new SmtpClient("smtp.tudominio.com", 587);
                smtp.Credentials = new NetworkCredential("tucorreo@dominio.com", "tu_password");
                smtp.EnableSsl = true;

                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(
                    this,
                    this.GetType(),
                    "emailError",
                    $"Swal.fire('Advertencia', 'El permiso se guardó, pero el correo no se pudo enviar: {ex.Message}', 'warning');",
                    true
                );
            }





            limpiar();

            string successScript = @"
        Swal.fire({
            icon: 'success',
            title: 'Guardado',
            text: 'El comisión se registró correctamente.',
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
            txtHoraSalida.Text = "";
            txtHoraRegreso.Text = "";
            txtHoras.Text = "";
            txtDia.Text = "";
            txtObservaciones.Text = "";
            // txtTipoPermiso NO se limpia porque es readonly y siempre "Permiso por horas"
        }

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

            // Validación: Hora fin no puede ser menor
            if (horaFin < horaInicio)
            {
                txtHoras.Text = "";

                string successScript = @"
        Swal.fire({
            icon: 'warning',
            title: 'Alerta',
            text: 'La hora de regreso no puede ser menor a la hora de salida.',
            showConfirmButton: false,
            timer: 2000
        });
    ";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertSuccess", successScript, true);


                //ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(),
                //    "alert('La hora fin no puede ser menor a la hora inicio.');", true);
                return;
            }

            // Diferencia en horas decimales
            double horas = (horaFin - horaInicio).TotalHours;

            txtHoras.Text = horas.ToString("0.##");
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