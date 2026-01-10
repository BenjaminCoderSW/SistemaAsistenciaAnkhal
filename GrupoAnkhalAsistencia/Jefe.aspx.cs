using GrupoAnkhalAsistencia.Modelo;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class Jefe : System.Web.UI.Page
    {
        //conexion
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
        ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarJefe();

            }
        }

        private void CargarJefe()
        {
            var Jefe = from t in db.tJefe
                       where t.Estatus==1
                         select t;

            dvgJefe.DataSource = Jefe.ToList();
            dvgJefe.DataBind();
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            tJefe pue = new tJefe();
            {
                if (string.IsNullOrWhiteSpace(txtJefe.Text))
                {
                    string script = "Swal.fire({ icon: 'warning', title: 'Campo obligatorio', text: 'El campo jefe es obligatorio.' });";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "alertEdificio", script, true);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtEmail.Text))
                {
                    string script = "Swal.fire({ icon: 'warning', title: 'Campo obligatorio', text: 'El campo email es obligatorio.' });";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "alertEdificio", script, true);
                    return;
                }



                // Verificar si ya existe la combinación
                bool existe = db.tJefe.Any(p =>
                    p.Jefe == txtJefe.Text.Trim()
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
                                        document.getElementById('" + txtJefe.ClientID + @"').value = '';
                                    });
                                ";

                    ScriptManager.RegisterStartupScript(this, this.GetType(), "SweetAlert", script, true);
                    return;
                }

                pue.Jefe = txtJefe.Text.Trim();
                pue.Correo = txtEmail.Text.Trim();
                pue.Estatus = 1;
              
            }
            ;
            db.tJefe.InsertOnSubmit(pue);
            db.SubmitChanges();
            limpiar();

            // Recargar la tabla
            CargarJefe();

            // Mostrar notificación de éxito
            string successScript = @"
                                    Swal.fire({
                                        icon: 'success',
                                        title: 'Guardado',
                                        text: 'El Jefe se guardó correctamente.',
                                        showConfirmButton: false,
                                        timer: 2000
                                    });
                                ";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "alertSuccess", successScript, true);

        }

        public void limpiar()
        {

            txtJefe.Text = "";
            txtEmail.Text = "";
            
        }

        protected void btnGuardarModal_Click(object sender, EventArgs e)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(JefeModal.Text))
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(), "alert('El campo Jefe  es obligatorio.');", true);
                return;
            }

            // Guardar cambios
            int id = Convert.ToInt32(hfIdJefe.Value);
            var pue = db.tJefe.FirstOrDefault(t => t.IdJefe == id);

            if (pue != null)
            {


                pue.Jefe = JefeModal.Text;
                pue.Correo = EmailModal.Text.Trim();

                db.SubmitChanges();
                // Cerrar modal y mostrar mensaje
                ScriptManager.RegisterStartupScript(this, this.GetType(),
                    Guid.NewGuid().ToString(),
                    "$('#modalEditar').modal('hide'); alert('¡El jefe se actualizó correctamente!');",
                    true);
                CargarJefes();
            }
        }




        private void CargarJefes(string filtro = "")
        {
            // usa directamente la variable db que ya declaraste arriba
            var query = from t in db.tJefe
                        select t;

            if (!string.IsNullOrEmpty(filtro))
            {
                // En LINQ to SQL a veces Contains truena, entonces usamos SqlMethods.Like
                query = query.Where(x => System.Data.Linq.SqlClient.SqlMethods.Like(x.Jefe, "%" + filtro + "%"));
            }

            dvgJefe.DataSource = query.ToList();
            dvgJefe.DataBind();
        }

        protected void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            CargarJefes(txtBuscar.Text.Trim());
        }



        protected void btnEliminar_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int id = Convert.ToInt32(btn.CommandArgument);

            var pue = db.tJefe.FirstOrDefault(t => t.IdJefe == id);
            if (pue != null)
            {
                // Cambiar el estatus a 0 en lugar de eliminar
                pue.Estatus = 0;

                db.SubmitChanges();
                CargarJefe();

                // Mostrar mensaje de éxito
                string script = @"
            Swal.fire({
                icon: 'success',
                title: 'Desactivado',
                text: 'El jefe se desactivó correctamente.',
                showConfirmButton: false,
                timer: 2000
            });
        ";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertDesactivar", script, true);
            }
        }

        protected void dvgJefe_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dvgJefe.PageIndex = e.NewPageIndex;
            CargarJefes(txtBuscar.Text.Trim()); // mantiene el filtro
        }
    }
}