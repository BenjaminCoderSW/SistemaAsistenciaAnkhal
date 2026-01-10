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
    public partial class Puesto : System.Web.UI.Page
    {
        //conexion
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
        ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarPuesto();

            }
        }

        private void CargarPuesto()
        {
            var Puesto = from t in db.tPuesto
                         select t;

            dvgPuesto.DataSource = Puesto.ToList();
            dvgPuesto.DataBind();
        }


        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            tPuesto pue = new tPuesto();
            {
                if (string.IsNullOrWhiteSpace(txtPuesto.Text))
                {
                    string script = "Swal.fire({ icon: 'warning', title: 'Campo obligatorio', text: 'El campo puesto es obligatorio.' });";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "alertEdificio", script, true);
                    return;
                }




                // Verificar si ya existe la combinación
                bool existe = db.tPuesto.Any(p =>
                    p.Puesto == txtPuesto.Text.Trim()
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
                                        document.getElementById('" + txtPuesto.ClientID + @"').value = '';
                                    });
                                ";

                    ScriptManager.RegisterStartupScript(this, this.GetType(), "SweetAlert", script, true);
                    return;
                }


                pue.Puesto = txtPuesto.Text.Trim();

            }
          ;

            db.tPuesto.InsertOnSubmit(pue);
            db.SubmitChanges();
            limpiar();

            // Recargar la tabla
            CargarPuesto();

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
            txtPuesto.Text = "";

        }

        protected void btnGuardarModal_Click(object sender, EventArgs e)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(txtPuestoModal.Text))
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(), "alert('El campo Puesto es obligatorio.');", true);
                return;
            }

            // Guardar cambios
            int id = Convert.ToInt32(hfIdPuesto.Value);
            var pue = db.tPuesto.FirstOrDefault(t => t.IdPuesto == id);

            if (pue != null)
            {
                pue.Puesto = txtPuestoModal.Text;


                db.SubmitChanges();
                CargarPuesto();

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

            var pue = db.tPuesto.FirstOrDefault(t => t.IdPuesto == id);
            if (pue != null)
            {
                db.tPuesto.DeleteOnSubmit(pue);
                db.SubmitChanges();
                CargarPuesto();

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

        protected void btnBuscar_Click(object sender, EventArgs e)
        {
            CargarPuestos(txtBuscar.Text.Trim());
        }

        private void CargarPuestos(string filtro = "")
        {
            // usa directamente la variable db que ya declaraste arriba
            var query = from t in db.tPuesto
                        select t;

            if (!string.IsNullOrEmpty(filtro))
            {
                // En LINQ to SQL a veces Contains truena, entonces usamos SqlMethods.Like
                query = query.Where(x => System.Data.Linq.SqlClient.SqlMethods.Like(x.Puesto, "%" + filtro + "%"));
            }

            dvgPuesto.DataSource = query.ToList();
            dvgPuesto.DataBind();
        }

        protected void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            CargarPuestos(txtBuscar.Text.Trim());
        }

        protected void dvgPuesto_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dvgPuesto.PageIndex = e.NewPageIndex;
            CargarPuestos(txtBuscar.Text.Trim()); // mantiene el filtro
        }
    }
}