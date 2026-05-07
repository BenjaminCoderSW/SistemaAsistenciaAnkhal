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
                             join p in db.tPlanta on u.IdPlanta equals p.IdPlanta into pj
                             from p in pj.DefaultIfEmpty()
                             where v.EstatusJefe != null && v.FechaResolucionRH == null
                             orderby v.EstatusJefe, v.IdVacaciones
                             select new
                             {
                                 v.IdVacaciones,
                                 v.IdUsuario,
                                 Empleado = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,
                                 Planta = p != null ? p.Planta : "N/A",
                                 Jefe = j.Jefe,
                                 v.FechaInicio,
                                 v.FechaFin,
                                 v.Dias,
                                 v.FechaSolicitud,
                                 DecisionJefe = v.EstatusJefe == 1 ? "✔ Aprobada" : "✘ Rechazada",
                                 AprobadoPorJefe = v.IdAprobadorJefe != null
                                     ? db.tUsuario.Where(ap => ap.IdUsuario == v.IdAprobadorJefe)
                                                  .Select(ap => ap.Nombre + " " + ap.ApellidoPaterno)
                                                  .FirstOrDefault()
                                     : "N/A",
                                 MotivoJefe = v.MotivoJefe ?? ""
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
                    TimeZoneInfo zonaRH = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");
                    DateTime ahoraRH = TimeZoneInfo.ConvertTime(DateTime.Now, zonaRH);

                    // 1. Cambiar estatus a Autorizado y registrar resolución RH
                    vacacion.Estatus = 2;
                    vacacion.FechaResolucionRH = ahoraRH;
                    vacacion.IdAprobadorRH = SesionState.usuario.IdUsuario;

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

                var registroExistente = db.tAsistencia.FirstOrDefault(a =>
                    a.IdUsuario == vacacion.IdUsuario &&
                    a.Fecha == fecha);

                if (registroExistente == null)
                {
                    // No existe: insertar nuevo registro de vacaciones
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
                else if (registroExistente.EstatusEntrada == "Falta")
                {
                    // Ya existe como Falta (timer nocturno): actualizar retroactivamente a Vacaciones
                    registroExistente.IdAsignarHorario       = null;
                    registroExistente.IdVacaciones           = vacacion.IdVacaciones;
                    registroExistente.EstatusEntrada         = "Vacaciones";
                    registroExistente.EstatusSalida          = "Vacaciones";
                    registroExistente.HorasTrabajadas        = TimeSpan.Zero;
                    registroExistente.HorasTrabajadasDecimal = 0;
                    registroExistente.latitud                = 20;
                    registroExistente.latitudSalida          = 20;
                    registroExistente.longitud               = -99;
                    registroExistente.longitudSalida         = -99;
                    registroExistente.DiaSalidaVacaciones    = fechaInicio;
                    registroExistente.DiaEntradaVacaciones   = fechaFin;
                    registroExistente.DiasVacaciones         = diasTotales;
                    registroExistente.HorasExtras            = null;
                    registroExistente.EstatusHorasExtras     = null;
                }
                // Si ya existe con otro estatus (permiso, comisión, checado real, etc.) no se toca
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

        protected void btnRechazar_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int id = Convert.ToInt32(btn.CommandArgument);

            var vacacion = db.tVacaciones.FirstOrDefault(v => v.IdVacaciones == id);
            if (vacacion != null)
            {
                try
                {
                    TimeZoneInfo zona = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");
                    vacacion.Estatus = 3;
                    vacacion.FechaResolucionRH = TimeZoneInfo.ConvertTime(DateTime.Now, zona);
                    vacacion.IdAprobadorRH = SesionState.usuario.IdUsuario;
                    db.SubmitChanges();

                    if (vacacion.IdUsuario.HasValue)
                        EnviarCorreoRechazoRH(vacacion.IdUsuario.Value, vacacion);

                    CargarVacaciones();

                    string script = @"
                        Swal.fire({
                            icon: 'success',
                            title: 'Rechazada',
                            text: 'La solicitud fue rechazada y se notificó al empleado.',
                            showConfirmButton: false,
                            timer: 2500
                        });";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "alertRechazar", script, true);
                }
                catch (Exception ex)
                {
                    MostrarAlerta("error", "Error", ex.Message);
                }
            }
        }

        private void EnviarCorreoRechazoRH(int idUsuario, tVacaciones vacacion)
        {
            try
            {
                var usuario = db.tUsuario.FirstOrDefault(u => u.IdUsuario == idUsuario);
                if (usuario == null || string.IsNullOrEmpty(usuario.Email)) return;

                var cfg = ObtenerConfig();
                if (cfg == null) return;

                string nombreEmpleado = usuario.Nombre + " " + usuario.ApellidoPaterno + " " + usuario.ApellidoMaterno;

                string cuerpoHtml = $@"
<div style='font-family:Arial;font-size:15px;'>
  <h2 style='color:#dc3545;'>Solicitud de Vacaciones Rechazada — GRUPO ANKHAL</h2>
  <p>Hola <strong>{nombreEmpleado}</strong>,</p>
  <p>Lamentamos informarte que tu solicitud de vacaciones ha sido <strong style='color:#dc3545;'>rechazada</strong> por Recursos Humanos.</p>
  <br/>
  <p><strong>Fecha inicio solicitada:</strong> {vacacion.FechaInicio:dd/MM/yyyy}</p>
  <p><strong>Fecha fin solicitada:</strong> {vacacion.FechaFin:dd/MM/yyyy}</p>
  <p><strong>Días solicitados:</strong> {vacacion.Dias}</p>
  <br/>
  <p>Para mayor información, comunícate con el Departamento de Recursos Humanos.</p>
  <p>Atentamente,<br/>Departamento de Recursos Humanos<br/><strong>GRUPO ANKHAL</strong></p>
</div>";

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(cfg.CorreoEmisor, "Recursos Humanos GRUPO ANKHAL");
                mail.To.Add(usuario.Email);
                mail.Subject = "Solicitud de Vacaciones Rechazada — GRUPO ANKHAL";
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

        private void CargarVacacionesFiltro(string filtro = "")
        {
            var query = from v in db.tVacaciones
                        join u in db.tUsuario on v.IdUsuario equals u.IdUsuario
                        join j in db.tJefe on v.IdJefe equals j.IdJefe
                        join p in db.tPlanta on u.IdPlanta equals p.IdPlanta into pj
                        from p in pj.DefaultIfEmpty()
                        where (v.EstatusJefe == 1 && v.Estatus == 1)
                           || (v.EstatusJefe == 2 && v.Estatus == 3)
                        select new
                        {
                            v.IdVacaciones,
                            Empleado = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,
                            Planta = p != null ? p.Planta : "N/A",
                            Jefe = j.Jefe,
                            v.FechaInicio,
                            v.FechaFin,
                            v.Dias,
                            v.FechaSolicitud,
                            DecisionJefe = v.EstatusJefe == 1 ? "✔ Aprobada" : "✘ Rechazada",
                            AprobadoPorJefe = v.IdAprobadorJefe != null
                                ? db.tUsuario.Where(ap => ap.IdUsuario == v.IdAprobadorJefe)
                                             .Select(ap => ap.Nombre + " " + ap.ApellidoPaterno)
                                             .FirstOrDefault()
                                : "N/A",
                            MotivoJefe = v.MotivoJefe ?? ""
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