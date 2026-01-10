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
    public partial class Area : System.Web.UI.Page
    {

        //conexion
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
        ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarArea();

            }
        }

        private void CargarArea()
        {
            var Area = from t in db.tArea
                       select t;

            dvgArea.DataSource = Area.ToList();
            dvgArea.DataBind();
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            tArea pue = new tArea();
            {
                if (string.IsNullOrWhiteSpace(txtArea.Text))
                {
                    string script = "Swal.fire({ icon: 'warning', title: 'Campo obligatorio', text: 'El campo área es obligatorio.' });";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "alertEdificio", script, true);
                    return;
                }




                // Verificar si ya existe la combinación
                bool existe = db.tArea.Any(p =>
                    p.Area == txtArea.Text.Trim()
                );

                if (existe)
                {
                    string script = @"
                                    Swal.fire({
                                        icon: 'error',
                                        title: 'Duplicado',
                                        text: 'Ya existe un registro Puesto.'
                                    }).then(() => {
                                        // Limpiar los campos en el modal
                                        document.getElementById('" + txtArea.ClientID + @"').value = '';
                                    });
                                ";

                    ScriptManager.RegisterStartupScript(this, this.GetType(), "SweetAlert", script, true);
                    return;
                }


                pue.Area = txtArea.Text.Trim();

            }
            ;
            db.tArea.InsertOnSubmit(pue);
            db.SubmitChanges();
            limpiar();

            // Recargar la tabla
            CargarArea();

            // Mostrar notificación de éxito
            string successScript = @"
                                    Swal.fire({
                                        icon: 'success',
                                        title: 'Guardado',
                                        text: 'El puesto se guardó correctamente.',
                                        showConfirmButton: false,
                                        timer: 2000
                                    });
                                ";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "alertSuccess", successScript, true);

        }

        public void limpiar()
        {
            txtArea.Text = "";

        }

        protected void btnGuardarModal_Click(object sender, EventArgs e)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(txtAreaModal.Text))
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(), "alert('El campo Area es obligatorio.');", true);
                return;
            }

            // Guardar cambios
            int id = Convert.ToInt32(hfIdArea.Value);
            var pue = db.tArea.FirstOrDefault(t => t.IdArea == id);

            if (pue != null)
            {
                pue.Area = txtAreaModal.Text;


                db.SubmitChanges();
                CargarArea();

                // Cerrar modal y mostrar mensaje
                ScriptManager.RegisterStartupScript(this, this.GetType(),
                    Guid.NewGuid().ToString(),
                    "$('#modalEditar').modal('hide'); alert('¡El puesto se actualizó correctamente!');",
                    true);
            }
        }

        protected void btnEliminar_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int id = Convert.ToInt32(btn.CommandArgument);

            var pue = db.tArea.FirstOrDefault(t => t.IdArea == id);
            if (pue != null)
            {
                db.tArea.DeleteOnSubmit(pue);
                db.SubmitChanges();
                CargarArea();

                // Mostrar mensaje de éxito
                string script = @"
                                Swal.fire({
                                    icon: 'success',
                                    title: 'Eliminado',
                                    text: 'El puesto  se eliminó correctamente.',
                                    showConfirmButton: false,
                                    timer: 2000
                                });
                            ";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertEliminar", script, true);
            }

        }

        private void CargarAreas(string filtro = "")
        {
            // usa directamente la variable db que ya declaraste arriba
            var query = from t in db.tArea
                        select t;

            if (!string.IsNullOrEmpty(filtro))
            {
                // En LINQ to SQL a veces Contains truena, entonces usamos SqlMethods.Like
                query = query.Where(x => System.Data.Linq.SqlClient.SqlMethods.Like(x.Area, "%" + filtro + "%"));
            }

            dvgArea.DataSource = query.ToList();
            dvgArea.DataBind();
        }

        protected void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            CargarAreas(txtBuscar.Text.Trim());
        }

        protected void dvgArea_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dvgArea.PageIndex = e.NewPageIndex;
            CargarAreas(txtBuscar.Text.Trim()); // mantiene el filtro
        }
    }
}