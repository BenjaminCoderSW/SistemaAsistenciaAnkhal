using GrupoAnkhalAsistencia.Modelo;
using System;
using System.Configuration;
using System.Linq;

namespace GrupoAnkhalAsistencia.Helpers
{
    /// <summary>
    /// Helper para gestión automática de días de vacaciones
    /// </summary>
    public class VacacionesHelper
    {
        private static dbAsistenciaDataContext db = new dbAsistenciaDataContext(
            ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        /// <summary>
        /// Actualiza los días de vacaciones de todos los usuarios según su antigüedad
        /// Este método debe ejecutarse diariamente (idealmente en Global.asax o mediante un Job)
        /// </summary>
        public static void ActualizarDiasVacacionesAutomatico()
        {
            try
            {
                // Ejecutar el stored procedure
                db.sp_ActualizarVacacionesAniversario();
            }
            catch (Exception ex)
            {
                // Log del error (puedes implementar tu sistema de logging)
                System.Diagnostics.Debug.WriteLine($"Error al actualizar vacaciones: {ex.Message}");
            }
        }

        /// <summary>
        /// Calcula los días que corresponden a un usuario según su antigüedad
        /// </summary>
        public static int CalcularDiasSegunAntiguedad(int idUsuario)
        {
            try
            {
                var usuario = db.tUsuario.FirstOrDefault(u => u.IdUsuario == idUsuario);
                if (usuario == null || !usuario.FechaIngreso.HasValue)
                    return 0;

                int antiguedad = DateTime.Now.Year - usuario.FechaIngreso.Value.Year;

                // Obtener configuración
                var config = db.tConfiguracionVacaciones
                    .Where(c => c.AñosAntiguedad <= antiguedad && c.Estatus == 1)
                    .OrderByDescending(c => c.AñosAntiguedad)
                    .FirstOrDefault();

                return config?.DiasCorresponden ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Verifica si un usuario tiene días disponibles suficientes
        /// </summary>
        public static bool TieneDiasDisponibles(int idUsuario, int diasSolicitados)
        {
            try
            {
                var usuario = db.tUsuario.FirstOrDefault(u => u.IdUsuario == idUsuario);
                if (usuario == null)
                    return false;

                int disponibles = usuario.DiasVacacionesDisponibles ?? 0;
                return disponibles >= diasSolicitados;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Descuenta días de vacaciones a un usuario
        /// </summary>
        public static bool DescontarDias(int idUsuario, int diasADescontar)
        {
            try
            {
                var usuario = db.tUsuario.FirstOrDefault(u => u.IdUsuario == idUsuario);
                if (usuario == null)
                    return false;

                int disponibles = usuario.DiasVacacionesDisponibles ?? 0;

                if (disponibles < diasADescontar)
                    return false;

                usuario.DiasVacacionesDisponibles = disponibles - diasADescontar;
                db.SubmitChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Restaura días de vacaciones (por ejemplo, si se cancela una solicitud autorizada)
        /// </summary>
        public static bool RestaurarDias(int idUsuario, int diasARestaurar)
        {
            try
            {
                var usuario = db.tUsuario.FirstOrDefault(u => u.IdUsuario == idUsuario);
                if (usuario == null)
                    return false;

                int disponibles = usuario.DiasVacacionesDisponibles ?? 0;
                usuario.DiasVacacionesDisponibles = disponibles + diasARestaurar;
                db.SubmitChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}