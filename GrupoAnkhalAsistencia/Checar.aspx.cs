using GrupoAnkhalAsistencia.Modelo;
using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class Checar : System.Web.UI.Page
    {
        private dbAsistenciaDataContext db = new dbAsistenciaDataContext(
            ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        private const string SessionChecarMsg = "ChecarMsg";

        protected void Page_Load(object sender, EventArgs e)
        {
            // Mostrar mensaje solo tras redirect (GET). Así el refresh no reenvía el POST.
            if (IsPostBack) return;
            var msg = Session[SessionChecarMsg] as string;
            if (string.IsNullOrEmpty(msg)) return;
            Session.Remove(SessionChecarMsg);
            var parts = msg.Split(new[] { '\x1f' }, 3);
            var tipo = parts.Length > 0 ? parts[0] : "info";
            var titulo = parts.Length > 1 ? parts[1] : "";
            var mensaje = parts.Length > 2 ? parts[2] : "";
            EmitirSwal(tipo, titulo, mensaje, reload: false);
        }

        protected void btnChecarQr_Click(object sender, EventArgs e)
        {
            try
            {
                string codigo = (hdQr.Value ?? "").Trim();
                hdQr.Value = "";

                if (string.IsNullOrEmpty(codigo))
                {
                    RedirectConMensaje("error", "Error", "No se leyó ningún código QR.");
                    return;
                }

                var usuario = db.tUsuario.FirstOrDefault(u => u.NumeroEmpleado == codigo && u.Estatus == 1);

                if (usuario == null)
                {
                    RedirectConMensaje("error", "Credencial no válida", "No se encontró un empleado activo con ese número.");
                    return;
                }

                int idUsuario = usuario.IdUsuario;
                DateTime fechaHoy = DateTime.Now.Date;
                TimeSpan horaActual = DateTime.Now.TimeOfDay;

                var registro = db.tAsistencia.FirstOrDefault(x => x.IdUsuario == idUsuario && x.Fecha == fechaHoy);
                var horario = db.v_validarhorario
                    .Where(x => x.IdUsuario == idUsuario)
                    .OrderByDescending(x => x.HoraInicio)
                    .FirstOrDefault();

                TimeSpan horaInicioNormal = horario?.HoraInicio ?? TimeSpan.Zero;
                TimeSpan horaFinNormal = horario?.HoraFin ?? TimeSpan.MaxValue;

                var permisoHoy = db.tPermisoHora
                    .Where(p => p.IdUsuario == idUsuario && p.Dia == fechaHoy && p.Estatus == 2)
                    .OrderBy(p => p.HoraInicio)
                    .FirstOrDefault();

                // ENTRADA
                if (registro == null)
                {
                    if (horario == null)
                    {
                        RedirectConMensaje("warning", "Alerta", "No existe horario asignado para este empleado.");
                        return;
                    }

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
                        MacEntrada = "QR",
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

                    RedirectConMensaje("success", "Entrada", "Entrada registrada correctamente para " + usuario.Nombre + ".");
                    return;
                }

                // SALIDA A COMER
                if (registro.HoraSalidaComer == null)
                {
                    registro.HoraSalidaComer = horaActual;
                    db.SubmitChanges();
                    RedirectConMensaje("success", "Salida a comer", "Salida a comer registrada para " + usuario.Nombre + ".");
                    return;
                }

                // ENTRADA DE COMER
                if (registro.HoraEntradaComer == null)
                {
                    registro.HoraEntradaComer = horaActual;
                    if (registro.HoraSalidaComer.HasValue && registro.HoraEntradaComer.HasValue)
                    {
                        TimeSpan duracionComida = registro.HoraEntradaComer.Value - registro.HoraSalidaComer.Value;
                        decimal minutosComida = (decimal)duracionComida.TotalMinutes;
                        registro.HoraComida = (decimal)duracionComida.TotalHours;
                        registro.EstatusComida = minutosComida <= 60 ? "Comida a tiempo" : "Retardo Comida";
                    }
                    db.SubmitChanges();
                    RedirectConMensaje("success", "Entrada de comer", "Entrada de comer registrada para " + usuario.Nombre + ".");
                    return;
                }

                // PERMISOS (salida / regreso)
                if (permisoHoy != null)
                {
                    TimeSpan horaInicioPermiso = permisoHoy.HoraInicio ?? TimeSpan.Zero;
                    TimeSpan horaFinPermiso = permisoHoy.HoraFin ?? TimeSpan.MaxValue;

                    if (!registro.HoraSalidaPermiso.HasValue &&
                        horaActual >= horaInicioPermiso && horaActual <= horaFinPermiso)
                    {
                        registro.HoraSalidaPermiso = horaActual;
                        db.SubmitChanges();
                        RedirectConMensaje("success", "Permiso", "Salida de permiso registrada para " + usuario.Nombre + ".");
                        return;
                    }

                    if (registro.HoraSalidaPermiso.HasValue && !registro.HoraEntradaPermiso.HasValue)
                    {
                        registro.HoraEntradaPermiso = horaActual;
                        db.SubmitChanges();
                        RedirectConMensaje("success", "Permiso", "Regreso de permiso registrado para " + usuario.Nombre + ".");
                        return;
                    }
                }

                // SALIDA NORMAL
                string estatusSalida = horaActual < horaFinNormal ? "Horario no cumplido" : "Horario cumplido";
                registro.HoraSalida = horaActual;
                registro.EstatusSalida = estatusSalida;
                registro.MacSalida = "QR";

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

                    if (registro.HorasTrabajadasDecimal > horasNormales)
                    {
                        registro.HorasExtras = registro.HorasTrabajadasDecimal - horasNormales;
                        registro.EstatusHorasExtras = registro.HorasExtras > 2 ? "Horas extra excesivas" : "Horas extra normales";
                    }
                    else
                    {
                        registro.HorasExtras = 0;
                        registro.EstatusHorasExtras = "Sin horas extra";
                    }
                }

                db.SubmitChanges();
                RedirectConMensaje("success", "Salida", "Salida registrada correctamente para " + usuario.Nombre + ". Descansa.");
            }
            catch (Exception ex)
            {
                RedirectConMensaje("error", "Error", ex.Message);
            }
        }

        /// <summary>
        /// Guarda el mensaje en sesión y redirige a GET. Evita que al refrescar se reenvíe el POST y se cheque varias veces.
        /// </summary>
        private void RedirectConMensaje(string tipo, string titulo, string mensaje)
        {
            var t = (titulo ?? "").Replace("\x1f", " ");
            var m = (mensaje ?? "").Replace("\x1f", " ");
            Session[SessionChecarMsg] = tipo + "\x1f" + t + "\x1f" + m;
            Response.Redirect("Checar.aspx", false);
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }

        private void EmitirSwal(string tipo, string titulo, string mensaje, bool reload)
        {
            var t = (titulo ?? "").Replace("\\", "\\\\").Replace("'", "\\'").Replace("\r", "").Replace("\n", " ");
            var m = (mensaje ?? "").Replace("\\", "\\\\").Replace("'", "\\'").Replace("\r", "").Replace("\n", " ");
            var reloadJs = reload ? "window.location.reload();" : "";
            var speakTxt = (titulo ?? "") + ". " + (mensaje ?? "");
            var speakEsc = speakTxt.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\r", "").Replace("\n", " ");
            var script = $@"
                Swal.fire({{
                    icon: '{tipo}',
                    title: '{t}',
                    text: '{m}',
                    timer: 2200,
                    showConfirmButton: false
                }}).then(function() {{ {reloadJs} }});
                var _msg = '{speakEsc}';
                if (typeof speakChecar === 'function' && _msg)
                    setTimeout(function() {{ speakChecar(_msg); }}, 350);
            ";
            ClientScript.RegisterStartupScript(GetType(), Guid.NewGuid().ToString(), script, true);
        }
    }
}
