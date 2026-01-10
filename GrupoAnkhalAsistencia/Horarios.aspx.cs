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
    public partial class Horarios : System.Web.UI.Page
    {
        //conexion
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
        ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarHorario();

            }
        }

        private void CargarHorario()
        {
            var Horario = from t in db.tHorario
                          where t.Estatus == 1
                          select t;

            dvgHorario.DataSource = Horario.ToList();
            dvgHorario.DataBind();
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            tHorario pue = new tHorario();
            {
                if (string.IsNullOrWhiteSpace(txtDescripcion.Text))
                {
                    string script = "Swal.fire({ icon: 'warning', title: 'Campo obligatorio', text: 'El campo descripcion es obligatorio.' });";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "alertEdificio", script, true);
                    return;
                }




                // Verificar si ya existe la combinación
                bool existe = db.tHorario.Any(p =>
                    p.Descripcion == txtDescripcion.Text.Trim()
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
                                        document.getElementById('" + txtDescripcion.ClientID + @"').value = '';
                                    });
                                ";

                    ScriptManager.RegisterStartupScript(this, this.GetType(), "SweetAlert", script, true);
                    return;
                }

                pue.HoraInicio = TimeSpan.Parse(txtHoraInicio.Value);
                pue.HoraFin = TimeSpan.Parse(txtHoraFin.Value);
                pue.Descripcion = txtDescripcion.Text.Trim();
                pue.Estatus = 1;

            }
            ;
            db.tHorario.InsertOnSubmit(pue);
            db.SubmitChanges();
            limpiar();

            // Recargar la tabla
            CargarHorario();

            // Mostrar notificación de éxito
            string successScript = @"
                                    Swal.fire({
                                        icon: 'success',
                                        title: 'Guardado',
                                        text: 'El horario se guardó correctamente.',
                                        showConfirmButton: false,
                                        timer: 2000
                                    });
                                ";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "alertSuccess", successScript, true);

        }

        public void limpiar()
        {

            txtHoraInicio.TemplateControl = this;
            txtHoraFin.TemplateControl = this;
            txtDescripcion.Text = "";

        }

        protected void btnGuardarModal_Click(object sender, EventArgs e)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(DescripcionModal.Text))
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(), "alert('El campo Area es obligatorio.');", true);
                return;
            }

            // Guardar cambios
            int id = Convert.ToInt32(hfIdHorario.Value);
            var pue = db.tHorario.FirstOrDefault(t => t.IdHorario == id);

            if (pue != null)
            {
                pue.HoraInicio = TimeSpan.Parse(HorarioInicioModal.Value);
                pue.HoraFin= TimeSpan.Parse(HorarioFinModal.Value);
                pue.Descripcion = DescripcionModal.Text;


                db.SubmitChanges();
                // Cerrar modal y mostrar mensaje
                ScriptManager.RegisterStartupScript(this, this.GetType(),
                    Guid.NewGuid().ToString(),
                    "$('#modalEditar').modal('hide'); alert('¡El puesto se actualizó correctamente!');",
                    true);
                CargarHorario();
            }
        }

        protected void btnEliminar_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int id = Convert.ToInt32(btn.CommandArgument);

            var pue = db.tHorario.FirstOrDefault(t => t.IdHorario == id);
            if (pue != null)
            {
                // Cambiar el estatus a 0 en lugar de eliminar
                pue.Estatus = 0;

                db.SubmitChanges();
                CargarHorario();

                // Mostrar mensaje de éxito
                string script = @"
            Swal.fire({
                icon: 'success',
                title: 'Desactivado',
                text: 'El horario se desactivó correctamente.',
                showConfirmButton: false,
                timer: 2000
            });
        ";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertDesactivar", script, true);
            }
        }


        private void CargarHorario(string filtro = "")
        {
            // usa directamente la variable db que ya declaraste arriba
            var query = from t in db.tHorario
                        select t;

            if (!string.IsNullOrEmpty(filtro))
            {
                // En LINQ to SQL a veces Contains truena, entonces usamos SqlMethods.Like
                query = query.Where(x => System.Data.Linq.SqlClient.SqlMethods.Like(x.Descripcion, "%" + filtro + "%"));
            }

            dvgHorario.DataSource = query.ToList();
            dvgHorario.DataBind();
        }

        protected void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            CargarHorario(txtBuscar.Text.Trim());
        }

        protected void dvgHorario_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dvgHorario.PageIndex = e.NewPageIndex;
            CargarHorario(txtBuscar.Text.Trim()); // mantiene el filtro
        }
    }
}