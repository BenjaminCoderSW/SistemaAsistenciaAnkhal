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
    public partial class Avisos : System.Web.UI.Page
    {

        dbAsistenciaDataContext db = new dbAsistenciaDataContext(
           ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarUsuarios();
            }
        }

        private void CargarUsuarios()
        {
            var usuarios = db.tUsuario
                .Select(u => new { u.IdUsuario, Nombre = u.Nombre + " " + u.ApellidoPaterno })
                .ToList();

            ddlUsuario.DataSource = usuarios;
            ddlUsuario.DataTextField = "Nombre";
            ddlUsuario.DataValueField = "IdUsuario";
            ddlUsuario.DataBind();

            // Opción general
            ddlUsuario.Items.Insert(0, new System.Web.UI.WebControls.ListItem(
                "Aviso general (todos los empleados)", "0"));
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                tAvisos aviso = new tAvisos();
                aviso.Titulo = txtTitulo.Text;
                aviso.Mensaje = txtMensaje.Text;
                aviso.Importancia = ddlImportancia.SelectedValue;
                aviso.Estatus = true;
                aviso.Fecha = DateTime.Now;

                if (ddlUsuario.SelectedValue == "0")
                {
                    aviso.IdUsuario = null; // Aviso general
                }
                else
                {
                    aviso.IdUsuario = Convert.ToInt32(ddlUsuario.SelectedValue);
                }

                db.tAvisos.InsertOnSubmit(aviso);
                db.SubmitChanges();

                MostrarOk("Aviso guardado correctamente.");
            }
            catch (Exception)
            {
                MostrarError("Hubo un problema al guardar el aviso.");
            }
        }

        private void MostrarOk(string mensaje)
        {
            string script = $@"Swal.fire({{ icon: 'success', title: 'Éxito', text: '{mensaje}' }});";
            ScriptManager.RegisterStartupScript(this, GetType(), "ok", script, true);
        }

        private void MostrarError(string mensaje)
        {
            string script = $@"Swal.fire({{ icon: 'error', title: 'Error', text: '{mensaje}' }});";
            ScriptManager.RegisterStartupScript(this, GetType(), "err", script, true);
        }
    }
}