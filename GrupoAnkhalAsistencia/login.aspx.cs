using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class login : System.Web.UI.Page
    {

        //conexion
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
        ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        public bool user { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {

            }
        }

        protected void btnIngresar_Click(object sender, EventArgs e)
        {
            try
            {
                var usuarioDb = db.tUsuario.FirstOrDefault(c => c.Clave == txtcontrasena.Text && c.Usuario == txtusuario.Text);

                if (usuarioDb != null)
                {
                    // ✅ Asignar a la sesión
                    SesionState.usuario = usuarioDb;

                    string rolPantalla = SesionState.usuario.tRol.Rol;

                    if (rolPantalla == "Administrador" || rolPantalla == "Rh")
                    {
                        Response.Redirect("PrincipalAdmin.aspx", false);
                        Context.ApplicationInstance.CompleteRequest();
                    }
                    else if (rolPantalla == "Empleado")
                    {
                        Response.Redirect("PrincipalEmpleados.aspx", false);
                        Context.ApplicationInstance.CompleteRequest();
                    }
                }
                else
                {
                    MostrarError("No existe", "El usuario no existe. Revisa la información.");
                }
            }
            catch (Exception ex)
            {
                // ✅ AGREGA ESTO PARA VER EL ERROR EXACTO
                MostrarError("Error", "Error: " + ex.Message + " - " + ex.StackTrace + "Contacta al administrador del sistema.");
            }
        }
        protected void btnIrChecador_Click(object sender, EventArgs e)
        {
            Response.Redirect("Checar.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void MostrarError(string titulo, string mensaje)
        {
            string script = $@"
        Swal.fire({{
            icon: 'error',
            title: '{titulo}',
            text: '{mensaje}'
        }}).then(() => {{
            document.getElementById('{txtusuario.ClientID}').value = '';
            document.getElementById('{txtcontrasena.ClientID}').value = '';
        }});";

            ScriptManager.RegisterStartupScript(this, this.GetType(), "SweetAlert", script, true);
        }
    }
}