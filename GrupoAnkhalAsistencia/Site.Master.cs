using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class Site : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {

                if (SesionState.usuario != null)
                {
                    lblUsuario.Text = SesionState.usuario.Nombre + " " + SesionState.usuario.ApellidoPaterno;

                    // Obtén el rol (ajusta al nombre de tu propiedad real en usuario)
                    string rol = SesionState.usuario.tRol.Rol;


                    // Obtiene el varbinary de la BD
                    var fotoBin = SesionState.usuario.Foto; // Tipo Binary


                    if (fotoBin != null)
                    {
                        byte[] fotoBytes = fotoBin.ToArray();

                        if (fotoBytes != null && fotoBytes.Length > 0)
                        {
                            string base64 = Convert.ToBase64String(fotoBytes);
                            imgUsuario.Src = "data:image/png;base64," + base64;
                        }
                        else
                        {
                            imgUsuario.Src = "dist/img/user2-160x160.jpg";
                        }
                    }
                    else
                    {
                        imgUsuario.Src = "dist/img/user2-160x160.jpg";
                    }

                    switch (rol)
                    {
                        case "Administrador":
                            // Ve todo
                            menuAdmAcessos.Visible = true;
                            menuAdmAprovaciones.Visible = true;
                            menuAdmAsistencia.Visible = true;
                            menuAdmGraficas.Visible = true;
                            menuAdminHorario.Visible = true;
                            menuAdmReportes.Visible = true;
                            menuAdmVacaciones.Visible = true;
                            lnkInicio.Visible = true;


                            break;

                        case "Rh":
                            // Solo ve Directorio
                            menuAdmAcessos.Visible = true;
                            menuAdmAprovaciones.Visible = true;
                            menuAdmAsistencia.Visible = true;
                            menuAdmGraficas.Visible = true;
                            menuAdminHorario.Visible = true;
                            menuAdmReportes.Visible = true;
                            menuAdmVacaciones.Visible = true;
                            lnkInicio.Visible = false;

                            break;

                        case "Empleado":
                            // Solo ve Directorio
                            menuAdmAcessos.Visible = false;
                            menuAdmAprovaciones.Visible = false;
                            menuAdmAsistencia.Visible = true;
                            menuAdmGraficas.Visible = false;
                            menuAdminHorario.Visible = false;
                            menuAdmReportes.Visible = false;
                            menuAdmVacaciones.Visible = false;
                            lhorario.Visible = false;
                            lgraficas.Visible = false;
                            lAprobacaiones.Visible = false;
                            lvacaciones.Visible = false;
                            lreportes.Visible = false;
                            laccesos.Visible = false;
                            lnkInicio.Visible = false;
                            break;

                    }
                }
                else
                {
                    SesionState.usuario = null; // limpiar sesión
                    Response.Redirect("login.aspx"); // redirigir al login
                }
            }

        }

        protected void btnHome_Click(object sender, EventArgs e)
        {
            // EJEMPLO: el rol que guardas en sesión
            int rol = Convert.ToInt32(Session["Rol"]);

            if (rol == 1 || rol == 3)
            {
                Response.Redirect("~/PrincipalAdmin.aspx");   // Admin
            }
            else if (rol == 2)
            {
                Response.Redirect("~/PrincipalEmpleado.aspx"); // Empleado
            }
            else
            {
                Response.Redirect("~/Default.aspx"); // por si no hay rol válido
            }
        }

        protected void CerrarSesion_Click(object sender, EventArgs e)
        {
            SesionState.usuario = null; // limpiar sesión
            Response.Redirect("login.aspx"); // redirigir al login
        }

    
    }
}