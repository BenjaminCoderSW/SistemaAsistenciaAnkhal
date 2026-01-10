using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Configuration;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Net.Mail;


namespace GrupoAnkhalAsistencia
{
    public partial class PermisosHoras : System.Web.UI.Page
    {
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
            ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

       public  int UsuarioSesion;

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

            tPermisoHora pue = new tPermisoHora();

            // Validación
            if (string.IsNullOrWhiteSpace(txtNombreEmpleado.Text))
            {
                string script = "Swal.fire({ icon: 'warning', title: 'Campo obligatorio', text: 'El campo empleado es obligatorio.' });";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertEmpleado", script, true);
                return;
            }

            // Validar campos vacíos
            if (string.IsNullOrWhiteSpace(txtHoraInicio.Text))
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertInicio",
                    "Swal.fire({ icon: 'warning', title: 'Campo obligatorio', text: 'La hora de inicio es obligatoria.' });",
                    true);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtHoraFin.Text))
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



            DateTime dia = DateTime.Parse(txtDia.Text.Trim());

            // Validar duplicado
            bool existe = db.tPermisoHora.Any(p => p.Dia == dia);
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
            TimeSpan horaInicio = TimeSpan.Parse(txtHoraInicio.Text.Trim());
            TimeSpan horaFin = TimeSpan.Parse(txtHoraFin.Text.Trim());
            

            // Guardar ✅ YA USANDO EL USUARIO DE LA SESIÓN
            pue.IdUsuario = UsuarioSesion;
            pue.IdJefe = Convert.ToInt32(ddlNombreJefe.SelectedValue);
            pue.CorreoJefe = txtEmail.Text.Trim();
            pue.Motivo = ddlMotivo.Text.Trim();
            pue.TipoPermiso = txtTipoPermiso.Text.Trim();
            pue.HoraInicio = horaInicio;
            pue.HoraFin = horaFin;
            pue.Horas =Convert.ToDecimal(txtHoras.Text.Trim());
            pue.Dia = dia;
            pue.Estatus = 1;

            db.tPermisoHora.InsertOnSubmit(pue);
            db.SubmitChanges();

         

            try
            {
                string empleado = txtNombreEmpleado.Text.Trim();
                string correoJefe = txtEmail.Text.Trim();
                string correoEmpleado = SesionState.usuario.Email;
                string motivo = ddlMotivo.SelectedItem.Text;
                string tipo = txtTipoPermiso.Text.Trim();

                // 👉 OBTENER CONFIGURACIÓN DESDE BD (igual que en la otra clase)
                var configService = new PermisoDias();
                var cfg = configService.ObtenerConfig();

                if (cfg == null)
                    throw new Exception("No existe configuración de correo.");

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
                            <td style='padding: 8px; border: 1px solid #ccc;'>{dia}</td>
                        </tr>
                        <tr>
                            <td style='padding: 8px; border: 1px solid #ccc;'>Hora inicio</td>
                            <td style='padding: 8px; border: 1px solid #ccc;'>{horaInicio}</td>
                        </tr>
                        <tr>
                            <td style='padding: 8px; border: 1px solid #ccc;'>Hora fin</td>
                            <td style='padding: 8px; border: 1px solid #ccc;'>{horaFin}</td>
                        </tr>
                        <tr>
                            <td style='padding: 8px; border: 1px solid #ccc;'>Motivo</td>
                            <td style='padding: 8px; border: 1px solid #ccc;'>{motivo}</td>
                        </tr>
                        <tr>
                            <td style='padding: 8px; border: 1px solid #ccc;'>Tipo de permiso</td>
                            <td style='padding: 8px; border: 1px solid #ccc;'>{tipo}</td>
                        </tr>
                    </table>

                    <p style='margin-top:20px;'>Favor de dar seguimiento.</p>

                    <p style='font-size:13px; color:#777;'>(Correo generado automáticamente)</p>
                </div>";

                // 👉 ARMAR CORREO
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(cfg.CorreoEmisor, "Sistema de Permisos GRUPO ANKAHL");
                mail.To.Add(correoJefe);

                if (!string.IsNullOrWhiteSpace(correoEmpleado))
                    mail.CC.Add(correoEmpleado);

                mail.Subject = "Solicitud de Permiso por horas GRUPO ANKHAL";
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



            limpiar();

            string successScript = @"
        Swal.fire({
            icon: 'success',
            title: 'Guardado',
            text: 'El permiso se registró correctamente.',
            showConfirmButton: false,
            timer: 2000
        });
    ";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "alertSuccess", successScript, true);
        }

        public void limpiar()
        {
            
            ddlNombreJefe.SelectedIndex = 0;
            txtEmail.Text = "";
            ddlMotivo.SelectedIndex = 0;
            txtHoraInicio.Text = "";
            txtHoraFin.Text = "";
            txtHoras.Text = "";
            txtDia.Text = "";
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

        protected void CalcularHoras(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtHoraInicio.Text) ||
                string.IsNullOrWhiteSpace(txtHoraFin.Text))
            {
                txtHoras.Text = "";
                return;
            }

            TimeSpan horaInicio, horaFin;

            if (!TimeSpan.TryParse(txtHoraInicio.Text, out horaInicio) ||
                !TimeSpan.TryParse(txtHoraFin.Text, out horaFin))
            {
                txtHoras.Text = "";
                return;
            }

            // Validación: Hora fin no puede ser menor
            if (horaFin < horaInicio)
            {
                txtHoras.Text = "";
                ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(),
                    "alert('La hora fin no puede ser menor a la hora inicio.');", true);
                return;
            }

            // Diferencia en horas decimales
            double horas = (horaFin - horaInicio).TotalHours;

            txtHoras.Text = horas.ToString("0.##");
        }

    }
}
