using GrupoAnkhalAsistencia.Helpers;
using System;
using System.Collections.Generic;
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

        protected void Application_Start(object sender, EventArgs e)
        {
            // CONFIGURAR ACTUALIZACIÓN AUTOMÁTICA DE VACACIONES
            IniciarActualizacionAutomaticaVacaciones();
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
            // Limpiar el timer
            if (timerVacaciones != null)
            {
                timerVacaciones.Dispose();
                timerVacaciones = null;
            }
        }

        // ========================================
        // MÉTODOS PARA ACTUALIZACIÓN DE VACACIONES
        // ========================================

        private void IniciarActualizacionAutomaticaVacaciones()
        {
            try
            {
                // Calcular el tiempo hasta las 2:00 AM del día siguiente
                DateTime ahora = DateTime.Now;
                DateTime proximaEjecucion = ahora.Date.AddDays(1).AddHours(2); // 2:00 AM del día siguiente

                TimeSpan tiempoHastaProximaEjecucion = proximaEjecucion - ahora;

                // Crear timer que se ejecuta cada 24 horas (86400000 ms)
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
    }
}