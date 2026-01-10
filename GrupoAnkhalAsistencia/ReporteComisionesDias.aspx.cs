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
    public partial class ReporteComisionesDias : System.Web.UI.Page
    {
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
      ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarComisionDias();

            }
        }

        private void CargarComisionDias()
        {
            var usuario = from m in db.tComisionDia
                          join r in db.tUsuario on m.IdUsuario equals r.IdUsuario
                          join p in db.tJefe on m.IdJefe equals p.IdJefe
                          where m.Estatus == 2
                          orderby m.IdComisionDia
                          select new
                          {
                              m.IdComisionDia,
                              m.IdUsuario,
                              m.IdJefe,
                              Empleado = r.Nombre + " " + r.ApellidoPaterno + " " + r.ApellidoMaterno,
                              Jefe = p.Jefe,
                              m.CorreoJefe,
                              m.Motivo,
                              m.Destino,
                              m.FechaSalida,
                              m.FechaRegreso,
                              m.Dias,
                              m.Hospedaje,
                              m.Transporte,
                              m.Observaciones,

                              // ✅ Texto limpio para el Grid
                              EstatusTexto =
                                  m.Estatus == 1 ? "Pendiente" :
                                  m.Estatus == 2 ? "Autorizado" :
                                  "Desconocido"
                          };

            dvgComisionDias.DataSource = usuario.ToList();
            dvgComisionDias.DataBind();
        }


        protected void btnEliminar_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int id = Convert.ToInt32(btn.CommandArgument);

            var pue = db.tComisionDia.FirstOrDefault(t => t.IdComisionDia == id);
            if (pue != null)
            {
                // ✅ Eliminar registro real
                db.tComisionDia.DeleteOnSubmit(pue);
                db.SubmitChanges();

                CargarComisionDias();

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


        private void CargarComisionDia(string filtro = "")
        {
            var query = from t in db.tComisionDia
                        join u in db.tUsuario on t.IdUsuario equals u.IdUsuario
                        join p in db.tJefe on t.IdJefe equals p.IdJefe
                        where t.Estatus == 2
                        select new
                        {
                            t.IdComisionDia,
                            t.IdUsuario,
                            t.IdJefe,
                            Empleado = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,
                            Jefe = p.Jefe,
                            t.CorreoJefe,
                            t.Motivo,
                            t.Destino,
                            t.FechaSalida,
                            t.FechaRegreso,
                            t.Dias,
                            t.Hospedaje,
                            t.Transporte,
                            t.Observaciones,

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

            dvgComisionDias.DataSource = query.ToList();
            dvgComisionDias.DataBind();
        }

        protected void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            CargarComisionDia(txtBuscar.Text.Trim());
        }

        protected void dvgComisionDias_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dvgComisionDias.PageIndex = e.NewPageIndex;
            CargarComisionDia(txtBuscar.Text.Trim());
        }
    }
}