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
    public partial class AprobarVacaciones : System.Web.UI.Page
    {
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
            ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        public ConfigCorreo ObtenerConfig()
        {
            return db.ConfigCorreo.FirstOrDefault();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (SesionState.usuario == null)
            {
                Response.Redirect("login.aspx");
                return;
            }

            string rolUsuario = SesionState.usuario.tRol.Rol;
            string[] rolesPermitidos = { "Administrador", "Rh" };

            if (!rolesPermitidos.Contains(rolUsuario))
            {
                Response.Redirect("login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                CargarVacaciones();
            }
        }

        private void CargarVacaciones()
        {
            var vacaciones = from v in db.tVacaciones
                             join u in db.tUsuario on v.IdUsuario equals u.IdUsuario
                             join j in db.tJefe on v.IdJefe equals j.IdJefe
                             where v.Estatus == 1
                             orderby v.IdVacaciones
                             select new
                             {
                                 v.IdVacaciones,
                                 v.IdUsuario,
                                 Empleado = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,
                                 Jefe = j.Jefe,
                                 v.CorreoJefe,
                                 v.FechaInicio,
                                 v.FechaFin,
                                 v.Dias,
                                 EstatusTexto = v.Estatus == 1 ? "Pendiente" :
                                                v.Estatus == 2 ? "Autorizado" :
                                                "Desconocido"
                             };

            dvgVacaciones.DataSource = vacaciones.ToList();
            dvgVacaciones.DataBind();
        }

        protected void btnAutorizar_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int id = Convert.ToInt32(btn.CommandArgument);

            var vacacion = db.tVacaciones.FirstOrDefault(v => v.IdVacaciones == id);
            if (vacacion != null)
            {
                // 1. Cambiar estatus a Autorizado
                vacacion.Estatus = 2;
                db.SubmitChanges();

                // 2. 🔥 NUEVO: Registrar en tAsistencia los días de vacaciones
                RegistrarVacacionesEnAsistencia(vacacion);

                CargarVacaciones();

                if (vacacion.IdUsuario.HasValue)
                {
                    EnviarCorreoAutorizacion(vacacion.IdUsuario.Value, vacacion);
                }

                string script = @"
            Swal.fire({
                icon: 'success',
                title: 'Autorizado',
                text: 'Las vacaciones se autorizaron y registraron en asistencia.',
                showConfirmButton: false,
                timer: 2000
            });";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertAutorizar", script, true);
            }
        }

        // Registrar cada día de vacaciones en tabla tAsistencia en la BD
        private void RegistrarVacacionesEnAsistencia(tVacaciones vacacion)
        {
            if (!vacacion.FechaInicio.HasValue || !vacacion.FechaFin.HasValue)
                return;

            DateTime fechaInicio = vacacion.FechaInicio.Value;
            DateTime fechaFin = vacacion.FechaFin.Value;
            int diasTotales = vacacion.Dias ?? 0; // Total de días de vacaciones

            // Obtener planta del usuario (puedes ajustar la lógica)
            int idPlanta = 1; // Por defecto planta 1, o buscar la del usuario

            // Recorrer cada día del rango de vacaciones
            for (DateTime fecha = fechaInicio; fecha <= fechaFin; fecha = fecha.AddDays(1))
            {
                // Verificar que no exista ya un registro ese día
                bool existe = db.tAsistencia.Any(a =>
                    a.IdUsuario == vacacion.IdUsuario &&
                    a.Fecha == fecha);

                if (!existe)
                {
                    // Crear registro de asistencia marcado como "Vacaciones"
                    tAsistencia registro = new tAsistencia
                    {
                        IdUsuario = vacacion.IdUsuario,
                        IdPlanta = idPlanta,
                        Fecha = fecha,
                        IdVacaciones = vacacion.IdVacaciones, // Relacionar con vacaciones
                        DiaSalidaVacaciones = fechaInicio, // Fecha de inicio
                        DiaEntradaVacaciones = fechaFin, // Fecha de fin
                        DiasVacaciones = diasTotales, // Total de días
                        latitud = 20, // Valor fijo
                        latitudSalida = 20, // Valor fijo
                        longitud = -99, // Valor fijo
                        longitudSalida = -99, // Valor fijo
                        EstatusEntrada = "Vacaciones",
                        EstatusSalida = "Vacaciones",
                        HorasTrabajadas = TimeSpan.Zero,
                        HorasTrabajadasDecimal = 0
                    };

                    db.tAsistencia.InsertOnSubmit(registro);
                }
            }

            db.SubmitChanges();
        }

        private void EnviarCorreoAutorizacion(int idUsuario, tVacaciones vacacion)
        {
            try
            {
                var usuario = db.tUsuario.FirstOrDefault(u => u.IdUsuario == idUsuario);
                if (usuario == null || string.IsNullOrEmpty(usuario.Email))
                    return;

                var cfg = ObtenerConfig();
                if (cfg == null)
                    return;

                string correoDestino = usuario.Email;
                string nombreEmpleado = usuario.Nombre + " " + usuario.ApellidoPaterno + " " + usuario.ApellidoMaterno;

                string cuerpoHtml = $@"
                    <h2>Solicitud Autorizada</h2>
                    <p>Hola <strong>{nombreEmpleado}</strong>,</p>
                    <p>Tu solicitud de vacaciones ha sido <strong>autorizada</strong>.</p>
                    <p><strong>Fecha inicio:</strong> {vacacion.FechaInicio:dd/MM/yyyy}</p>
                    <p><strong>Fecha fin:</strong> {vacacion.FechaFin:dd/MM/yyyy}</p>
                    <p><strong>Días:</strong> {vacacion.Dias}</p>
                    <br/>
                    <p>Atentamente,<br>Departamento de Recursos Humanos</p>";

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(cfg.CorreoEmisor);
                mail.To.Add(correoDestino);
                mail.Subject = "Vacaciones Autorizadas";
                mail.Body = cuerpoHtml;
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient(cfg.SmtpHost);
                smtp.Port = cfg.Puerto;
                smtp.EnableSsl = cfg.UsaSSL;
                smtp.Credentials = new NetworkCredential(cfg.CorreoEmisor, cfg.PasswordCorreo);

                smtp.Send(mail);
            }
            catch { }
        }

        protected void btnEliminar_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int id = Convert.ToInt32(btn.CommandArgument);

            var vacacion = db.tVacaciones.FirstOrDefault(v => v.IdVacaciones == id);
            if (vacacion != null)
            {
                db.tVacaciones.DeleteOnSubmit(vacacion);
                db.SubmitChanges();

                CargarVacaciones();

                string script = @"
                    Swal.fire({
                        icon: 'success',
                        title: 'Eliminado',
                        text: 'La solicitud se eliminó correctamente.',
                        showConfirmButton: false,
                        timer: 2000
                    });";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertEliminar", script, true);
            }
        }

        private void CargarVacacionesFiltro(string filtro = "")
        {
            var query = from v in db.tVacaciones
                        join u in db.tUsuario on v.IdUsuario equals u.IdUsuario
                        join j in db.tJefe on v.IdJefe equals j.IdJefe
                        where v.Estatus == 1
                        select new
                        {
                            v.IdVacaciones,
                            Empleado = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,
                            Jefe = j.Jefe,
                            v.CorreoJefe,
                            v.FechaInicio,
                            v.FechaFin,
                            v.Dias,
                            EstatusTexto = v.Estatus == 1 ? "Pendiente" :
                                           v.Estatus == 2 ? "Autorizado" :
                                           "Desconocido"
                        };

            if (!string.IsNullOrEmpty(filtro))
            {
                query = query.Where(x =>
                    System.Data.Linq.SqlClient.SqlMethods.Like(x.Empleado, "%" + filtro + "%"));
            }

            dvgVacaciones.DataSource = query.ToList();
            dvgVacaciones.DataBind();
        }

        protected void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            CargarVacacionesFiltro(txtBuscar.Text.Trim());
        }

        protected void dvgVacaciones_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dvgVacaciones.PageIndex = e.NewPageIndex;
            CargarVacacionesFiltro(txtBuscar.Text.Trim());
        }
    }
}