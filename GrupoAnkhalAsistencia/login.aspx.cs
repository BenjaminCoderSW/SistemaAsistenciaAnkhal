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
                    SesionState.usuario = usuarioDb;
                    this.user = true;



                    string rolPantalla = SesionState.usuario.tRol.Rol;


                    if (rolPantalla == "Administrador" || rolPantalla == "Rh")
                    {
                        // Redirigir sin ThreadAbortException
                        Response.Redirect("PrincipalAdmin.aspx", false);
                        Context.ApplicationInstance.CompleteRequest();
                    }
                    else if (rolPantalla == "Empleado")
                    {
                        // Redirigir sin ThreadAbortException
                        Response.Redirect("PrincipalEmpleados.aspx", false);
                        Context.ApplicationInstance.CompleteRequest();
                       
                    }



                    // Redirigir sin ThreadAbortException
                    //Response.Redirect("PrincipalAdmin.aspx", false);
                    //Context.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    MostrarError("No existe", "El usuario no existe. Revisa la información.");
                }
            }
            catch (Exception)
            {
                MostrarError("Error", "Contacta al Administrador del sistema.");
            }
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