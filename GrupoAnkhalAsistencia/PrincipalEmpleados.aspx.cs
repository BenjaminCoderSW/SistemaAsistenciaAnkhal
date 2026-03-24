using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class PrincipalEmpleados : System.Web.UI.Page
    {

        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
          ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);


        protected void Page_Load(object sender, EventArgs e)
        {
            // ¿Sesion válida?
            if (SesionState.usuario == null)
            {
                SesionState.usuario = null;
                Response.Redirect("login.aspx");
                return;
            }


            string rolUsuario = SesionState.usuario.tRol.Rol;

            // Aquí pones los roles que SI pueden entrar
            string[] rolesPermitidos = { "Empleado", "Jefe de Planta" };

            if (!rolesPermitidos.Contains(rolUsuario))
            {
                // Si NO tiene rol válido → lo sacamos
                Response.Redirect("login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                // Actualizar estatus de avisos vencidos antes de cargar
                ActualizarAvisosVencidos();
                CargarAvisos();
            }
        }

        private void ActualizarAvisosVencidos()
        {
            try
            {
                // Obtener todos los avisos activos cuya fecha de vigencia ya pasó
                var avisosVencidos = db.tAvisos
                    .Where(a => a.Estatus == true &&
                               a.FechaVigencia.HasValue &&
                               a.FechaVigencia.Value < DateTime.Today)
                    .ToList();

                if (avisosVencidos.Any())
                {
                    // Desactivar todos los avisos vencidos
                    foreach (var aviso in avisosVencidos)
                    {
                        aviso.Estatus = false;
                    }

                    db.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                // Log del error si es necesario
                System.Diagnostics.Debug.WriteLine("Error al actualizar avisos vencidos: " + ex.Message);
            }
        }

        private void CargarAvisos()
        {
            int idEmpleado = SesionState.usuario.IdUsuario;

            try
            {
                var avisos = db.tAvisos
                    .Where(a => a.Estatus == true &&
                               (a.IdUsuario == idEmpleado || a.IdUsuario == null) &&
                               // Solo mostrar avisos vigentes
                               (!a.FechaVigencia.HasValue || a.FechaVigencia.Value >= DateTime.Today))
                    .OrderByDescending(a => a.Fecha)
                    .ToList();

                rptAvisos.DataSource = avisos;
                rptAvisos.DataBind();
            }
            catch (Exception ex)
            {
                // Manejo de errores
                System.Diagnostics.Debug.WriteLine("Error al cargar avisos: " + ex.Message);
            }
        }
    }
}