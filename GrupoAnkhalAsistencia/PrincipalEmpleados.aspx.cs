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

           
            string rolUsuario = SesionState.usuario.tRol.Rol;  // ajusta al nombre que tengas en tu clase

            // Aquí pones los roles que SI pueden entrar
            string[] rolesPermitidos = { "Empleado" };

            if (!rolesPermitidos.Contains(rolUsuario))
            {
                // Si NO tiene rol válido → lo sacamos
                Response.Redirect("login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                CargarAvisos();
            }
        }

        private void CargarAvisos()
        {
            int idEmpleado = SesionState.usuario.IdUsuario;

            
                var avisos = db.tAvisos
                    .Where(a => a.Estatus == true &&
                               (a.IdUsuario == idEmpleado || a.IdUsuario == null))
                    .OrderByDescending(a => a.Fecha)
                    .ToList();
                rptAvisos.DataSource = avisos;
                rptAvisos.DataBind();
            
        }
    }
}