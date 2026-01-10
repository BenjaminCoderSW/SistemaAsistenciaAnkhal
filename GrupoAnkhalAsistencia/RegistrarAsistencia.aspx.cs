using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class RegistrarAsistencia : System.Web.UI.Page
    {
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
        ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        public int UsuarioSesion = SesionState.usuario.IdUsuario;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (SesionState.usuario != null)
            {
                txtNombreEmpleado.Text =
                    SesionState.usuario.Nombre + " " +
                    SesionState.usuario.ApellidoPaterno + " " +
                    SesionState.usuario.ApellidoMaterno;
            }
            else
            {
                SesionState.usuario = null;
                Response.Redirect("login.aspx");
            }

            if (!IsPostBack)
            {
                txtFecha.Text = DateTime.Now.ToString("dd/MM/yyyy");
                txtHora.Text = DateTime.Now.ToString("HH:mm:ss");
            }
        }

     //
        private static double DistanciaMetros(double lat1, double lon1, double lat2, double lon2)
        {
            double R = 6371000; // Radio de la Tierra en metros
            double rad(double x) => x * Math.PI / 180;

            double dLat = rad(lat2 - lat1);
            double dLon = rad(lon2 - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(rad(lat1)) * Math.Cos(rad(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }


        protected void btnRegistrar_Click(object sender, EventArgs e)
        {
            int idUsuario = SesionState.usuario.IdUsuario;
            DateTime fechaHoy = DateTime.Now.Date;
            TimeSpan horaActual = DateTime.Now.TimeOfDay;

            // Buscar si hay registro de hoy
            var registro = db.tAsistencia
                .FirstOrDefault(x => x.IdUsuario == idUsuario && x.Fecha == fechaHoy);


            //primero verifica que el usuario tenga un horario asignado

            int total = db.v_validarhorario
              .Count(x => x.IdUsuario == SesionState.usuario.IdUsuario);
            if (total == 0)
            {
                MostrarSwal("warning", "Alerta", "No existe horario asignado.");
                return;
            }




            //aqui debe de ir una validacion que valide el horario registrado al usuario eso esta en la tabla [dbo].[tAsignarHorario]

            var horaInicio = db.v_validarhorario
                   .Where(x => x.IdUsuario == SesionState.usuario.IdUsuario)
                   .Max(x => (TimeSpan?)x.HoraInicio);

            string estatus;

            //esta consulta es para mi hora de salida
            var horaFin = db.v_validarhorario
                .Where(x => x.IdUsuario == SesionState.usuario.IdUsuario)
                .Max(x => (TimeSpan?)x.HoraFin);

            string estatusfin;





            // 1) Registrar ENTRADA
            if (registro == null)
            {
                //valida el IdAsignarHorario del usuario    uw inicio la session
                int idAsignarHorario = db.v_validarhorario
                .Where(x => x.IdUsuario == SesionState.usuario.IdUsuario)
                .Select(x => x.IdAsignarHorario)
                .FirstOrDefault();

                // ----------------------------------------
                // A) Validar estatus (A tiempo / Retardo)
                // ----------------------------------------
                if (horaInicio.HasValue)
                {
                    estatus = (horaActual > horaInicio.Value)
                                ? "Retardo"
                                : "A tiempo";
                }
                else
                {
                    estatus = "Error al obtener horario";
                }


                // ----------------------------------------
                // B) Validar ubicación (latitud / longitud)
                // ----------------------------------------

                // OBTENER lat y lon desde los hidden fields
                //double lat = Convert.ToDouble(hdLat.Value);
                //double lon = Convert.ToDouble(hdLon.Value);
                double lat = 0;
                double lon = 0;

                // Si viene vacío o null, TryParse NO truena y deja el valor en 0
                double.TryParse(hdLat.Value?.Trim(), out lat);
                double.TryParse(hdLon.Value?.Trim(), out lon);

                // Si NO llegó ubicación válida, detener el proceso
                if (lat == 0 || lon == 0)
                {
                    MostrarSwal("warning", "Alerta", "No se recibio la ubicación intente de nuevo.");
                    return;
                }

                double radioPermitido = 3000; // metros

                // Obtener plantas
                var plantas = db.tPlanta
                    .ToList()
                    .Select(p => new
                    {
                        p.IdPlanta,
                        p.Planta,
                        Lat = double.TryParse(p.latitud, out double la) ? la : 0,
                        Lon = double.TryParse(p.longitud, out double lo) ? lo : 0
                    })
                    .ToList();

                // DEBUG: muestra distancias en consola
                foreach (var p in plantas)
                {
                    double dtest = DistanciaMetros(lat, lon, p.Lat, p.Lon);
                    System.Diagnostics.Debug.WriteLine(
                        $"PLANTA {p.IdPlanta} | {p.Planta} | DISTANCIA = {dtest} metros");
                }

                // Buscar planta dentro del radio permitido
                var plantaEncontrada = plantas
                    .Select(p => new
                    {
                        p.IdPlanta,
                        p.Planta,
                        Distancia = DistanciaMetros(lat, lon, p.Lat, p.Lon)
                    })
                    .Where(p => p.Distancia <= radioPermitido)
                    .OrderBy(p => p.Distancia)
                    .FirstOrDefault();

                bool inside = plantaEncontrada != null;
                int idPlanta = plantaEncontrada?.IdPlanta ?? 0;

                

                if (idPlanta==0)
                {
                    MostrarSwal("warning", "Alerta", "las cordenadas no coinciden con las de la planta no se puede checar intente de nuevo.");
                    return;
                }




                // ----------------------------------------
                // C) Registrar asistencia
                // ----------------------------------------
                tAsistencia nuevo = new tAsistencia();
                nuevo.IdUsuario = idUsuario;
                nuevo.IdAsignarHorario = idAsignarHorario;
                nuevo.Fecha = fechaHoy;
                nuevo.HoraEntrada = horaActual;
                nuevo.EstatusEntrada = estatus;
                nuevo.MacEntrada= hdFingerprint.Value; //checa mac entrada

                // Guardar ubicación detectada
                nuevo.latitud =Convert.ToDecimal(lat.ToString());
                nuevo.longitud =Convert.ToDecimal(lon.ToString());
                nuevo.IdPlanta = idPlanta;
                

                db.tAsistencia.InsertOnSubmit(nuevo);
                db.SubmitChanges();

                //string mensajeAlerta = inside
                //    ? "Hora de entrada Registrada correctamente"
                //    : "Registrado fuera de planta";
                string nombre1 = $"«{SesionState.usuario.Nombre}»";
                MostrarSwal("success", "Entrada", $"{nombre1}, hora de entrada registrada correctamente, buen dia.");
                return;
            }



            // 2) Registrar SALIDA A COMER
            if (registro.HoraSalidaComer == null)
            {
                registro.HoraSalidaComer = horaActual;
                db.SubmitChanges();

                string nombre2 = $"«{SesionState.usuario.Nombre}»";

                MostrarSwal("success", "Salida", $"{nombre2}, Salida  comer registrada.");
                return;

            }

            // 3) Registrar ENTRADA DE COMER
            if (registro.HoraEntradaComer == null)
            {
                registro.HoraEntradaComer = horaActual;
                db.SubmitChanges();

                string nombre3 = $"«{SesionState.usuario.Nombre}»";
                MostrarSwal("success", "Entrada", $"{nombre3}, Entrada de comer registrada correctamente.");
                return;
            }

            // 4) Registrar SALIDA /////////////////////////////////////////////////////////////////////////////////////////////////////7

            //valida si la hora de salida es correcta
            if (horaFin.HasValue)
            {
                estatusfin = (horaActual < horaFin.Value)
                            ? "Horario no cumplido"
                            : "Horario Cumplido";
            }
            else
            {
                estatusfin = "Error al obtener horario";
            }

            //obtine la localizacion de salida

            // OBTENER lat y lon desde los hidden fields
            double latSalida = Convert.ToDouble(hdLat.Value);
            double lonSalida = Convert.ToDouble(hdLon.Value);

            double radioPermitidoSalida = 2000; // metros

            // Obtener plantas
            var plantasSalida = db.tPlanta
                .ToList()
                .Select(p => new
                {
                    p.IdPlanta,
                    p.Planta,
                    Lat = double.TryParse(p.latitud, out double la) ? la : 0,
                    Lon = double.TryParse(p.longitud, out double lo) ? lo : 0
                })
                .ToList();

            // DEBUG: muestra distancias en consola
            foreach (var p in plantasSalida)
            {
                double dtest = DistanciaMetros(latSalida, lonSalida, p.Lat, p.Lon);
                System.Diagnostics.Debug.WriteLine(
                    $"PLANTA {p.IdPlanta} | {p.Planta} | DISTANCIA = {dtest} metros");
            }

            // Buscar planta dentro del radio permitido
            var plantaEncontradaSalida = plantasSalida
                .Select(p => new
                {
                    p.IdPlanta,
                    p.Planta,
                    Distancia = DistanciaMetros(latSalida, lonSalida, p.Lat, p.Lon)
                })
                .Where(p => p.Distancia <= radioPermitidoSalida)
                .OrderBy(p => p.Distancia)
                .FirstOrDefault();

            bool insideSalida = plantaEncontradaSalida != null;
            int idPlantaSalida = plantaEncontradaSalida?.IdPlanta ?? 0;



            if (idPlantaSalida == 0)
            {
                MostrarSwal("warning", "Alerta", "las cordenadas no coinciden con las de la planta, no se puede checar, intente de nuevo.");
                return;
            }

            registro.HoraSalida = horaActual;
            registro.EstatusSalida = estatusfin;
            registro.latitudSalida = Convert.ToDecimal(latSalida);
            registro.longitudSalida = Convert.ToDecimal(lonSalida);
            registro.MacSalida= hdFingerprint.Value; //esta es la mac del dispositivo que checa la salida

            db.SubmitChanges();

            string nombre4 = $"«{SesionState.usuario.Nombre}»";
            MostrarSwal("success", "Salida", $"{nombre4}, registrada correctamente, descansa");
            return;
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
