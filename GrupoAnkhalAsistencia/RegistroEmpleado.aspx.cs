using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Collections.Generic;
using System.Configuration;
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
            if (string.IsNullOrWhiteSpace(ip)) return 0;

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
            string[] rolesPermitidos = { "Administrador", "Rh", "Empleado" };

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

        private bool EstaEnRangoPlanta(string ipUsuario, out tPlanta plantaEncontrada)
        {
            plantaEncontrada = null;

            if (string.IsNullOrWhiteSpace(ipUsuario))
                return false;

            long ipUserLong = IPToLong(ipUsuario);

            if (ipUserLong == 0)
                return false;

            var plantas = db.tPlanta.Where(p => !string.IsNullOrEmpty(p.IP_INICIO) && !string.IsNullOrEmpty(p.IP_FIN)).ToList();

            foreach (var p in plantas)
            {
                long ipIni = IPToLong(p.IP_INICIO);
                long ipFin = IPToLong(p.IP_FIN);

                if (ipIni > 0 && ipFin > 0 && ipUserLong >= ipIni && ipUserLong <= ipFin)
                {
                    plantaEncontrada = p;
                    return true;
                }
            }

            return false;
        }

        private string ObtenerIPCliente()
        {
            string ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ip))
                return ip.Split(',')[0].Trim();

            ip = Request.ServerVariables["HTTP_X_CLIENT_IP"];
            if (!string.IsNullOrEmpty(ip))
                return ip.Trim();

            ip = Request.ServerVariables["REMOTE_ADDR"];
            if (!string.IsNullOrEmpty(ip))
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

        protected void btnRegistrar_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Obtener IP del dispositivo (SOLO PARA REGISTRO, NO VALIDACIÓN)
                string ipUsuario = ObtenerIPCliente();

                if (string.IsNullOrWhiteSpace(ipUsuario) || ipUsuario == "IP no disponible")
                {
                    ipUsuario = "IP no detectada";
                }

                int idUsuario = SesionState.usuario.IdUsuario;
                DateTime fechaHoy = DateTime.Now.Date;
                TimeSpan horaActual = DateTime.Now.TimeOfDay;

                // 2. Buscar registro de hoy
                var registro = db.tAsistencia.FirstOrDefault(x => x.IdUsuario == idUsuario && x.Fecha == fechaHoy);

                // 3. Verificar horario asignado
                var horario = db.v_validarhorario
                               .Where(x => x.IdUsuario == idUsuario)
                               .OrderByDescending(x => x.HoraInicio)
                               .FirstOrDefault();

                if (horario == null)
                {
                    MostrarSwal("warning", "Alerta", "No existe horario asignado para este empleado");
                    return;
                }

                TimeSpan horaInicioNormal = horario.HoraInicio ?? TimeSpan.Zero;
                TimeSpan horaFinNormal = horario.HoraFin ?? TimeSpan.MaxValue;

                // 4. Validar latitud y longitud - CORRECCIÓN
                decimal latitud = 0, longitud = 0;

                if (!string.IsNullOrWhiteSpace(hdLat.Value))
                {
                    // Forzar punto como separador decimal
                    string latStr = hdLat.Value.Trim().Replace(',', '.');
                    if (!decimal.TryParse(latStr, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out latitud))
                    {
                        MostrarSwal("warning", "Ubicación inválida", "La latitud no tiene un formato válido.");
                        return;
                    }
                }

                if (!string.IsNullOrWhiteSpace(hdLon.Value))
                {
                    // Forzar punto como separador decimal
                    string lonStr = hdLon.Value.Trim().Replace(',', '.');
                    if (!decimal.TryParse(lonStr, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out longitud))
                    {
                        MostrarSwal("warning", "Ubicación inválida", "La longitud no tiene un formato válido.");
                        return;
                    }
                }

                // Validar que las coordenadas sean válidas
                if (latitud == 0 && longitud == 0)
                {
                    MostrarSwal("warning", "Sin ubicación", "No se pudo obtener la ubicación GPS. Intenta nuevamente.");
                    return;
                }

                // 5. Intentar detectar planta por IP (OPCIONAL, no bloquea si falla)
                tPlanta plantaDetectada = null;
                EstaEnRangoPlanta(ipUsuario, out plantaDetectada);

                // Si no se detectó planta por IP, usar la planta asignada al usuario
                int? idPlanta = plantaDetectada?.IdPlanta ?? SesionState.usuario.IdPlanta;

                // 6. Obtener permiso activo para hoy
                var permisoHoy = db.tPermisoHora
                    .Where(p => p.IdUsuario == idUsuario
                             && p.Dia == fechaHoy
                             && p.Estatus == 2)
                    .OrderBy(p => p.HoraInicio)
                    .FirstOrDefault();

                // 7. ENTRADA
                if (registro == null)
                {
                    int idAsignarHorario = horario.IdAsignarHorario;
                    string estatusEntrada;

                    if (permisoHoy != null)
                    {
                        TimeSpan horaFinPermiso = permisoHoy.HoraFin ?? TimeSpan.MaxValue;
                        estatusEntrada = horaActual <= horaFinPermiso ? "A tiempo" : "Retardo";
                    }
                    else
                    {
                        estatusEntrada = horaActual > horaInicioNormal ? "Retardo" : "A tiempo";
                    }

                    var asistencia = new tAsistencia
                    {
                        IdUsuario = idUsuario,
                        IdAsignarHorario = idAsignarHorario,
                        Fecha = fechaHoy,
                        HoraEntrada = horaActual,
                        MacEntrada = hdFingerprint.Value ?? "Web",
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

                    string nombreUsuario = SesionState.usuario.Nombre;
                    MostrarSwal("success", "Entrada",
                        $"Entrada registrada correctamente para {nombreUsuario}");
                    return;
                }

                // 8. SALIDA A COMER
                if (registro.HoraSalidaComer == null)
                {
                    registro.HoraSalidaComer = horaActual;
                    db.SubmitChanges();

                    string nombreUsuario = SesionState.usuario.Nombre;
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

                    string nombreUsuario = SesionState.usuario.Nombre;
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

                        string nombreUsuario = SesionState.usuario.Nombre;
                        MostrarSwal("success", "Permiso",
                            $"Salida de permiso registrada para {nombreUsuario}");
                        return;
                    }

                    if (registro.HoraSalidaPermiso.HasValue && !registro.HoraEntradaPermiso.HasValue)
                    {
                        registro.HoraEntradaPermiso = horaActual;
                        db.SubmitChanges();

                        string nombreUsuario = SesionState.usuario.Nombre;
                        MostrarSwal("success", "Permiso",
                            $"Regreso de permiso registrado para {nombreUsuario}");
                        return;
                    }
                }

                // 11. SALIDA NORMAL
                string estatusSalida = horaActual < horaFinNormal ? "Horario no cumplido" : "Horario cumplido";

                registro.HoraSalida = horaActual;
                registro.EstatusSalida = estatusSalida;
                registro.latitudSalida = latitud;
                registro.longitudSalida = longitud;
                registro.MacSalida = hdFingerprint.Value ?? "Web";

                // CALCULAR HORAS TRABAJADAS Y HORAS EXTRA
                if (registro.HoraEntrada.HasValue && registro.HoraSalida.HasValue)
                {
                    TimeSpan duracion = registro.HoraSalida.Value - registro.HoraEntrada.Value;

                    // Descontar tiempo de comida
                    if (registro.HoraSalidaComer.HasValue && registro.HoraEntradaComer.HasValue)
                    {
                        TimeSpan tiempoComida = registro.HoraEntradaComer.Value - registro.HoraSalidaComer.Value;
                        duracion = duracion - tiempoComida;
                    }

                    registro.HorasTrabajadas = duracion;
                    registro.HorasTrabajadasDecimal = (decimal)duracion.TotalHours;

                    TimeSpan jornadaNormal = horaFinNormal - horaInicioNormal;
                    decimal horasNormales = (decimal)jornadaNormal.TotalHours;

                    if (registro.HorasTrabajadasDecimal > horasNormales)
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

                string nombreUsuarioSalida = SesionState.usuario.Nombre;
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
            // Escapar caracteres especiales para JavaScript
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

                            // Voces preferidas en español
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

                            // Buscar voz preferida
                            for (var i = 0; i < preferidas.length; i++) {{
                                selectedVoice = voices.find(v => v.name.includes(preferidas[i]));
                                if (selectedVoice) break;
                            }}

                            // Si no encuentra, usar cualquier voz en español
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

                // Hablar el mensaje
                setTimeout(function() {{
                    speakText('{titulo}. {mensaje}');
                }}, 300);
            ";

            ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(), script, true);
        }
    }
}