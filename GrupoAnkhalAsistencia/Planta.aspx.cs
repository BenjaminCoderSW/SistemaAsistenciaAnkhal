using GrupoAnkhalAsistencia.Modelo;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class Planta : System.Web.UI.Page
    {
        //conexion
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
        ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarPlanta();
            }
        }

        private void CargarPlanta()
        {
            var Planta = from t in db.tPlanta
                         select t;

            dvgPlanta.DataSource = Planta.ToList();
            dvgPlanta.DataBind();
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            tPlanta pue = new tPlanta();

            if (string.IsNullOrWhiteSpace(txtPlanta.Text))
            {
                string script = "Swal.fire({ icon: 'warning', title: 'Campo obligatorio', text: 'El campo planta es obligatorio.' });";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertEdificio", script, true);
                return;
            }

            // Verificar si ya existe la planta
            bool existe = db.tPlanta.Any(p => p.Planta == txtPlanta.Text.Trim());

            if (existe)
            {
                string script = @"
                    Swal.fire({
                        icon: 'error',
                        title: 'Duplicado',
                        text: 'Ya existe una planta con ese nombre.'
                    }).then(() => {
                        document.getElementById('" + txtPlanta.ClientID + @"').value = '';
                    });
                ";

                ScriptManager.RegisterStartupScript(this, this.GetType(), "SweetAlert", script, true);
                return;
            }

            pue.Planta = txtPlanta.Text.Trim();
            pue.longitud = txtlongitud.Text.Trim();
            pue.latitud = txtlatitud.Text.Trim();
            pue.IP_INICIO = string.IsNullOrWhiteSpace(txtIPInicio.Text) ? null : txtIPInicio.Text.Trim();
            pue.IP_FIN = string.IsNullOrWhiteSpace(txtIPFin.Text) ? null : txtIPFin.Text.Trim();

            db.tPlanta.InsertOnSubmit(pue);
            db.SubmitChanges();
            limpiar();

            // Recargar la tabla
            CargarPlanta();

            // Mostrar notificación de éxito
            string successScript = @"
                Swal.fire({
                    icon: 'success',
                    title: 'Guardado',
                    text: 'La Planta se guardó correctamente.',
                    showConfirmButton: false,
                    timer: 2000
                });
            ";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "alertSuccess", successScript, true);
        }

        public void limpiar()
        {
            txtPlanta.Text = "";
            txtlongitud.Text = "";
            txtlatitud.Text = "";
            txtIPInicio.Text = "";
            txtIPFin.Text = "";
        }

        protected void btnGuardarModal_Click(object sender, EventArgs e)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(PlantaModal.Text))
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(),
                    "Swal.fire({ icon: 'warning', title: 'Campo obligatorio', text: 'El campo Planta es obligatorio.' });", true);
                return;
            }

            // Guardar cambios
            int id = Convert.ToInt32(hfIdPlanta.Value);
            var pue = db.tPlanta.FirstOrDefault(t => t.IdPlanta == id);

            if (pue != null)
            {
                pue.Planta = PlantaModal.Text.Trim();
                pue.longitud = LongitudModal.Text.Trim();
                pue.latitud = LatitudModal.Text.Trim();
                pue.IP_INICIO = string.IsNullOrWhiteSpace(IPInicioModal.Text) ? null : IPInicioModal.Text.Trim();
                pue.IP_FIN = string.IsNullOrWhiteSpace(IPFinModal.Text) ? null : IPFinModal.Text.Trim();

                db.SubmitChanges();

                // Cerrar modal y mostrar mensaje
                string script = @"
                    $('#modalEditar').modal('hide');
                    Swal.fire({
                        icon: 'success',
                        title: 'Actualizado',
                        text: '¡La planta se actualizó correctamente!',
                        showConfirmButton: false,
                        timer: 2000
                    });
                ";
                ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(), script, true);

                CargarPlantas();
            }
        }

        protected void btnEliminar_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int id = Convert.ToInt32(btn.CommandArgument);

            var pue = db.tPlanta.FirstOrDefault(t => t.IdPlanta == id);
            if (pue != null)
            {
                // Eliminar registro
                db.tPlanta.DeleteOnSubmit(pue);
                db.SubmitChanges();

                CargarPlantas();

                string script = @"
                    Swal.fire({
                        icon: 'success',
                        title: 'Eliminado',
                        text: 'La planta se eliminó correctamente.',
                        showConfirmButton: false,
                        timer: 2000
                    });
                ";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertEliminar", script, true);
            }
        }

        private void CargarPlantas(string filtro = "")
        {
            var query = from t in db.tPlanta
                        select t;

            if (!string.IsNullOrEmpty(filtro))
            {
                query = query.Where(x => System.Data.Linq.SqlClient.SqlMethods.Like(x.Planta, "%" + filtro + "%"));
            }

            dvgPlanta.DataSource = query.ToList();
            dvgPlanta.DataBind();
        }

        protected void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            CargarPlantas(txtBuscar.Text.Trim());
        }

        protected void dvgPlanta_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dvgPlanta.PageIndex = e.NewPageIndex;
            CargarPlantas(txtBuscar.Text.Trim());
        }
    }
}