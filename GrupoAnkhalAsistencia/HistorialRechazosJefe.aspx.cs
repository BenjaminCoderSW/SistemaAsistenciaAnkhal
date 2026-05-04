using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Configuration;
using System.Linq;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class HistorialRechazosJefe : System.Web.UI.Page
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

            string rol = SesionState.usuario.tRol.Rol;
            if (rol != "Administrador" && rol != "Rh")
            {
                Response.Redirect("login.aspx");
                return;
            }

            if (!IsPostBack)
                CargarHistorial();
        }

        private void CargarHistorial(string filtro = "")
        {
            var query = from v in db.tVacaciones
                        join u in db.tUsuario on v.IdUsuario equals u.IdUsuario
                        join p in db.tPlanta on u.IdPlanta equals p.IdPlanta into pj
                        from p in pj.DefaultIfEmpty()
                        join ap in db.tUsuario on v.IdAprobadorJefe equals ap.IdUsuario into apj
                        from ap in apj.DefaultIfEmpty()
                        join rh in db.tUsuario on v.IdAprobadorRH equals rh.IdUsuario into rhj
                        from rh in rhj.DefaultIfEmpty()
                        where v.EstatusJefe == 2
                        orderby v.FechaResolucionJefe descending
                        select new
                        {
                            Empleado = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,
                            Planta = p != null ? p.Planta : "N/A",
                            v.FechaInicio,
                            v.FechaFin,
                            v.Dias,
                            v.FechaSolicitud,
                            JefePlanta = ap != null
                                ? ap.Nombre + " " + ap.ApellidoPaterno
                                : "N/A",
                            FechaRechazoJefe = v.FechaResolucionJefe,
                            MotivoJefe = v.MotivoJefe ?? "",
                            DecisionFinalRH = v.FechaResolucionRH == null
                                ? "Pendiente"
                                : v.Estatus == 2
                                    ? (rh != null ? rh.Nombre + " " + rh.ApellidoPaterno + " Aprobó" : "RH Aprobó")
                                    : (rh != null ? rh.Nombre + " " + rh.ApellidoPaterno + " Confirmó rechazo" : "RH Confirmó rechazo")
                        };

            if (!string.IsNullOrWhiteSpace(filtro))
                query = query.Where(x =>
                    System.Data.Linq.SqlClient.SqlMethods.Like(x.Empleado, "%" + filtro + "%"));

            gvHistorial.DataSource = query.ToList();
            gvHistorial.DataBind();
        }

        protected void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            CargarHistorial(txtBuscar.Text.Trim());
        }

        protected void gvHistorial_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvHistorial.PageIndex = e.NewPageIndex;
            CargarHistorial(txtBuscar.Text.Trim());
        }
    }
}
