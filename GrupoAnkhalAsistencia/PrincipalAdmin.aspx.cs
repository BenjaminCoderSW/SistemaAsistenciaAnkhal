using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class PrincipalAdmin : System.Web.UI.Page
    {
        dbAsistenciaDataContext db = new dbAsistenciaDataContext(
           System.Configuration.ConfigurationManager
           .ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

      
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
            string[] rolesPermitidos = { "Administrador", "Rh" };

            if (!rolesPermitidos.Contains(rolUsuario))
            {
                // Si NO tiene rol válido → lo sacamos
                Response.Redirect("login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                CargarDashboard();
                CargarAsistenciaHoy();
            }
        }

        private void CargarDashboard()
        {
            DateTime hoy = DateTime.Today;

            // Total empleados registrados en el sistema
            int totalEmpleados = db.tUsuario.Count();

            // Llegaron a tiempo
            int llegaronTiempo = db.tAsistencia
                .Where(a => a.Fecha == hoy && a.EstatusEntrada == "A TIEMPO")
                .Count();

            // Llegaron tarde
            int llegaronTarde = db.tAsistencia
                .Where(a => a.Fecha == hoy && a.EstatusEntrada == "RETARDO")
                .Count();

            // Faltaron = empleados que NO tienen asistencia hoy
            int faltaron =
                totalEmpleados -
                db.tAsistencia.Where(a => a.Fecha == hoy).Select(a => a.IdUsuario).Distinct().Count();

            // Asignar a los labels
            lblTotalEmpleados.Text = totalEmpleados.ToString();
            lblLlegaronTiempo.Text = llegaronTiempo.ToString();
            lblLlegaronTarde.Text = llegaronTarde.ToString();
            lblFaltaron.Text = faltaron.ToString();
        }

        private void CargarAsistenciaHoy()
        {
            DateTime hoy = DateTime.Today;

            var asistencia = from m in db.V_REPORTE_ASISTENCIA
                             where m.Fecha == hoy
                             orderby m.HoraEntrada
                             select new
                             {
                                 m.EMPLEADO,
                                 m.Planta,
                                 m.Fecha,
                                 m.HoraEntrada,
                                 m.HoraSalida,
                                 m.EstatusEntrada
                             };

            gvAsistenciaHoy.DataSource = asistencia.ToList();
            gvAsistenciaHoy.DataBind();
        }
    }

}