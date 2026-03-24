using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Linq;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class RegistroEmpleado : System.Web.UI.Page
    {
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
           ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        public int UsuarioSesion;

        public static long IPToLong(string ip)
        {
            // ✅ CORRECCIÓN: Validar sin usar IsNullOrWhiteSpace
            if (ip == null || ip.Trim().Length == 0) return 0;

            var segmentos = ip.Split('.');
            if (segmentos.Length != 4) return 0;

            // Validar que cada segmento sea un número válido
            foreach (var segmento in segmentos)
            {
                if (!int.TryParse(segmento, out int num) || num < 0 || num > 255)
                    return 0;
            }

            return (long.Parse(segmentos[0]) << 24)
                 + (long.Parse(segmentos[1]) << 16)
                 + (long.Parse(segmentos[2]) << 8)
                 + long.Parse(segmentos[3]);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // ¿Sesion válida?
            if (SesionState.usuario == null)
            {
                SesionState.usuario = null;
                Response.Redirect("login.aspx");
                return;
            }

            // VALIDAR ROL PERMITIDO
            string rolUsuario = SesionState.usuario.tRol.Rol;
            string[] rolesPermitidos = { "Administrador", "Rh", "Empleado", "Jefe de Planta" };

            if (!rolesPermitidos.Contains(rolUsuario))
            {
                Response.Redirect("login.aspx");
                return;
            }

            // Si pasa la validación
            UsuarioSesion = SesionState.usuario.IdUsuario;
            txtEmpleado.Text = SesionState.usuario.Nombre + " " +
                               SesionState.usuario.ApellidoPaterno + " " +
                               SesionState.usuario.ApellidoMaterno;

            if (!IsPostBack)
            {
                txtFecha.Text = DateTime.Now.ToString("dd/MM/yyyy");
                txtHora.Text = DateTime.Now.ToString("HH:mm:ss");
            }
        }

        private string ObtenerIPCliente()
        {
            string ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (ip != null && ip.Trim().Length > 0)
                return ip.Split(',')[0].Trim();

            ip = Request.ServerVariables["HTTP_X_CLIENT_IP"];
            if (ip != null && ip.Trim().Length > 0)
                return ip.Trim();

            ip = Request.ServerVariables["REMOTE_ADDR"];
            if (ip != null && ip.Trim().Length > 0)
                return ip.Trim();

            // Si estamos en desarrollo local (localhost)
            if (Request.IsLocal)
                return ObtenerIPLocal();

            return "IP no disponible";
        }

        public string ObtenerIPLocal()
        {
            string localIP = "";
            try
            {
                foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }
            }
            catch
            {
                localIP = "127.0.0.1";
            }
            return localIP;
        }

        public static DateTime HoraMexico()
        {
            TimeZoneInfo zona = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");
            return TimeZoneInfo.ConvertTime(DateTime.Now, zona);
        }

        protected void btnRegistrar_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Obtener IP del dispositivo
                string ipUsuario = ObtenerIPCliente();

                // ✅ CORRECCIÓN: No usar IsNullOrWhiteSpace
                if (ipUsuario == null || ipUsuario.Trim().Length == 0 || ipUsuario == "IP no disponible")
                {
                    ipUsuario = "IP no detectada";
                }

                // Verificar si el empleado está de vacaciones aprobadas
                var hoy = DateTime.Today;
                bool enVacaciones = db.tVacaciones.Any(v =>
                    v.IdUsuario == SesionState.usuario.IdUsuario &&
                    v.Estatus == 2 &&
                    v.FechaInicio <= hoy &&
                    v.FechaFin >= hoy);

                if (enVacaciones)
                {
                    MostrarSwal("info", "Empleado en vacaciones",
                        "Este empleado se encuentra de vacaciones, no es posible registrar su asistencia.");
                    return;
                }

                int idUsuario = SesionState.usuario.IdUsuario;
                DateTime fechaHoy = DateTime.Now.Date;
                //TimeSpan horaActual = DateTime.Now.TimeOfDay;

                TimeZoneInfo zonaMexico = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");
                DateTime horaMexico = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaMexico);

                TimeSpan horaActual = horaMexico.TimeOfDay;

                // 2. Buscar registro de hoy
                var registro = db.tAsistencia.FirstOrDefault(x => x.IdUsuario == idUsuario && x.Fecha == fechaHoy);

                // 3. Verificar horario asignado
                var horario = db.v_validarhorario
                               .Where(x => x.IdUsuario == idUsuario)
                               .OrderByDescending(x => x.HoraInicio)
                               .FirstOrDefault();

                bool esDomingo = DateTime.Today.DayOfWeek == DayOfWeek.Sunday;

                if (horario == null && !esDomingo)
                {
                    MostrarSwal("warning", "Alerta", "No existe horario asignado para este empleado");
                    return;
                }

                TimeSpan horaInicioNormal = (horario != null) ? horario.HoraInicio ?? TimeSpan.Zero : TimeSpan.Zero;
                TimeSpan horaFinNormal    = (horario != null) ? horario.HoraFin ?? TimeSpan.MaxValue : TimeSpan.MaxValue;
               


                // 4. Validar latitud y longitud
                decimal latitud = 0, longitud = 0;
                string latStr = (hdLat.Value ?? "").Trim();
                string lonStr = (hdLon.Value ?? "").Trim();

                // ✅ CORRECCIÓN: No usar IsNullOrWhiteSpace
                if (latStr.Length > 0)
                {
                    latStr = latStr.Replace(',', '.');
                    if (!decimal.TryParse(latStr, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out latitud))
                    {
                        MostrarSwal("warning", "Ubicación inválida", "La latitud no tiene un formato válido.");
                        return;
                    }
                }

                if (lonStr.Length > 0)
                {
                    lonStr = lonStr.Replace(',', '.');
                    if (!decimal.TryParse(lonStr, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out longitud))
                    {
                        MostrarSwal("warning", "Ubicación inválida", "La longitud no tiene un formato válido.");
                        return;
                    }
                }

                if (latitud == 0 && longitud == 0)
                {
                    MostrarSwal("warning", "Sin ubicación", "No se pudo obtener la ubicación GPS. Intenta nuevamente.");
                    return;
                }

                // 5. Planta del perfil del empleado (el GPS ya registra ubicación física)
                int? idPlanta = SesionState.usuario.IdPlanta;

                // 6. ✅ OBTENER PERMISO - QUERY SQL DIRECTA
                tPermisoHora permisoHoy = null;

                try
                {
                    var permisosQuery = db.ExecuteQuery<tPermisoHora>(
                        @"SELECT * FROM tPermisoHora 
                          WHERE IdUsuario = {0} 
                            AND Dia = {1} 
                            AND (Estatus = 2 OR Estatus = '2')
                          ORDER BY HoraInicio",
                        idUsuario,
                        fechaHoy
                    ).ToList();

                    if (permisosQuery != null && permisosQuery.Count > 0)
                    {
                        permisoHoy = permisosQuery
                            .Where(p => p.HoraInicio != null)
                            .OrderBy(p => p.HoraInicio.Value)
                            .FirstOrDefault();
                    }
                }
                catch
                {
                    permisoHoy = null;
                }

                // 7. ENTRADA
                if (registro == null)
                {
                    int? idAsignarHorario = (horario != null) ? (int?)horario.IdAsignarHorario : null;
                    string estatusEntrada;

                    if (esDomingo && horario == null)
                    {
                        estatusEntrada = "Domingo voluntario";
                    }
                    else if (permisoHoy != null)
                    {
                        TimeSpan horaFinPermiso = permisoHoy.HoraFin ?? TimeSpan.MaxValue;
                        estatusEntrada = horaActual <= horaFinPermiso ? "A tiempo" : "Retardo";
                    }
                    else
                    {
                        estatusEntrada = horaActual > horaInicioNormal ? "Retardo" : "A tiempo";
                    }

                    string fingerprint = (hdFingerprint.Value ?? "").Trim();
                    if (fingerprint.Length == 0) fingerprint = "Web";

                    var asistencia = new tAsistencia
                    {
                        IdUsuario = idUsuario,
                        IdAsignarHorario = idAsignarHorario,
                        Fecha = fechaHoy,
                        HoraEntrada = horaActual,
                        MacEntrada = fingerprint,
                        IP = ipUsuario,
                        IdPlanta = idPlanta,
                        latitud = latitud,
                        longitud = longitud,
                        EstatusEntrada = estatusEntrada,
                        HorasExtras = 0,
                        EstatusHorasExtras = "Sin registro"
                    };

                    if (permisoHoy != null && horaActual <= (permisoHoy.HoraFin ?? TimeSpan.MaxValue))
                    {
                        asistencia.HoraSalidaPermiso = permisoHoy.HoraInicio;
                        asistencia.HoraEntradaPermiso = permisoHoy.HoraFin;
                    }

                    db.tAsistencia.InsertOnSubmit(asistencia);
                    db.SubmitChanges();

                    string nombreUsuario = SesionState.usuario.Nombre ?? "";
                    MostrarSwal("success", "Entrada",
                        $"Entrada registrada correctamente para {nombreUsuario}");
                    return;
                }

                // 8. SALIDA A COMER
                if (registro.HoraSalidaComer == null)
                {
                    registro.HoraSalidaComer = horaActual;
                    db.SubmitChanges();

                    string nombreUsuario = SesionState.usuario.Nombre ?? "";
                    MostrarSwal("success", "Salida a comer",
                        $"Salida a comer registrada para {nombreUsuario}");
                    return;
                }

                // 9. ENTRADA DE COMER
                if (registro.HoraEntradaComer == null)
                {
                    registro.HoraEntradaComer = horaActual;

                    if (registro.HoraEntradaComer.HasValue && registro.HoraSalidaComer.HasValue)
                    {
                        TimeSpan duracionComida = registro.HoraEntradaComer.Value - registro.HoraSalidaComer.Value;
                        decimal minutosComida = (decimal)duracionComida.TotalMinutes;

                        registro.HoraComida = (decimal)duracionComida.TotalHours;
                        registro.EstatusComida = minutosComida <= 60 ? "Comida a tiempo" : "Retardo Comida";
                    }

                    db.SubmitChanges();

                    string nombreUsuario = SesionState.usuario.Nombre ?? "";
                    MostrarSwal("success", "Entrada de comer",
                        $"Entrada de comer registrada para {nombreUsuario}");
                    return;
                }

                // 10. PERMISOS (salida / regreso)
                if (permisoHoy != null)
                {
                    TimeSpan horaInicioPermiso = permisoHoy.HoraInicio ?? TimeSpan.Zero;
                    TimeSpan horaFinPermiso = permisoHoy.HoraFin ?? TimeSpan.MaxValue;

                    if (!registro.HoraSalidaPermiso.HasValue &&
                        horaActual >= horaInicioPermiso && horaActual <= horaFinPermiso)
                    {
                        registro.HoraSalidaPermiso = horaActual;
                        db.SubmitChanges();

                        string nombreUsuario = SesionState.usuario.Nombre ?? "";
                        MostrarSwal("success", "Permiso",
                            $"Salida de permiso registrada para {nombreUsuario}");
                        return;
                    }

                    if (registro.HoraSalidaPermiso.HasValue && !registro.HoraEntradaPermiso.HasValue)
                    {
                        registro.HoraEntradaPermiso = horaActual;
                        db.SubmitChanges();

                        string nombreUsuario = SesionState.usuario.Nombre ?? "";
                        MostrarSwal("success", "Permiso",
                            $"Regreso de permiso registrado para {nombreUsuario}");
                        return;
                    }
                }

                // 11. SALIDA NORMAL
                string estatusSalida = (esDomingo && horario == null) ? "Horario cumplido"
                                     : horaActual < horaFinNormal ? "Horario no cumplido" : "Horario cumplido";

                registro.HoraSalida = horaActual;
                registro.EstatusSalida = estatusSalida;
                registro.latitudSalida = latitud;
                registro.longitudSalida = longitud;

                string fingerprintSalida = (hdFingerprint.Value ?? "").Trim();
                if (fingerprintSalida.Length == 0) fingerprintSalida = "Web";
                registro.MacSalida = fingerprintSalida;

                // CALCULAR HORAS TRABAJADAS Y HORAS EXTRA
                if (registro.HoraEntrada.HasValue && registro.HoraSalida.HasValue)
                {
                    TimeSpan duracion = registro.HoraSalida.Value - registro.HoraEntrada.Value;

                    if (registro.HoraSalidaComer.HasValue && registro.HoraEntradaComer.HasValue)
                    {
                        TimeSpan tiempoComida = registro.HoraEntradaComer.Value - registro.HoraSalidaComer.Value;
                        duracion = duracion - tiempoComida;
                    }

                    registro.HorasTrabajadas = duracion;
                    registro.HorasTrabajadasDecimal = (decimal)duracion.TotalHours;

                    TimeSpan jornadaNormal = horaFinNormal - horaInicioNormal;
                    decimal horasNormales = (decimal)jornadaNormal.TotalHours;

                    if (esDomingo && horario == null)
                    {
                        // Domingo voluntario: todo lo trabajado son horas extra
                        registro.HorasExtras = registro.HorasTrabajadasDecimal;
                        registro.EstatusHorasExtras = "Horas extra domingo";
                    }
                    else if (registro.HorasTrabajadasDecimal > horasNormales)
                    {
                        registro.HorasExtras = registro.HorasTrabajadasDecimal - horasNormales;
                        registro.EstatusHorasExtras = registro.HorasExtras > 2 ?
                            "Horas extra excesivas" : "Horas extra normales";
                    }
                    else
                    {
                        registro.HorasExtras = 0;
                        registro.EstatusHorasExtras = "Sin horas extra";
                    }
                }

                db.SubmitChanges();

                string nombreUsuarioSalida = SesionState.usuario.Nombre ?? "";
                MostrarSwal("success", "Salida",
                    $"Salida registrada correctamente para {nombreUsuarioSalida}. Descansa");
            }
            catch (Exception ex)
            {
                MostrarSwal("error", "Error", "Ocurrió un error al registrar la asistencia: " + ex.Message);
            }
        }

        private void MostrarSwal(string tipo, string titulo, string mensaje)
        {
            titulo = titulo.Replace("'", "\\'").Replace("\n", " ").Replace("\r", "");
            mensaje = mensaje.Replace("'", "\\'").Replace("\n", " ").Replace("\r", "");

            string script = $@"
                Swal.fire({{
                    icon: '{tipo}',
                    title: '{titulo}',
                    text: '{mensaje}',
                    timer: 2500,
                    showConfirmButton: false
                }});

                function speakText(text) {{
                    if (!text || !window.speechSynthesis) return;
                    
                    window.speechSynthesis.cancel();
                    
                    var interval = setInterval(function() {{
                        var voices = window.speechSynthesis.getVoices();
                        if (voices.length !== 0) {{
                            clearInterval(interval);

                            var preferidas = [
                                'Google español (Latinoamérica)',
                                'Google español',
                                'es-MX-Standard-A',
                                'es-US-Standard-A',
                                'es-ES-Standard-A',
                                'Microsoft Laura',
                                'Microsoft Sabina'
                            ];

                            var selectedVoice = null;

                            for (var i = 0; i < preferidas.length; i++) {{
                                selectedVoice = voices.find(v => v.name.includes(preferidas[i]));
                                if (selectedVoice) break;
                            }}

                            if (!selectedVoice) {{
                                selectedVoice = voices.find(v => v.lang.startsWith('es'));
                            }}

                            var utter = new SpeechSynthesisUtterance(text);
                            if (selectedVoice) utter.voice = selectedVoice;
                            utter.lang = 'es-MX';
                            utter.rate = 0.95;
                            utter.pitch = 1.1;

                            window.speechSynthesis.speak(utter);
                        }}
                    }}, 200);
                }}

                setTimeout(function() {{
                    speakText('{titulo}. {mensaje}');
                }}, 300);
            ";

            ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(), script, true);
        }
    }
}