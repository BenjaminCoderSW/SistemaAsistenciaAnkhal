using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class AceptarJustificacion : System.Web.UI.Page
    {
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
         ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            // ¿Sesion válida?
            if (SesionState.usuario == null)
            {
                SesionState.usuario = null;
                Response.Redirect("login.aspx");
                return;
            }


            string rolUsuario = SesionState.usuario.tRol.Rol;  // ajusta al nombre que tengas en tu clase

            // Aquí pones los roles que SI pueden entrar
            string[] rolesPermitidos = { "Administrador", "Rh" };

            if (!rolesPermitidos.Contains(rolUsuario))
            {
                // Si NO tiene rol válido → lo sacamos
                Response.Redirect("login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                CargarJustificaion();

            }
        }

        private void CargarJustificaion()
        {
            var usuario = from m in db.v_aceptarJustificaion
                          where m.Estatus == "Pendiente"
                          orderby m.fechaJustificaion descending
                          select new
                          {
                              m.IdJustificacion,
                              m.IdAsistencia,
                              m.nombreCompleto,
                              m.fechaAsistencia,
                              m.fechaJustificaion,
                              m.Motivo,
                              m.Observaciones,
                              m.Estatus,
                              m.HoraInicio
                          };

            dvgJustificaion.DataSource = usuario.ToList();
            dvgJustificaion.DataBind();
        }





        protected void btnAutorizar_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            // Recibe: "IdJustificacion|IdAsistencia"
            string[] valores = btn.CommandArgument.Split('|');

            int idJustificacion = Convert.ToInt32(valores[0]);
            int idAsistencia = Convert.ToInt32(valores[1]);
            DateTime horaInicio = Convert.ToDateTime(valores[2]);

            var pue = db.tJustificacion.FirstOrDefault(t => t.IdJustificacion == idJustificacion);
            if (pue != null)
            {
                // Cambiar el estatus a 0 en lugar de eliminar
                pue.Estatus = 2;

                db.SubmitChanges();

                // 🔥 Actualizar la tabla tAsistencia con el ID que recibiste
                var asistencia = db.tAsistencia.FirstOrDefault(a => a.IdAsistencia == idAsistencia);
                if (asistencia != null)
                {
                    asistencia.IdJustificacion = idJustificacion;
                    asistencia.EstatusEntrada = "A tiempo";
                    asistencia.HoraEntrada = horaInicio.TimeOfDay;
                    asistencia.Justificacion = "Aceptada"; // o el campo que quieras modificar
                    db.SubmitChanges();
                }


                CargarJustificaion();

                if (pue.IdUsuario.HasValue)
                {
                    EnviarCorreoAutorizacion(pue.IdUsuario.Value, pue);
                }
                else
                {

                }


                // Mostrar mensaje de éxito
                string script = @"
            Swal.fire({
                icon: 'success',
                title: 'Autorizado',
                text: 'La justificación se autorizo correctamente.',
                showConfirmButton: false,
                timer: 2000
            });
        ";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertDesactivar", script, true);
            }
        }

        private void EnviarCorreoAutorizacion(int idUsuario, tJustificacion permiso)
        {
            try
            {
                // ✅ Buscar datos del usuario
                var usuario = db.tUsuario.FirstOrDefault(u => u.IdUsuario == idUsuario);

                if (usuario == null || string.IsNullOrEmpty(usuario.Email))
                    return; // No hay correo para enviar

                string correoDestino = usuario.Email;
                string nombreEmpleado = usuario.Nombre + " " + usuario.ApellidoPaterno + " " + usuario.ApellidoMaterno;

                string asunto = "Justificación autorizada";

                string cuerpo = $@"
            <h2>Solicitud Autorizada</h2>

            <p>Hola <strong>{nombreEmpleado}</strong>,</p>

            <p>Tu justificaión ha sido <strong>autorizada</strong>.</p>


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





        protected void btnEliminar_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int id = Convert.ToInt32(btn.CommandArgument);

            var pue = db.tJustificacion.FirstOrDefault(t => t.IdJustificacion == id);
            if (pue != null)
            {
                // ✅ Eliminar registro real
                db.tJustificacion.DeleteOnSubmit(pue);
                db.SubmitChanges();

                CargarJustificaion();

                string script = @"
            Swal.fire({
                icon: 'success',
                title: 'Eliminado',
                text: 'La justificaión se eliminó correctamente.',
                showConfirmButton: false,
                timer: 2000
            });
        ";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertEliminar", script, true);
            }
        }


        private void CargarJustificiaonF(string filtro = "")
        {
            var query = from t in db.v_aceptarJustificaion
                        where t.Estatus == "Pendiente"
                        select new
                        {
                            t.IdJustificacion,
                            t.IdAsistencia,
                            t.nombreCompleto,
                            t.fechaAsistencia,
                            t.fechaJustificaion,
                            t.Motivo,
                            t.Observaciones,
                            t.Estatus,
                            t.HoraInicio
                        };

          
            if (!string.IsNullOrEmpty(filtro))
            {
                query = query.Where(x =>
                    System.Data.Linq.SqlClient.SqlMethods.Like(x.nombreCompleto, "%" + filtro + "%")
                );
            }

            dvgJustificaion.DataSource = query.ToList();
            dvgJustificaion.DataBind();
        }

        protected void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            CargarJustificiaonF(txtBuscar.Text.Trim());
        }

        protected void dvgJustificaion_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dvgJustificaion.PageIndex = e.NewPageIndex;
            CargarJustificiaonF(txtBuscar.Text.Trim());
        }
    }
}