using GrupoAnkhalAsistencia.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace GrupoAnkhalAsistencia
{
    public class Global : System.Web.HttpApplication
    {
        // Timer para actualización automática de vacaciones
        private static System.Threading.Timer timerVacaciones;

        // Timer para registro automático de faltas
        private static System.Threading.Timer timerFaltas;

        protected void Application_Start(object sender, EventArgs e)
        {
            // CONFIGURAR ACTUALIZACIÓN AUTOMÁTICA DE VACACIONES
            IniciarActualizacionAutomaticaVacaciones();

            // CONFIGURAR REGISTRO AUTOMÁTICO DE FALTAS
            IniciarRegistroAutomaticoFaltas();
        }

        protected void Session_Start(object sender, EventArgs e)
        {
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
        }

        protected void Application_Error(object sender, EventArgs e)
        {
        }

        protected void Session_End(object sender, EventArgs e)
        {
        }

        protected void Application_End(object sender, EventArgs e)
        {
            // Limpiar el timer de vacaciones
            if (timerVacaciones != null)
            {
                timerVacaciones.Dispose();
                timerVacaciones = null;
            }

            // Limpiar el timer de faltas
            if (timerFaltas != null)
            {
                timerFaltas.Dispose();
                timerFaltas = null;
            }
        }

        // ========================================
        // MÉTODOS PARA ACTUALIZACIÓN DE VACACIONES
        // ========================================
        private void IniciarActualizacionAutomaticaVacaciones()
        {
            try
            {
                DateTime ahora = DateTime.Now;
                DateTime proximaEjecucion = ahora.Date.AddDays(1).AddHours(2); // 2:00 AM del día siguiente
                TimeSpan tiempoHastaProximaEjecucion = proximaEjecucion - ahora;

                timerVacaciones = new System.Threading.Timer(
                    callback: ActualizarVacacionesCallback,
                    state: null,
                    dueTime: tiempoHastaProximaEjecucion,
                    period: TimeSpan.FromHours(24)
                );

                System.Diagnostics.Debug.WriteLine($"Timer de vacaciones configurado. Primera ejecución: {proximaEjecucion}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al configurar timer de vacaciones: {ex.Message}");
            }
        }

        private void ActualizarVacacionesCallback(object state)
        {
            try
            {
                VacacionesHelper.ActualizarDiasVacacionesAutomatico();
                System.Diagnostics.Debug.WriteLine($"Vacaciones actualizadas automáticamente: {DateTime.Now}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en actualización automática de vacaciones: {ex.Message}");
            }
        }

        // ========================================
        // MÉTODOS PARA REGISTRO AUTOMÁTICO DE FALTAS
        // ========================================
        private void IniciarRegistroAutomaticoFaltas()
        {
            try
            {
                // =====================================================
                // HORA DE EJECUCIÓN
                // PRUEBA:      19 horas, 23 minutos (7:23 PM)
                // PRODUCCIÓN:  23 horas, 59 minutos (11:59 PM)
                // =====================================================
                int horaEjecucion = 20; // <-- CAMBIA A 23 EN PRODUCCIÓN
                int minutoEjecucion = 17; // <-- CAMBIA A 59 EN PRODUCCIÓN

                DateTime ahora = DateTime.Now;
                DateTime horaHoy = new DateTime(ahora.Year, ahora.Month, ahora.Day,
                                                horaEjecucion, minutoEjecucion, 0);

                // Si ya pasó la hora de hoy, programar para mañana
                if (ahora >= horaHoy)
                    horaHoy = horaHoy.AddDays(1);

                TimeSpan tiempoHasta = horaHoy - ahora;

                timerFaltas = new System.Threading.Timer(
                    callback: RegistrarFaltasCallback,
                    state: null,
                    dueTime: tiempoHasta,
                    period: TimeSpan.FromHours(24)
                );

                System.Diagnostics.Debug.WriteLine(
                    $"[RegistrarFaltas] Timer configurado. Primera ejecucion: {horaHoy:dd/MM/yyyy HH:mm:ss}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[RegistrarFaltas] Error al configurar timer: {ex.Message}");
            }
        }

        private void RegistrarFaltasCallback(object state)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[RegistrarFaltas] Ejecutando SP a las {DateTime.Now:dd/MM/yyyy HH:mm:ss}");

                string connStr = ConfigurationManager
                    .ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_RegistrarFaltasDelDia", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Fecha", DateTime.Today);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int faltasInsertadas = reader.GetInt32(0);
                                System.Diagnostics.Debug.WriteLine(
                                    $"[RegistrarFaltas] Faltas insertadas: {faltasInsertadas}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[RegistrarFaltas] ERROR al ejecutar SP: {ex.Message}");
            }
        }
    }
}