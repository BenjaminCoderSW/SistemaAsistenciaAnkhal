using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Eventing.Reader;
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

        //convierte ip a numero
        //public static long IPToLong(string ip)
        //{
        //    string[] parts = ip.Split('.');
        //    return (long.Parse(parts[0]) << 24)
        //         + (long.Parse(parts[1]) << 16)
        //         + (long.Parse(parts[2]) << 8)
        //         + long.Parse(parts[3]);
        //}

        public static long IPToLong(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip)) return 0;

            var segmentos = ip.Split('.');
            if (segmentos.Length != 4) return 0;

            return (long.Parse(segmentos[0]) << 24)
                 + (long.Parse(segmentos[1]) << 16)
                 + (long.Parse(segmentos[2]) << 8)
                 + long.Parse(segmentos[3]);
        }


        //protected void Page_Load(object sender, EventArgs e)
        //{
        //    if (SesionState.usuario != null)
        //    {

        //        UsuarioSesion = SesionState.usuario.IdUsuario;

        //        txtEmpleado.Text =
        //            SesionState.usuario.Nombre + " " +
        //            SesionState.usuario.ApellidoPaterno + " " +
        //            SesionState.usuario.ApellidoMaterno;
        //    }
        //    else
        //    {
        //        SesionState.usuario = null;
        //        Response.Redirect("login.aspx");
        //        return;
        //    }



        //    if (!IsPostBack)
        //    {
        //        txtFecha.Text = DateTime.Now.ToString("dd/MM/yyyy");
        //        txtHora.Text = DateTime.Now.ToString("HH:mm:ss");
        //    }
        //}

        //crea metodo para validar si la ip es de una planta

        protected void Page_Load(object sender, EventArgs e)
        {
            // ¿Sesion válida?
            if (SesionState.usuario == null)
            {
                SesionState.usuario = null;
                Response.Redirect("login.aspx");
                return;
            }

            // VALIDAR ROL PERMITIDO PARA ESTA PÁGINA
            // Ejemplo: solo rol "Administrador" o "RH"
            string rolUsuario = SesionState.usuario.tRol.Rol;  // ajusta al nombre que tengas en tu clase

            // Aquí pones los roles que SI pueden entrar
            string[] rolesPermitidos = { "Administrador", "Rh", "Empleado" };

            if (!rolesPermitidos.Contains(rolUsuario))
            {
                // Si NO tiene rol válido → lo sacamos
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

            long ipUserLong = IPToLong(ipUsuario);

            var plantas = db.tPlanta.ToList();  // Tu tabla con IP_INICIO e IP_FIN

            foreach (var p in plantas)
            {
                long ipIni = IPToLong(p.IP_INICIO);
                long ipFin = IPToLong(p.IP_FIN);

                if (ipUserLong >= ipIni && ipUserLong <= ipFin)
                {
                    plantaEncontrada = p;
                    return true;
                }
            }

            return false;
        }

        //obtener ip cliente
        private string ObtenerIPCliente()
        {
            string ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ip))
                return ip.Split(',')[0]; // Toma la primera IP real

            ip = Request.ServerVariables["HTTP_X_CLIENT_IP"];
            if (!string.IsNullOrEmpty(ip))
                return ip;

            ip = Request.ServerVariables["REMOTE_ADDR"];
            if (!string.IsNullOrEmpty(ip))
                return ip;

            return "IP no disponible";
        }


        public string ObtenerIPLocal()
        {
            string localIP = "";
            try
            {
                foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork) // IPv4
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }
            }
            catch
            {
                localIP = "No se pudo obtener la IP";
            }
            return localIP;
        }



        protected void btnRegistrar_Click(object sender, EventArgs e)
        {
            // 1. Obtener IP del dispositivo donde checa
            string ipUsuario = ObtenerIPCliente();

            // 2. Validar IP contra la tabla de plantas
            tPlanta planta;
            if (!EstaEnRangoPlanta(ipUsuario, out planta))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "msg",
                    "Swal.fire('Acceso Denegado', 'Tu dispositivo no pertenece a la red de ninguna planta registrada', 'error');", true);
                return;
            }

            int idUsuario = SesionState.usuario.IdUsuario;
            DateTime fechaHoy = DateTime.Now.Date;
            TimeSpan horaActual = DateTime.Now.TimeOfDay;

            // 3. Buscar registro de hoy
            var registro = db.tAsistencia.FirstOrDefault(x => x.IdUsuario == idUsuario && x.Fecha == fechaHoy);

            // 4. Verificar horario asignado
            var horario = db.v_validarhorario
                           .Where(x => x.IdUsuario == idUsuario)
                           .OrderByDescending(x => x.HoraInicio)
                           .FirstOrDefault();

            if (horario == null)
            {
                MostrarSwal("warning", "Alerta", "No existe horario asignado.");
                return;
            }

            TimeSpan horaInicioNormal = horario.HoraInicio ?? TimeSpan.Zero;
            TimeSpan horaFinNormal = horario.HoraFin ?? TimeSpan.MaxValue;

            // 5. Validar latitud y longitud
            double latitud, longitud;
            if (!double.TryParse(hdLat.Value, out latitud)) latitud = 0;
            if (!double.TryParse(hdLon.Value, out longitud)) longitud = 0;

            // 6. Obtener permiso activo para hoy
            var permisoHoy = db.tPermisoHora
                .Where(p => p.IdUsuario == idUsuario
                         && p.Dia == fechaHoy
                         && p.Estatus == 2)
                .OrderBy(p => p.HoraInicio)
                .FirstOrDefault();

            // 7. Si no hay registro, registrar entrada (considerando permiso)
            if (registro == null)
            {
                int idAsignarHorario = horario.IdAsignarHorario;

                string estatusEntrada;

                if (permisoHoy != null)
                {
                    TimeSpan horaInicioPermiso = permisoHoy.HoraInicio ?? TimeSpan.Zero;
                    TimeSpan horaFinPermiso = permisoHoy.HoraFin ?? TimeSpan.MaxValue;

                    if (horaActual <= horaFinPermiso)
                        estatusEntrada = "A tiempo";
                    else
                        estatusEntrada = "Retardo";
                }
                else
                {
                    estatusEntrada = (horaActual > horaInicioNormal) ? "Retardo" : "A tiempo";
                }

                var asistencia = new tAsistencia
                {
                    IdUsuario = idUsuario,
                    IdAsignarHorario = idAsignarHorario,
                    Fecha = fechaHoy,
                    HoraEntrada = horaActual,
                    MacEntrada = hdFingerprint.Value,
                    IP = ipUsuario,
                    IdPlanta = planta.IdPlanta,
                    latitud = Convert.ToDecimal(latitud),
                    longitud = Convert.ToDecimal(longitud),
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

                MostrarSwal("success", "Registro", "Entrada registrada correctamente.");
                return;
            }

            // 8. Registrar SALIDA A COMER
            if (registro.HoraSalidaComer == null)
            {
                registro.HoraSalidaComer = horaActual;
                db.SubmitChanges();
                MostrarSwal("success", "Salida", "Salida a comer registrada.");
                return;
            }

            // 9. Registrar ENTRADA DE COMER
            if (registro.HoraEntradaComer == null)
            {
                registro.HoraEntradaComer = horaActual;
                if (registro.HoraEntradaComer.HasValue && registro.HoraSalidaComer.HasValue)
                {
                    TimeSpan duracionComida = registro.HoraEntradaComer.Value - registro.HoraSalidaComer.Value;
                    decimal minutosComida = (decimal)duracionComida.TotalMinutes;

                    registro.HoraComida = (decimal)duracionComida.TotalHours;

                    if (minutosComida <= 60)
                    {
                        registro.EstatusComida = "Comida a tiempo";
                    }
                    else
                    {
                        registro.EstatusComida = "Retardo Comida";
                    }
                }

                db.SubmitChanges();
                MostrarSwal("success", "Entrada", "Entrada de comer registrada.");
                return;
            }

            // 10. Registrar SALIDA NORMAL O PERMISO POSTERIOR
            string estatusSalida;

            if (permisoHoy != null)
            {
                TimeSpan horaInicioPermiso = permisoHoy.HoraInicio ?? TimeSpan.Zero;
                TimeSpan horaFinPermiso = permisoHoy.HoraFin ?? TimeSpan.MaxValue;

                if (!registro.HoraSalidaPermiso.HasValue && horaActual >= horaInicioPermiso && horaActual <= horaFinPermiso)
                {
                    registro.HoraSalidaPermiso = horaActual;
                    db.SubmitChanges();
                    MostrarSwal("success", "Permiso", "Salida de permiso registrada.");
                    return;
                }

                if (registro.HoraSalidaPermiso.HasValue && !registro.HoraEntradaPermiso.HasValue)
                {
                    registro.HoraEntradaPermiso = horaActual;
                    estatusSalida = (horaActual > horaFinNormal) ? "Retardo" : "A tiempo";
                    db.SubmitChanges();
                    MostrarSwal("success", "Permiso", "Regreso de permiso registrado.");
                    return;
                }
            }

            // Salida normal
            if (horaActual <= horaFinNormal)
                estatusSalida = "Horario no cumplido";
            else
                estatusSalida = "Horario cumplido";

            registro.HoraSalida = horaActual;
            registro.EstatusSalida = estatusSalida;
            registro.latitudSalida = Convert.ToDecimal(latitud);
            registro.longitudSalida = Convert.ToDecimal(longitud);
            registro.MacSalida = hdFingerprint.Value;

            // CALCULAR HORAS TRABAJADAS Y HORAS EXTRA (CON DESCUENTO DE COMIDA)
            if (registro.HoraEntrada.HasValue && registro.HoraSalida.HasValue)
            {
                TimeSpan duracion = registro.HoraSalida.Value - registro.HoraEntrada.Value;

                // Descontar tiempo de comida si existe
                if (registro.HoraSalidaComer.HasValue && registro.HoraEntradaComer.HasValue)
                {
                    TimeSpan tiempoComida = registro.HoraEntradaComer.Value - registro.HoraSalidaComer.Value;
                    duracion = duracion - tiempoComida;
                }

                registro.HorasTrabajadas = duracion;
                registro.HorasTrabajadasDecimal = (decimal)duracion.TotalHours;

                // Calcular la jornada normal (hora fin - hora inicio)
                TimeSpan jornadalNormal = horaFinNormal - horaInicioNormal;
                decimal horasNormales = (decimal)jornadalNormal.TotalHours;

                // Si las horas trabajadas superan las horas normales, calcular horas extra
                if (registro.HorasTrabajadasDecimal > horasNormales)
                {
                    registro.HorasExtras = registro.HorasTrabajadasDecimal - horasNormales;

                    if (registro.HorasExtras > 0 && registro.HorasExtras <= 2)
                    {
                        registro.EstatusHorasExtras = "Horas extra normales";
                    }
                    else if (registro.HorasExtras > 2)
                    {
                        registro.EstatusHorasExtras = "Horas extra excesivas";
                    }
                }
                else
                {
                    registro.HorasExtras = 0;
                    registro.EstatusHorasExtras = "Sin horas extra";
                }
            }

            db.SubmitChanges();
            MostrarSwal("success", "Salida", "Salida registrada correctamente. Descansa.");
        }


        private void MostrarSwal(string tipo, string titulo, string mensaje)
        {
            string script = $@"
        Swal.fire({{
            icon: '{tipo}',
            title: '{titulo}',
            text: '{mensaje}',
            timer: 2000,
            showConfirmButton: false
        }});

        function speakText(text) {{
            var synth = window.speechSynthesis;

            var interval = setInterval(function() {{
                var voices = synth.getVoices();
                if (voices.length !== 0) {{
                    clearInterval(interval);

                    // PRIORIDAD: voces de mujer de Google
                    var preferidas = [
                        'Google español (Latinoamérica)',
                        'Google español',
                        'es-MX-Standard-A',
                        'es-US-Standard-A',
                        'es-ES-Standard-A'
                    ];

                    var selectedVoice = null;

                    for (var i = 0; i < preferidas.length; i++) {{
                        selectedVoice = voices.find(v => v.name.includes(preferidas[i]));
                        if (selectedVoice) break;
                    }}

                    var utter = new SpeechSynthesisUtterance(text);
                    utter.voice = selectedVoice;
                    utter.lang = 'es-MX';
                    utter.rate = 1; 
                    utter.pitch = 1.1; // Más femenina

                    synth.speak(utter);
                }}
            }}, 200);
        }}

        // SOLO VOZ DE MUJER
        speakText('{titulo}. {mensaje}');
    ";

            ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(), script, true);
        }

    }
}