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
            ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        // ── ViewState para mantener el filtro al paginar ──────────────────────────
        private string FiltroActual
        {
            get { return ViewState["FiltroAsignar"] as string ?? ""; }
            set { ViewState["FiltroAsignar"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (SesionState.usuario == null)
            {
                SesionState.usuario = null;
                Response.Redirect("login.aspx");
                return;
            }

            string rolUsuario = SesionState.usuario.tRol.Rol;
            string[] rolesPermitidos = { "Administrador", "Rh" };

            if (!rolesPermitidos.Contains(rolUsuario))
            {
                Response.Redirect("login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                CargarAsignarHorarioFiltrado();
                CargarHorario();
                CargarUsuario();
                CargarDia();
            }
        }

        // ── Carga con filtro opcional ────────────────────────────────────────────
        private void CargarAsignarHorarioFiltrado(string filtro = "")
        {
            var query = from m in db.tAsignarHorario
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
                            Usuario = p.Nombre + " " + p.ApellidoPaterno + " " + p.ApellidoMaterno,
                            Dia = d.Dia
                        };

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                query = query.Where(x =>
                    System.Data.Linq.SqlClient.SqlMethods.Like(x.Usuario, "%" + filtro + "%") ||
                    System.Data.Linq.SqlClient.SqlMethods.Like(x.Horario, "%" + filtro + "%"));
            }

            dvgAsignacionHorario.DataSource = query.ToList();
            dvgAsignacionHorario.DataBind();
        }

        // ── Búsqueda ─────────────────────────────────────────────────────────────
        protected void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            FiltroActual = txtBuscar.Text.Trim();
            dvgAsignacionHorario.PageIndex = 0;
            CargarAsignarHorarioFiltrado(FiltroActual);
        }

        // ── Paginación ───────────────────────────────────────────────────────────
        protected void dvgAsignacionHorario_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dvgAsignacionHorario.PageIndex = e.NewPageIndex;
            CargarAsignarHorarioFiltrado(FiltroActual);   // mantiene el filtro
        }

        // ── Catálogos ────────────────────────────────────────────────────────────
        private void CargarHorario()
        {
            var horario = db.tHorario
                .Where(h => h.Estatus == 1)
                .Select(r => new { r.IdHorario, r.Descripcion })
                .ToList();

            ddlHorario.DataSource = horario;
            ddlHorario.DataTextField = "Descripcion";
            ddlHorario.DataValueField = "IdHorario";
            ddlHorario.DataBind();
            ddlHorario.Items.Insert(0, new ListItem("-- Seleccione --", "0"));

            ddlHorarioModal.DataSource = horario;
            ddlHorarioModal.DataTextField = "Descripcion";
            ddlHorarioModal.DataValueField = "IdHorario";
            ddlHorarioModal.DataBind();
            ddlHorarioModal.Items.Insert(0, new ListItem("-- Seleccione --", "0"));
        }

        private void CargarUsuario()
        {
            var usuario = db.tUsuario
                .Where(u => u.Estatus == 1)
                .Select(t => new
                {
                    t.IdUsuario,
                    Usuario = t.Nombre + " " + t.ApellidoPaterno + " " + t.ApellidoMaterno
                })
                .ToList();

            ddlUsuario.DataSource = usuario;
            ddlUsuario.DataTextField = "Usuario";
            ddlUsuario.DataValueField = "IdUsuario";
            ddlUsuario.DataBind();
            ddlUsuario.Items.Insert(0, new ListItem("-- Seleccione --", "0"));

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

            chkDias.DataSource = dias;
            chkDias.DataTextField = "Dia";
            chkDias.DataValueField = "IdDia";
            chkDias.DataBind();

            ddlDiaModal.DataSource = dias;
            ddlDiaModal.DataTextField = "Dia";
            ddlDiaModal.DataValueField = "IdDia";
            ddlDiaModal.DataBind();
            ddlDiaModal.Items.Insert(0, new ListItem("-- Seleccione --", "0"));
        }

        // ── Guardar nueva asignación ─────────────────────────────────────────────
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
                    bool existe = db.tAsignarHorario.Any(u =>
                        u.IdHorario == Convert.ToInt32(ddlHorario.SelectedValue) &&
                        u.IdUsuario == Convert.ToInt32(ddlUsuario.SelectedValue) &&
                        u.IdDia == dia &&
                        u.Estatus == 1);

                    if (existe) continue;

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
                CargarAsignarHorarioFiltrado(FiltroActual);
                MostrarAlerta("success", "Guardado", "Los días fueron asignados correctamente.");
            }
            catch (Exception ex)
            {
                MostrarAlerta("error", "Error", "No se pudo guardar: " + ex.Message);
            }
        }

        // ── Guardar edición ──────────────────────────────────────────────────────
        protected void btnGuardarModal_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(hfIdAsignarHorario.Value))
            {
                MostrarAlerta("error", "Error", "No se encontró el ID del registro.");
                return;
            }

            int id = Convert.ToInt32(hfIdAsignarHorario.Value);

            try
            {
                var registro = db.tAsignarHorario.FirstOrDefault(t => t.IdAsignarHorario == id);
                if (registro == null)
                {
                    MostrarAlerta("error", "Error", "El registro no existe.");
                    return;
                }

                registro.IdHorario = Convert.ToInt32(ddlHorarioModal.SelectedValue);
                registro.IdUsuario = Convert.ToInt32(ddlUsuarioModal.SelectedValue);
                registro.IdDia = Convert.ToInt32(ddlDiaModal.SelectedValue);

                db.SubmitChanges();

                ScriptManager.RegisterStartupScript(this, GetType(),
                    Guid.NewGuid().ToString(),
                    "$('#modalEditar').modal('hide');", true);

                CargarAsignarHorarioFiltrado(FiltroActual);
                MostrarAlerta("success", "Actualizado", "La asignación se actualizó correctamente.");
            }
            catch (Exception ex)
            {
                MostrarAlerta("error", "Error", "No se pudo actualizar: " + ex.Message);
            }
        }

        // ── Eliminar (cambio de estatus) ─────────────────────────────────────────
        protected void btnEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                int id = Convert.ToInt32(btn.CommandArgument);

                var registro = db.tAsignarHorario.FirstOrDefault(t => t.IdAsignarHorario == id);
                if (registro != null)
                {
                    registro.Estatus = 0;
                    db.SubmitChanges();
                    CargarAsignarHorarioFiltrado(FiltroActual);
                    MostrarAlerta("success", "Inactivado", "La asignación se marcó como inactiva correctamente.");
                }
                else
                {
                    MostrarAlerta("warning", "No encontrado", "No se encontró el registro seleccionado.");
                }
            }
            catch (Exception ex)
            {
                MostrarAlerta("error", "Error", "No se pudo actualizar el estado: " + ex.Message);
            }
        }

        // ── Limpiar formulario ───────────────────────────────────────────────────
        private void LimpiarCampos()
        {
            ddlHorario.SelectedIndex = 0;
            ddlUsuario.SelectedIndex = 0;
            foreach (ListItem item in chkDias.Items)
                item.Selected = false;
        }

        // ── Helper alerta ────────────────────────────────────────────────────────
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
    }
}