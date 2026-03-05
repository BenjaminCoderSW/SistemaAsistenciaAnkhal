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
                // VALIDAR DÍAS DISPONIBLES ANTES DE AUTORIZAR
                var usuario = db.tUsuario.FirstOrDefault(u => u.IdUsuario == vacacion.IdUsuario);

                if (usuario == null)
                {
                    MostrarAlerta("error", "Error", "No se encontró el usuario.");
                    return;
                }

                int diasDisponibles = usuario.DiasVacacionesDisponibles ?? 0;
                int diasSolicitados = vacacion.Dias ?? 0;

                if (diasSolicitados > diasDisponibles)
                {
                    MostrarAlerta("error", "Días insuficientes",
                        $"El empleado no tiene suficientes días. Tiene {diasDisponibles} disponibles y solicita {diasSolicitados}.");
                    return;
                }

                try
                {
                    // 1. Cambiar estatus a Autorizado
                    vacacion.Estatus = 2;

                    // 2. DESCONTAR DÍAS DEL USUARIO
                    usuario.DiasVacacionesDisponibles = diasDisponibles - diasSolicitados;

                    // 3. Guardar cambios
                    db.SubmitChanges();

                    // 4. Registrar en tAsistencia los días de vacaciones
                    RegistrarVacacionesEnAsistencia(vacacion);

                    CargarVacaciones();

                    // 5. Enviar correo de autorización
                    if (vacacion.IdUsuario.HasValue)
                    {
                        EnviarCorreoAutorizacion(vacacion.IdUsuario.Value, vacacion);
                    }

                    string script = $@"
                Swal.fire({{
                    icon: 'success',
                    title: 'Autorizado',
                    html: 'Las vacaciones fueron autorizadas.<br>Días descontados: <strong>{diasSolicitados}</strong><br>Días restantes: <strong>{usuario.DiasVacacionesDisponibles}</strong>',
                    showConfirmButton: true,
                    timer: 3500
                }});";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "alertAutorizar", script, true);
                }
                catch (Exception ex)
                {
                    MostrarAlerta("error", "Error", "No se pudo autorizar: " + ex.Message);
                }
            }
        }

        // Registrar cada día de vacaciones en tabla tAsistencia en la BD
        private void RegistrarVacacionesEnAsistencia(tVacaciones vacacion)
        {
            if (!vacacion.FechaInicio.HasValue || !vacacion.FechaFin.HasValue)
                return;

            DateTime fechaInicio = vacacion.FechaInicio.Value;
            DateTime fechaFin = vacacion.FechaFin.Value;
            int diasTotales = vacacion.Dias ?? 0;

            // Obtener planta del usuario
            var usuario = db.tUsuario.FirstOrDefault(u => u.IdUsuario == vacacion.IdUsuario);
            int idPlanta = usuario?.IdPlanta ?? 1;

            // Recorrer cada día del rango de vacaciones
            for (DateTime fecha = fechaInicio; fecha <= fechaFin; fecha = fecha.AddDays(1))
            {
                // Saltar domingos — todos descansan
                if (fecha.DayOfWeek == DayOfWeek.Sunday)
                    continue;

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
                        IdVacaciones = vacacion.IdVacaciones,
                        DiaSalidaVacaciones = fechaInicio,
                        DiaEntradaVacaciones = fechaFin,
                        DiasVacaciones = diasTotales,
                        latitud = 20,
                        latitudSalida = 20,
                        longitud = -99,
                        longitudSalida = -99,
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
                int diasRestantes = usuario.DiasVacacionesDisponibles ?? 0;

                string cuerpoHtml = $@"
                    <div style='font-family: Arial; font-size: 15px;'>
                        <h2 style='color:#28a745;'>Solicitud Autorizada</h2>
                        <p>Hola <strong>{nombreEmpleado}</strong>,</p>
                        <p>Tu solicitud de vacaciones ha sido <strong>autorizada</strong>.</p>
                        <p><strong>Fecha inicio:</strong> {vacacion.FechaInicio:dd/MM/yyyy}</p>
                        <p><strong>Fecha fin:</strong> {vacacion.FechaFin:dd/MM/yyyy}</p>
                        <p><strong>Días autorizados:</strong> {vacacion.Dias}</p>
                        <hr>
                        <p><strong>Días restantes de vacaciones:</strong> <span style='color:#003366; font-size:18px;'>{diasRestantes}</span></p>
                        <br/>
                        <p>Atentamente,<br>Departamento de Recursos Humanos<br>GRUPO ANKHAL</p>
                    </div>";

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(cfg.CorreoEmisor, "Recursos Humanos GRUPO ANKHAL");
                mail.To.Add(correoDestino);
                mail.Subject = "Vacaciones Autorizadas - GRUPO ANKHAL";
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

        private void MostrarAlerta(string icono, string titulo, string mensaje)
        {
            string script = $@"
                Swal.fire({{
                    icon: '{icono}',
                    title: '{titulo}',
                    text: '{mensaje}',
                    showConfirmButton: true
                }});";
            ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), script, true);
        }
    }
}