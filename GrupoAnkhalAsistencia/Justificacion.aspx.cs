using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class Justificacion : System.Web.UI.Page
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
                return;
            }
            if (!IsPostBack)
            {
                
            }
        }

        private void CargarJustificaion()
        {
            DateTime fechaInicio, fechaFin;
            bool tieneInicio = DateTime.TryParse(txtFechaInicio.Text, out fechaInicio);
            bool tieneFin = DateTime.TryParse(txtFechaFin.Text, out fechaFin);


            int idUsuario = UsuarioSesion;

          
            var query = db.vJustificacion
                .Where(m => m.idusuario == idUsuario)
                 .Where(m => m.EstatusEntrada == "Retardo");

            // Filtro por fechas si las ingresaron
            if (tieneInicio)
                query = query.Where(m => m.Fecha >= fechaInicio);

            if (tieneFin)
                query = query.Where(m => m.Fecha <= fechaFin);

            var usuario = query
                .OrderBy(m => m.IdAsistencia)
                .Select(m => new
                {
                    m.IdAsistencia,
                    m.NombreCompleto,
                    m.Planta,
                    m.Horario,
                    m.IdJustificacion,
                    m.Justificacion,
                    m.Fecha,
                    m.HoraEntrada,
                    m.HoraSalida,
                    m.EstatusEntrada,
                    m.EstatusSalida,

                    EstatusTexto = m.Justificacion == "Aceptada" ? "Aceptada" : "Rechazada"
                })
                .ToList();

            dvgJustificaionHoras.DataSource = usuario;
            dvgJustificaionHoras.DataBind();
        }





   
        private void EnviarCorreoAutorizacion(int idUsuario, tPermisoHora permiso)
        {
            try
            {
                // ✅ Buscar datos del usuario
                var usuario = db.tUsuario.FirstOrDefault(u => u.IdUsuario == UsuarioSesion);

                if (usuario == null || string.IsNullOrEmpty(usuario.Email))
                    return; // No hay correo para enviar

                string correoDestino = usuario.Email;
                string nombreEmpleado = usuario.Nombre + " " + usuario.ApellidoPaterno + " " + usuario.ApellidoMaterno;

                string asunto = "Justificaion autorizada";

                string cuerpo = $@"
            <h2>Justificación Autorizada</h2>

            <p>Hola <strong>{nombreEmpleado}</strong>,</p>

            <p>Tu justificaión  ha sido <strong>autorizada</strong>.</p>

            <br/>
            <p>Atentamente,<br>Departamento de Recursos Humanos</p>
        ";

                System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
                msg.To.Add(correoDestino);
                msg.From = new System.Net.Mail.MailAddress("rh@GRUPOANKHAL.somee.com"); // ✅ CAMBIA
                msg.Subject = asunto;
                msg.Body = cuerpo;
                msg.IsBodyHtml = true;

                System.Net.Mail.SmtpClient cliente = new System.Net.Mail.SmtpClient("smtp.GRUPOANKHAL.somee.com"); // ✅ CAMBIA
                cliente.Port = 25;
                cliente.Credentials = new System.Net.NetworkCredential("rh@GRUPOANKHAL.somee.com", "RGrupoAnkhal2025#"); // ✅ CAMBIA
                cliente.EnableSsl = true;

                cliente.Send(msg);
            }
            catch (Exception ex)
            {
                // Puedes guardar un log si deseas
            }
        }

        protected void btnBuscarFechas_Click(object sender, EventArgs e)
        {
            CargarJustificaion();
        }

        protected void dvgJustificaion_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dvgJustificaionHoras.PageIndex = e.NewPageIndex;
            CargarJustificaion();
        }

        protected void btnJustificar_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int idAsistencia = Convert.ToInt32(btn.CommandArgument);

            // Guardamos el ID en un HiddenField
            hfIdAsistencia.Value = idAsistencia.ToString();

            // Limpiar campos del modal
            ddlMotivo.Text = "";
            txtComentarios.Text = "";

            // Mostrar modal
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowModal", "abrirModalJustificar();", true);
        }


        protected void btnGuardarJustificacion_Click(object sender, EventArgs e)
        {   

            tJustificacion pue = new tJustificacion();
            {
                if (string.IsNullOrWhiteSpace(ddlMotivo.Text))
                {
                    string script = "Swal.fire({ icon: 'warning', title: 'Campo obligatorio', text: 'El campo motivo es obligatorio.' });";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "alertEdificio", script, true);
                    return;
                }

                pue.IdAsistencia =Convert.ToInt32(hfIdAsistencia.Value);
                pue.IdUsuario = UsuarioSesion;
                pue.Fecha = DateTime.Now;
                pue.Motivo = ddlMotivo.Text;
                pue.Observaciones = txtComentarios.Text;
                pue.Estatus = 1; // Pendiente

            }
            ;
            db.tJustificacion.InsertOnSubmit(pue);
            db.SubmitChanges();
            // Cerrar modal
            ScriptManager.RegisterStartupScript(this, GetType(), "HideModal", "cerrarModalJustificar();", true);

            // Recargar tabla
            CargarJustificaion();

            // Mostrar notificación de éxito
            string successScript = @"
                                    Swal.fire({
                                        icon: 'success',
                                        title: 'Guardado',
                                        text: 'La justificación  se guardó correctamente.',
                                        showConfirmButton: false,
                                        timer: 2000
                                    });
                                ";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "alertSuccess", successScript, true);

        }
    }

    }
