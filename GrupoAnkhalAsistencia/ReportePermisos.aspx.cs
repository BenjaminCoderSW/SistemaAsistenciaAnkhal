using GrupoAnkhalAsistencia.Modelo;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class ReportePermisos : System.Web.UI.Page
    {
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
        ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarPermisosDias();

            }
        }

        private void CargarPermisosDias()
        {
            var usuario = from m in db.tPermisoDias
                          join r in db.tUsuario on m.IdUsuario equals r.IdUsuario
                          join p in db.tJefe on m.IdJefe equals p.IdJefe
                          where m.Estatus == 2
                          orderby m.IdPermisoDias
                          select new
                          {
                              m.IdPermisoDias,
                              m.IdUsuario,
                              m.IdJefe,
                              Empleado = r.Nombre + " " + r.ApellidoPaterno + " " + r.ApellidoMaterno,
                              Jefe = p.Jefe,
                              m.CorreoJefe,
                              m.Motivo,
                              m.TipoPermiso,
                              m.FechaInicio,
                              m.FechaFin,
                              m.Dias,
                              m.Observaciones,

                              // ✅ Texto limpio para el Grid
                              EstatusTexto =
                                  m.Estatus == 1 ? "Pendiente" :
                                  m.Estatus == 2 ? "Autorizado" :
                                  "Desconocido"
                          };

            dvgPermiso.DataSource = usuario.ToList();
            dvgPermiso.DataBind();
        }

        protected void btnEliminar_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int id = Convert.ToInt32(btn.CommandArgument);

            var pue = db.tPermisoDias.FirstOrDefault(t => t.IdPermisoDias == id);
            if (pue != null)
            {
                // ✅ Eliminar registro real
                db.tPermisoDias.DeleteOnSubmit(pue);
                db.SubmitChanges();

                CargarPermisosDias();

                string script = @"
            Swal.fire({
                icon: 'success',
                title: 'Eliminado',
                text: 'El permiso por hora se eliminó correctamente.',
                showConfirmButton: false,
                timer: 2000
            });
        ";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertEliminar", script, true);
            }
        }


        private void CargarPermisoDia(string filtro = "")
        {
            var query = from t in db.tPermisoDias
                        join u in db.tUsuario on t.IdUsuario equals u.IdUsuario
                        where t.Estatus == 2
                        select new
                        {
                            t.IdPermisoDias,
                            t.Dias,
                            t.FechaInicio,
                            t.FechaFin,
                            t.Motivo,
                            t.TipoPermiso,
                            nombrecompleto = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,   // Nombre del empleado
                            t.Observaciones
                        };

            if (!string.IsNullOrEmpty(filtro))
            {
                query = query.Where(x =>
                    System.Data.Linq.SqlClient.SqlMethods.Like(x.Motivo, "%" + filtro + "%") ||
                    System.Data.Linq.SqlClient.SqlMethods.Like(x.nombrecompleto, "%" + filtro + "%")
                );
            }

            dvgPermiso.DataSource = query.ToList();
            dvgPermiso.DataBind();
        }

        protected void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            CargarPermisoDia(txtBuscar.Text.Trim());
        }

        protected void dvgPermiso_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dvgPermiso.PageIndex = e.NewPageIndex;
            CargarPermisoDia(txtBuscar.Text.Trim()); // mantiene el filtro
        }
    }
}