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
                // Establecer fecha mínima como hoy
                txtFechaVigencia.Attributes["min"] = DateTime.Today.ToString("yyyy-MM-dd");
            }
        }

        private void CargarUsuarios()
        {
            var usuarios = db.tUsuario
                .Where(u => u.Estatus == 1) // Solo usuarios activos
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
                // Validaciones
                if (string.IsNullOrWhiteSpace(txtTitulo.Text))
                {
                    MostrarError("Por favor ingrese un título para el aviso.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtMensaje.Text))
                {
                    MostrarError("Por favor ingrese un mensaje para el aviso.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtFechaVigencia.Text))
                {
                    MostrarError("Por favor seleccione una fecha de vigencia.");
                    return;
                }

                DateTime fechaVigencia = DateTime.Parse(txtFechaVigencia.Text);

                // Validar que la fecha de vigencia no sea anterior a hoy
                if (fechaVigencia < DateTime.Today)
                {
                    MostrarError("La fecha de vigencia no puede ser anterior a hoy.");
                    return;
                }

                tAvisos aviso = new tAvisos();
                aviso.Titulo = txtTitulo.Text.Trim();
                aviso.Mensaje = txtMensaje.Text.Trim();
                aviso.Importancia = ddlImportancia.SelectedValue;
                aviso.Estatus = true;
                aviso.Fecha = DateTime.Now;
                aviso.FechaVigencia = fechaVigencia;

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

                // Limpiar campos después de guardar
                LimpiarCampos();

                MostrarOk("Aviso guardado correctamente y será visible hasta el " +
                         fechaVigencia.ToString("dd/MM/yyyy"));
            }
            catch (Exception ex)
            {
                MostrarError("Hubo un problema al guardar el aviso: " + ex.Message);
            }
        }

        private void LimpiarCampos()
        {
            txtTitulo.Text = string.Empty;
            txtMensaje.Text = string.Empty;
            txtFechaVigencia.Text = string.Empty;
            ddlImportancia.SelectedIndex = 0;
            ddlUsuario.SelectedIndex = 0;
        }

        private void MostrarOk(string mensaje)
        {
            string script = $@"
                Swal.fire({{
                    icon: 'success',
                    title: 'Éxito',
                    text: '{mensaje}',
                    confirmButtonText: 'Aceptar'
                }});";
            ScriptManager.RegisterStartupScript(this, GetType(), "ok", script, true);
        }

        private void MostrarError(string mensaje)
        {
            string script = $@"
                Swal.fire({{
                    icon: 'error',
                    title: 'Error',
                    text: '{mensaje}',
                    confirmButtonText: 'Aceptar'
                }});";
            ScriptManager.RegisterStartupScript(this, GetType(), "err", script, true);
        }
    }
}