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
    public partial class AprobarComisionDias : System.Web.UI.Page
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
                CargarComisionDias();
            }
        }



        private void CargarComisionDias()
        {
            var usuario = from m in db.tComisionDia
                          join r in db.tUsuario on m.IdUsuario equals r.IdUsuario
                          join p in db.tJefe on m.IdJefe equals p.IdJefe
                          where m.Estatus == 1
                          orderby m.IdComisionDia
                          select new
                          {
                              m.IdComisionDia,
                              m.IdUsuario,
                              m.IdJefe,
                              Empleado = r.Nombre + " " + r.ApellidoPaterno + " " + r.ApellidoMaterno,
                              Jefe = p.Jefe,
                              m.CorreoJefe,
                              m.Motivo,
                              m.Destino,
                              m.FechaSalida,
                              m.FechaRegreso,
                              m.Dias,
                              m.Hospedaje,
                              m.Transporte,
                              m.Observaciones,

                              // ✅ Texto limpio para el Grid
                              EstatusTexto =
                                  m.Estatus == 1 ? "Pendiente" :
                                  m.Estatus == 2 ? "Autorizado" :
                                  "Desconocido"
                          };

            dvgComisionDias.DataSource = usuario.ToList();
            dvgComisionDias.DataBind();
        }





        protected void btnAutorizar_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int id = Convert.ToInt32(btn.CommandArgument);

            var pue = db.tComisionDia.FirstOrDefault(t => t.IdComisionDia == id);
            if (pue != null)
            {
                // Cambiar el estatus a 0 en lugar de eliminar
                pue.Estatus = 2;

                db.SubmitChanges();
                CargarComisionDias();

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
                text: 'La comisión por dias se autorizo correctamente.',
                showConfirmButton: false,
                timer: 2000
            });
        ";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertDesactivar", script, true);
            }
        }

        private void EnviarCorreoAutorizacion(int idUsuario, tComisionDia permiso)
        {
            try
            {
                // ✅ Buscar datos del usuario
                var usuario = db.tUsuario.FirstOrDefault(u => u.IdUsuario == idUsuario);

                if (usuario == null || string.IsNullOrEmpty(usuario.Email))
                    return; // No hay correo para enviar

                string correoDestino = usuario.Email;
                string nombreEmpleado = usuario.Nombre + " " + usuario.ApellidoPaterno + " " + usuario.ApellidoMaterno;

                string asunto = "Permiso por dias autorizado";

                string cuerpo = $@"
            <h2>Solicitud Autorizada</h2>

            <p>Hola <strong>{nombreEmpleado}</strong>,</p>

            <p>Tu solicitud de permiso por horas ha sido <strong>autorizada</strong>.</p>

            <p><strong>Día:</strong> {permiso.Dias}</p>
            <p><strong>Hora inicio:</strong> {permiso.FechaSalida}</p>
            <p><strong>Hora fin:</strong> {permiso.FechaRegreso}</p>
            <p><strong>Motivo:</strong> {permiso.Motivo}</p>
            <p><strong>Destino:</strong> {permiso.Destino}</p>

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

            var pue = db.tComisionDia.FirstOrDefault(t => t.IdComisionDia == id);
            if (pue != null)
            {
                // ✅ Eliminar registro real
                db.tComisionDia.DeleteOnSubmit(pue);
                db.SubmitChanges();

                CargarComisionDias();

                string script = @"
            Swal.fire({
                icon: 'success',
                title: 'Eliminado',
                text: 'El permiso por hora se eliminó correctamente.',
                showConfirmButton: false,
                timer: 2000
            });
        ";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertEliminar", script, true);
            }
        }


        private void CargarComisionDia(string filtro = "")
        {
            var query = from t in db.tComisionDia
                        join u in db.tUsuario on t.IdUsuario equals u.IdUsuario
                        where t.Estatus == 1
                        select new
                        {
                            t.IdComisionDia,
                            t.Dias,
                            t.FechaSalida,
                            t.FechaRegreso,
                            t.Motivo,
                            t.Destino,
                            nombrecompleto = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,   // Nombre del empleado
                            t.Hospedaje,
                            t.Transporte,
                            t.Observaciones
                        };

            if (!string.IsNullOrEmpty(filtro))
            {
                query = query.Where(x =>
                    System.Data.Linq.SqlClient.SqlMethods.Like(x.Motivo, "%" + filtro + "%") ||
                    System.Data.Linq.SqlClient.SqlMethods.Like(x.nombrecompleto, "%" + filtro + "%")
                );
            }

            dvgComisionDias.DataSource = query.ToList();
            dvgComisionDias.DataBind();
        }

        protected void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            CargarComisionDia(txtBuscar.Text.Trim());
        }

        protected void dvgComisionDias_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dvgComisionDias.PageIndex = e.NewPageIndex;
            CargarComisionDia(txtBuscar.Text.Trim()); // mantiene el filtro
        }
    }
}