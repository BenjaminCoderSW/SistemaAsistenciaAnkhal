using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Configuration;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class ReporteVacaciones : System.Web.UI.Page
    {
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
            ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (SesionState.usuario == null)
            {
                Response.Redirect("login.aspx");
                return;
            }

            string rolUsuario = SesionState.usuario.tRol.Rol;
            string[] rolesPermitidos = { "Administrador", "Rh" };

            if (!rolesPermitidos.Contains(rolUsuario))
            {
                Response.Redirect("login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                CargarVacaciones();
            }
        }

        private void CargarVacaciones(string filtro = "")
        {
            var query = from v in db.tVacaciones
                        join u in db.tUsuario on v.IdUsuario equals u.IdUsuario
                        join j in db.tJefe on v.IdJefe equals j.IdJefe
                        orderby v.IdVacaciones descending
                        select new
                        {
                            v.IdVacaciones,
                            Empleado = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,
                            Jefe = j.Jefe,
                            v.CorreoJefe,
                            v.FechaInicio,
                            v.FechaFin,
                            v.Dias,
                            EstatusTexto = v.Estatus == 1 ? "Pendiente" :
                                           v.Estatus == 2 ? "Autorizado" :
                                           "Desconocido"
                        };

            if (!string.IsNullOrEmpty(filtro))
            {
                query = query.Where(x =>
                    System.Data.Linq.SqlClient.SqlMethods.Like(x.Empleado, "%" + filtro + "%"));
            }

            dvgVacaciones.DataSource = query.ToList();
            dvgVacaciones.DataBind();
        }

        protected void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            CargarVacaciones(txtBuscar.Text.Trim());
        }

        protected void dvgVacaciones_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dvgVacaciones.PageIndex = e.NewPageIndex;
            CargarVacaciones(txtBuscar.Text.Trim());
        }
    }
}