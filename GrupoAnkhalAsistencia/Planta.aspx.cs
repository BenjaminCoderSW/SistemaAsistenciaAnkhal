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
            {
                if (string.IsNullOrWhiteSpace(txtPlanta.Text))
                {
                    string script = "Swal.fire({ icon: 'warning', title: 'Campo obligatorio', text: 'El campo planta es obligatorio.' });";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "alertEdificio", script, true);
                    return;
                }




                // Verificar si ya existe la combinación
                bool existe = db.tPlanta.Any(p =>
                    p.Planta == txtPlanta.Text.Trim()
                );

                if (existe)
                {
                    string script = @"
                                    Swal.fire({
                                        icon: 'error',
                                        title: 'Duplicado',
                                        text: 'Ya existe un registro horario.'
                                    }).then(() => {
                                        // Limpiar los campos en el modal
                                        document.getElementById('" + txtPlanta.ClientID + @"').value = '';
                                    });
                                ";

                    ScriptManager.RegisterStartupScript(this, this.GetType(), "SweetAlert", script, true);
                    return;
                }

                pue.Planta = txtPlanta.Text.Trim();
                pue.longitud = txtlongitud.Text.Trim();
                pue.latitud = txtlatitud.Text.Trim();
            }
            ;
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

        }

        protected void btnGuardarModal_Click(object sender, EventArgs e)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(PlantaModal.Text))
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(), "alert('El campo Planta es obligatorio.');", true);
                return;
            }

            // Guardar cambios
            int id = Convert.ToInt32(hfIdPlanta.Value);
            var pue = db.tPlanta.FirstOrDefault(t => t.IdPlanta == id);

            if (pue != null)
            {

                
                pue.Planta = PlantaModal.Text;
                pue.longitud =  LongitudModal.Text.Trim();
                pue.latitud = LatitudModal.Text.Trim();
               



                db.SubmitChanges();
                // Cerrar modal y mostrar mensaje
                ScriptManager.RegisterStartupScript(this, this.GetType(),
                    Guid.NewGuid().ToString(),
                    "$('#modalEditar').modal('hide'); alert('¡La planta se actualizó correctamente!');",
                    true);
                CargarPlantas();
            }
        }

       


        private void CargarPlantas(string filtro = "")
        {
            // usa directamente la variable db que ya declaraste arriba
            var query = from t in db.tPlanta
                        select t;

            if (!string.IsNullOrEmpty(filtro))
            {
                // En LINQ to SQL a veces Contains truena, entonces usamos SqlMethods.Like
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
            CargarPlantas(txtBuscar.Text.Trim()); // mantiene el filtro
        }
    }
}