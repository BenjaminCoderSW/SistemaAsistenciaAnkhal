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
    public partial class ReporteComisionesHora : System.Web.UI.Page
    {
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
        ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarComisionHorarios();

            }
        }

        private void CargarComisionHorarios()
        {
            var usuario = from m in db.tComisionHoras
                          join r in db.tUsuario on m.IdUsuario equals r.IdUsuario
                          join p in db.tJefe on m.IdJefe equals p.IdJefe
                          where m.Estatus == 2
                          orderby m.IdComisonHoras
                          select new
                          {
                              m.IdComisonHoras,
                              m.IdUsuario,
                              m.IdJefe,
                              Empleado = r.Nombre + " " + r.ApellidoPaterno + " " + r.ApellidoMaterno,
                              Jefe = p.Jefe,
                              m.CorreoJefe,
                              m.Motivo,
                              m.Destino,
                              m.Fecha,
                              m.HoraSalida,
                              m.HoraRegreso,
                              m.Horas,
                              m.Observaciones,

                              // ✅ Texto limpio para el Grid
                              EstatusTexto =
                                  m.Estatus == 1 ? "Pendiente" :
                                  m.Estatus == 2 ? "Autorizado" :
                                  "Desconocido"
                          };

            dvgComisionHoras.DataSource = usuario.ToList();
            dvgComisionHoras.DataBind();
        }



        protected void btnEliminar_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int id = Convert.ToInt32(btn.CommandArgument);

            var pue = db.tComisionHoras.FirstOrDefault(t => t.IdComisonHoras == id);
            if (pue != null)
            {
                // ✅ Eliminar registro real
                db.tComisionHoras.DeleteOnSubmit(pue);
                db.SubmitChanges();

                CargarComisionHorarios();

                string script = @"
            Swal.fire({
                icon: 'success',
                title: 'Eliminado',
                text: 'La comisión por hora se eliminó correctamente.',
                showConfirmButton: false,
                timer: 2000
            });
        ";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertEliminar", script, true);
            }
        }


        private void CargarComisionHorario(string filtro = "")
        {
            var query = from t in db.tComisionHoras
                        join u in db.tUsuario on t.IdUsuario equals u.IdUsuario
                        join p in db.tJefe on t.IdJefe equals p.IdJefe
                        where t.Estatus == 2
                        select new
                        {
                            t.IdComisonHoras,
                            t.IdUsuario,
                            t.IdJefe,
                            Empleado = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,
                            Jefe = p.Jefe,
                            t.CorreoJefe,
                            t.Motivo,
                            t.Destino,
                            t.Fecha,
                            t.HoraSalida,
                            t.HoraRegreso,
                            t.Horas,
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

            dvgComisionHoras.DataSource = query.ToList();
            dvgComisionHoras.DataBind();
        }

        protected void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            CargarComisionHorario(txtBuscar.Text.Trim());
        }

        protected void dvgComisionHoras_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dvgComisionHoras.PageIndex = e.NewPageIndex;
            CargarComisionHorario(txtBuscar.Text.Trim()); // mantiene el filtro
        }
    }
}