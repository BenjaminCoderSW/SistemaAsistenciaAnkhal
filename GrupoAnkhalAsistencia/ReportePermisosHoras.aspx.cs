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
    public partial class ReportePermisosHoras : System.Web.UI.Page
    {

        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
        ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarPermisosHoras();

            }
        }

        private void CargarPermisosHoras()
        {
            var usuario = from m in db.tPermisoHora
                          join r in db.tUsuario on m.IdUsuario equals r.IdUsuario
                          join p in db.tJefe on m.IdJefe equals p.IdJefe
                          where m.Estatus == 2
                          orderby m.IdPermisoHora
                          select new
                          {
                              m.IdPermisoHora,
                              m.IdUsuario,
                              m.IdJefe,
                              Empleado = r.Nombre + " " + r.ApellidoPaterno + " " + r.ApellidoMaterno,
                              Jefe = p.Jefe,
                              m.CorreoJefe,
                              m.Motivo,
                              m.TipoPermiso,
                              m.HoraInicio,
                              m.HoraFin,
                              m.Horas,
                              m.Dia,

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

            var pue = db.tPermisoHora.FirstOrDefault(t => t.IdPermisoHora == id);
            if (pue != null)
            {
                // ✅ Eliminar registro real
                db.tPermisoHora.DeleteOnSubmit(pue);
                db.SubmitChanges();

                CargarPermisosHoras();

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
            var query = from t in db.tPermisoHora
                        join u in db.tUsuario on t.IdUsuario equals u.IdUsuario
                        join p in db.tJefe on t.IdJefe equals p.IdJefe
                        where t.Estatus == 2
                        select new
                        {
                            t.IdPermisoHora,
                            t.IdUsuario,
                            t.IdJefe,
                            Empleado = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,
                            Jefe = p.Jefe,
                            t.CorreoJefe,
                            t.Motivo,
                            t.TipoPermiso,
                            t.HoraInicio,
                            t.HoraFin,
                            t.Horas,
                            t.Dia,
                            // ✅ Texto limpio para el Grid
                            EstatusTexto =
                                  t.Estatus == 1 ? "Pendiente" :
                                  t.Estatus == 2 ? "Autorizado" :
                                  "Desconocido"
                        };

            if (!string.IsNullOrEmpty(filtro))
            {
                query = query.Where(x =>
                    System.Data.Linq.SqlClient.SqlMethods.Like(x.Motivo, "%" + filtro + "%") ||
                    System.Data.Linq.SqlClient.SqlMethods.Like(x.Empleado, "%" + filtro + "%")
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