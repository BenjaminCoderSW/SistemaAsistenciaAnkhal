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
                            menuAdmAcessos.Visible = true;
                            menuAdmAprovaciones.Visible = true;
                            menuAdmAsistencia.Visible = true;
                            menuAdmGraficas.Visible = true;
                            menuAdminHorario.Visible = true;
                            menuAdmReportes.Visible = true;
                            menuAdmVacaciones.Visible = true;
                            menuAdmFormatos.Visible = true;
                            menuConfigVacaciones.Visible = true;
                            menuRegistrarFaltas.Visible = true;
                            lnkInicio.Visible = true;
                            liAprobarVacacionesJefe.Visible = false;
                            liHistorialRechazosJefe.Visible = true;
                            liHistorialDecisionesRH.Visible = true;
                            liMisHorasExtra.Visible = false;

                            break;

                        case "Rh":
                            menuAdmAcessos.Visible = true;
                            menuAdmAprovaciones.Visible = true;
                            menuAdmAsistencia.Visible = true;
                            menuAdmGraficas.Visible = true;
                            menuAdminHorario.Visible = true;
                            menuAdmReportes.Visible = true;
                            menuAdmVacaciones.Visible = true;
                            menuAdmFormatos.Visible = true;
                            menuConfigVacaciones.Visible = true;
                            menuRegistrarFaltas.Visible = true;
                            lnkInicio.Visible = true;
                            liAprobarVacacionesJefe.Visible = false;
                            liHistorialRechazosJefe.Visible = true;
                            liHistorialDecisionesRH.Visible = true;
                            liMisHorasExtra.Visible = false;

                            break;

                        case "Empleado":
                            menuAdmAcessos.Visible = false;
                            menuAdmAprovaciones.Visible = false;
                            menuAdmAsistencia.Visible = true;
                            menuAdmGraficas.Visible = false;
                            menuAdminHorario.Visible = false;
                            menuAdmReportes.Visible = false;
                            menuAdmVacaciones.Visible = true;
                            menuAdmFormatos.Visible = false;
                            menuConfigVacaciones.Visible = false;
                            menuRegistrarFaltas.Visible = false;
                            lformatos.Visible = false;
                            lhorario.Visible = false;
                            lgraficas.Visible = false;
                            lAprobacaiones.Visible = false;
                            lvacaciones.Visible = true;
                            lreportes.Visible = false;
                            laccesos.Visible = false;
                            lnkInicio.Visible = false;
                            liAprobarVacacionesJefe.Visible = false;
                            liHistorialRechazosJefe.Visible = false;
                            liHistorialDecisionesRH.Visible = false;
                            liMisHorasExtra.Visible = true;
                            break;

                        case "Jefe de Planta":
                            menuAdmAcessos.Visible = false;
                            menuAdmAprovaciones.Visible = true;
                            menuAdmAsistencia.Visible = true;
                            menuAdmGraficas.Visible = false;
                            menuAdminHorario.Visible = false;
                            menuAdmReportes.Visible = true;
                            menuAdmVacaciones.Visible = true;
                            menuAdmFormatos.Visible = false;
                            menuConfigVacaciones.Visible = false;
                            menuRegistrarFaltas.Visible = false;
                            lformatos.Visible = false;
                            lhorario.Visible = false;
                            lgraficas.Visible = false;
                            lAprobacaiones.Visible = true;
                            lvacaciones.Visible = true;
                            lreportes.Visible = true;
                            laccesos.Visible = false;
                            lnkInicio.Visible = false;
                            // Solo Reporte de Asistencia visible en el submenú Reportes
                            liRptAsistencia.Visible = true;
                            liRptComida.Visible = false;
                            liRptComisionesDias.Visible = false;
                            liRptComisionesHoras.Visible = false;
                            liRptPermisos.Visible = false;
                            liRptPermisosHoras.Visible = false;
                            liRptVacaciones.Visible = false;
                            liRptJustificacion.Visible = false;
                            liRptHorasExtraRH.Visible = false;
                            // Aprobaciones: solo Horas Extra visible para Jefe de Planta
                            liAprobarPermisosHora.Visible = false;
                            liAprobarPermisoDias.Visible = false;
                            liAprobarComisionHoras.Visible = false;
                            liAprobarComisionDias.Visible = false;
                            liAprobarJustificacion.Visible = false;
                            liAprobarVacaciones.Visible = false;
                            liAprobarHorasExtra.Visible = true;
                            // Vacaciones: Jefe de Planta usa su propia página de aprobación
                            liAprobarVacacionesJefe.Visible = true;
                            liHistorialRechazosJefe.Visible = false;
                            liHistorialDecisionesRH.Visible = false;
                            liMisHorasExtra.Visible = false;
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
            string rol = SesionState.usuario.tRol.Rol;
            if (rol == "Administrador" || rol == "Rh")
                Response.Redirect("~/PrincipalAdmin.aspx");
            else
                Response.Redirect("~/PrincipalEmpleados.aspx");
        }

        protected void CerrarSesion_Click(object sender, EventArgs e)
        {
            // ✅ Limpiar completamente la sesión
            Session.Clear();
            Session.Abandon();
            SesionState.usuario = null;

            Response.Redirect("login.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }


    }
}