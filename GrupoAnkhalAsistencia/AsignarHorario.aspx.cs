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
    public partial class AsignarHorario : System.Web.UI.Page
    {
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
         ConfigurationManager.ConnectionStrings["MedensInventarioConnectionString"].ConnectionString);
      
        protected void Page_Load(object sender, EventArgs e)
        {
            // ¿Sesion válida?
            if (SesionState.usuario == null)
            {
                SesionState.usuario = null;
                Response.Redirect("login.aspx");
                return;
            }


            string rolUsuario = SesionState.usuario.tRol.Rol;  // ajusta al nombre que tengas en tu clase

            // Aquí pones los roles que SI pueden entrar
            string[] rolesPermitidos = { "Administrador", "Rh" };

            if (!rolesPermitidos.Contains(rolUsuario))
            {
                // Si NO tiene rol válido → lo sacamos
                Response.Redirect("login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                CargarAsignarHorario();
                CargarHorario();
                CargarUsuario();
                CargarDia();
            }
        }



        private void CargarAsignarHorario()
        {
            var asignarhorario = from m in db.tAsignarHorario
                          join r in db.tHorario on m.IdHorario equals r.IdHorario
                          join p in db.tUsuario on m.IdUsuario equals p.IdUsuario
                          join d in db.tDia on m.IdDia equals d.IdDia
                                 where m.Estatus == 1
                          orderby m.IdUsuario
                          select new
                          {
                              m.IdAsignarHorario,
                              m.IdHorario,
                              m.IdUsuario,
                              m.IdDia,
                              Horario = r.Descripcion,
                              Usuario = p.Nombre +" "+ p.ApellidoPaterno + " " + p.ApellidoMaterno,
                              Dia = d.Dia
                          };

            dvgAsignacionHorario.DataSource = asignarhorario.ToList();
            dvgAsignacionHorario.DataBind();
        }

        private void CargarHorario()
        {
            var horario = db.tHorario
            .Select(r => new { r.IdHorario, r.Descripcion }) // o los campos que necesites
            .ToList();

            // DropDown principal
            ddlHorario.DataSource = horario;
            ddlHorario.DataTextField = "Descripcion";
            ddlHorario.DataValueField = "IdHorario";
            ddlHorario.DataBind();
            ddlHorario.Items.Insert(0, new ListItem("-- Seleccione --", "0"));

            // DropDown del modal
            ddlHorarioModal.DataSource = horario;
            ddlHorarioModal.DataTextField = "Descripcion";
            ddlHorarioModal.DataValueField = "IdHorario";
            ddlHorarioModal.DataBind();
            ddlHorarioModal.Items.Insert(0, new ListItem("-- Seleccione --", "0"));
        }



        private void CargarUsuario()
        {
            var usuario = db.tUsuario
                .Select(t => new { t.IdUsuario,  Usuario=t.Nombre +" "+ t.ApellidoPaterno + " " + t.ApellidoMaterno })
                .ToList();

            // DropDown principal
            ddlUsuario.DataSource = usuario;
            ddlUsuario.DataTextField = "Usuario";
            ddlUsuario.DataValueField = "IdUsuario";
            ddlUsuario.DataBind();
            ddlUsuario.Items.Insert(0, new ListItem("-- Seleccione --", "0"));

            // DropDown del modal
            ddlUsuarioModal.DataSource = usuario;
            ddlUsuarioModal.DataTextField = "Usuario";
            ddlUsuarioModal.DataValueField = "IdUsuario";
            ddlUsuarioModal.DataBind();
            ddlUsuarioModal.Items.Insert(0, new ListItem("-- Seleccione --", "0"));
        }

        private void CargarDia()
        {
            var dias = db.tDia
                .Select(t => new { t.IdDia, t.Dia })
                .ToList();

            // Para el CheckBoxList principal
            chkDias.DataSource = dias;
            chkDias.DataTextField = "Dia";
            chkDias.DataValueField = "IdDia";
            chkDias.DataBind();

            // Para el modal de edición, puedes mantener el DropDownList si lo necesitas
            ddlDiaModal.DataSource = dias;
            ddlDiaModal.DataTextField = "Dia";
            ddlDiaModal.DataValueField = "IdDia";
            ddlDiaModal.DataBind();
            ddlDiaModal.Items.Insert(0, new ListItem("-- Seleccione --", "0"));
        }


        protected void btnEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                int id = Convert.ToInt32(btn.CommandArgument);

                var medico = db.tAsignarHorario.FirstOrDefault(t => t.IdAsignarHorario == id);
                if (medico != null)
                {
                    // 🔹 Cambiamos el estado en lugar de eliminar
                    medico.Estatus = 0;

                    db.SubmitChanges(); // Guardamos los cambios

                    CargarUsuario(); // Recarga la lista (asegúrate que filtre solo Estatus = 1)

                    MostrarAlerta("success", "Inactivado", "El Usuario se marcó como inactivo correctamente.");
                }
                else
                {
                    MostrarAlerta("warning", "No encontrado", "No se encontró el Usuario seleccionado.");
                }
            }
            catch (Exception ex)
            {
                MostrarAlerta("error", "Error", "No se pudo actualizar el estado del Usuario: " + ex.Message);
            }
        }

        //protected void btnGuardar_Click(object sender, EventArgs e)
        //{
        //    if (!ValidarCampos()) return;

        //    try
        //    {

        //        // Validar duplicado por CURP
        //        if (db.tAsignarHorario.Any(u => u.IdHorario ==Convert.ToInt32(ddlHorario.SelectedValue) && u.IdUsuario == Convert.ToInt32(ddlUsuario.SelectedValue)))
        //        {
        //            MostrarAlerta("error", "Duplicado", "Ya existe un Usuario con el mismo horario.");
        //            return;
        //        }


        //        tAsignarHorario nuevo = new tAsignarHorario
        //        {
        //            IdHorario = Convert.ToInt32(ddlHorario.SelectedValue),
        //            IdUsuario = Convert.ToInt32(ddlUsuario.SelectedValue),
        //            IdDia = Convert.ToInt32(ddlDia.SelectedValue),
        //            Estatus = 1
        //        };

        //        db.tAsignarHorario.InsertOnSubmit(nuevo);
        //        db.SubmitChanges();


        //        LimpiarCampos();
        //        CargarAsignarHorario();


        //    }
        //    catch (Exception ex)
        //    {
        //        MostrarAlerta("error", "Error", "No se pudo guardar el Usuario: " + ex.Message);
        //    }
        //}

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (ddlHorario.SelectedValue == "0")
            {
                MostrarAlerta("warning", "Campo obligatorio", "Debe seleccionar un horario.");
                return;
            }

            if (ddlUsuario.SelectedValue == "0")
            {
                MostrarAlerta("warning", "Campo obligatorio", "Debe seleccionar un usuario.");
                return;
            }

            // Obtener los días seleccionados
            var diasSeleccionados = chkDias.Items.Cast<ListItem>()
                .Where(i => i.Selected)
                .Select(i => Convert.ToInt32(i.Value))
                .ToList();

            if (diasSeleccionados.Count == 0)
            {
                MostrarAlerta("warning", "Campo obligatorio", "Debe seleccionar al menos un día.");
                return;
            }

            try
            {
                foreach (var dia in diasSeleccionados)
                {
                    // Verificar duplicado (usuario + horario + día)
                    bool existe = db.tAsignarHorario.Any(u =>
                        u.IdHorario == Convert.ToInt32(ddlHorario.SelectedValue) &&
                        u.IdUsuario == Convert.ToInt32(ddlUsuario.SelectedValue) &&
                        u.IdDia == dia &&
                        u.Estatus == 1);

                    if (existe)
                        continue; // Si ya existe, lo omitimos

                    // Insertar nuevo registro
                    tAsignarHorario nuevo = new tAsignarHorario
                    {
                        IdHorario = Convert.ToInt32(ddlHorario.SelectedValue),
                        IdUsuario = Convert.ToInt32(ddlUsuario.SelectedValue),
                        IdDia = dia,
                        Estatus = 1
                    };

                    db.tAsignarHorario.InsertOnSubmit(nuevo);
                }

                db.SubmitChanges();

                LimpiarCampos();
                CargarAsignarHorario();
                MostrarAlerta("success", "Guardado", "Los días fueron asignados correctamente.");
            }
            catch (Exception ex)
            {
                MostrarAlerta("error", "Error", "No se pudo guardar el usuario: " + ex.Message);
            }
        }




        private bool ValidarCampos()
        {
            if (ddlHorario.SelectedValue == "0")
            {
                MostrarAlerta("warning", "Campo obligatorio", "Debe seleccionar un horario.");
                return false;
            }

            if (ddlUsuario.SelectedValue == "0")
            {
                MostrarAlerta("warning", "Campo obligatorio", "Debe seleccionar un usuario.");
                return false;
            }

           

            // Si pasa todas las validaciones:
            return true;
        }


        private int ConvertirEntero(string valor)
        {
            int resultado;
            return int.TryParse(valor, out resultado) ? resultado : 0;
        }

        private void LimpiarCampos()
        {
            ddlHorario.SelectedIndex = 0;
            ddlUsuario.SelectedIndex = 0;
            foreach (ListItem item in chkDias.Items)
            {
                item.Selected = false;
            }


        }

        private void MostrarAlerta(string icono, string titulo, string mensaje)
        {
            string script = $@"
                Swal.fire({{
                    icon: '{icono}',
                    title: '{titulo}',
                    text: '{mensaje}',
                    showConfirmButton: false,
                    timer: 2500
                }});";
            ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), script, true);
        }


        protected void btnGuardarModal_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(hfIdAsignarHorario.Value))
            {
                MostrarAlerta("error", "Error", "No se encontró el ID del usuario.");
                return;
            }



            int id = Convert.ToInt32(hfIdAsignarHorario.Value);

            try
            {
                var medico = db.tAsignarHorario.FirstOrDefault(t => t.IdAsignarHorario == id);
                if (medico == null)
                {
                    MostrarAlerta("error", "Error", "El horario no existe.");
                    return;
                }

                // Actualizar campos
                medico.IdHorario = Convert.ToInt32(ddlHorarioModal.SelectedValue);
                medico.IdUsuario = Convert.ToInt32(ddlUsuarioModal.SelectedValue);
                medico.IdDia = Convert.ToInt32(ddlDiaModal.SelectedValue);
                
                db.SubmitChanges();
                CargarUsuario();

                ScriptManager.RegisterStartupScript(this, GetType(),
                    Guid.NewGuid().ToString(),
                    "$('#modalEditar').modal('hide');", true);

                MostrarAlerta("success", "Actualizado", "El usuario se actualizó correctamente.");
            }
            catch (Exception ex)
            {
                MostrarAlerta("error", "Error", "No se pudo actualizar: " + ex.Message);
            }
        }

        private void CargarAsignarHorarios(string filtro = "")
        {
            var query = from t in db.tAsignarHorario
                        join r in db.tHorario on t.IdHorario equals r.IdHorario
                        join p in db.tUsuario on t.IdUsuario equals p.IdUsuario
                        join a in db.tDia on t.IdDia equals a.IdDia
                        where  t.Estatus == 1
                        select new
                        {
                            t.IdAsignarHorario,
                            t.IdHorario,
                            t.IdUsuario,
                            t.IdDia,
                            Horario = r.Descripcion,
                            Usuario = p.Nombre +" "+ p.ApellidoPaterno + " " + p.ApellidoMaterno,
                            Dia = a.Dia
                        };

            if (!string.IsNullOrEmpty(filtro))
            {
                filtro = filtro.Trim();

                query = query.Where(x =>
                    System.Data.Linq.SqlClient.SqlMethods.Like(x.Usuario, "%" + filtro + "%") ||
                    System.Data.Linq.SqlClient.SqlMethods.Like(x.Horario, "%" + filtro + "%") 
                );
            }

            dvgAsignacionHorario.DataSource = query.ToList();
            dvgAsignacionHorario.DataBind();
        }



        protected void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            CargarAsignarHorarios(txtBuscar.Text.Trim());
        }



        protected void dvgAsignacionHorario_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dvgAsignacionHorario.PageIndex = e.NewPageIndex;
            CargarAsignarHorario();
        }
    }
}