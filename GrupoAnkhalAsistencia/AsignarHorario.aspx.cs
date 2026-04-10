using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
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
                Response.Redirect("login.aspx");
                return;
            }

            string rolUsuario = SesionState.usuario.tRol.Rol;
            if (!new[] { "Administrador", "Rh" }.Contains(rolUsuario))
            {
                Response.Redirect("login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                CargarGridSemanal();
                CargarEmpleadoDDL();
                CargarHorariosDDLs();
            }
        }

        // ── Grid pivotado: 1 fila por empleado ──────────────────────────────────
        private void CargarGridSemanal(string filtro = "")
        {
            var flat = (from a in db.tAsignarHorario
                        join h in db.tHorario on a.IdHorario equals h.IdHorario
                        join u in db.tUsuario  on a.IdUsuario  equals u.IdUsuario
                        where a.Estatus == 1 && u.Estatus == 1
                        select new
                        {
                            a.IdUsuario,
                            NombreCompleto = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,
                            a.IdDia,
                            h.Descripcion
                        }).ToList();

            if (!string.IsNullOrWhiteSpace(filtro))
                flat = flat.Where(x => x.NombreCompleto.IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            // Pivot en memoria: agrupar por empleado y distribuir por IdDia
            var rows = flat
                .GroupBy(x => new { x.IdUsuario, x.NombreCompleto })
                .Select(g => new HorarioSemanalRow
                {
                    IdUsuario      = g.Key.IdUsuario.GetValueOrDefault(),
                    NombreCompleto = g.Key.NombreCompleto,
                    Lunes          = g.FirstOrDefault(x => x.IdDia == 2)?.Descripcion,
                    Martes         = g.FirstOrDefault(x => x.IdDia == 3)?.Descripcion,
                    Miercoles      = g.FirstOrDefault(x => x.IdDia == 4)?.Descripcion,
                    Jueves         = g.FirstOrDefault(x => x.IdDia == 5)?.Descripcion,
                    Viernes        = g.FirstOrDefault(x => x.IdDia == 6)?.Descripcion,
                    Sabado         = g.FirstOrDefault(x => x.IdDia == 7)?.Descripcion,
                    Domingo        = g.FirstOrDefault(x => x.IdDia == 1)?.Descripcion
                })
                .OrderBy(r => r.NombreCompleto)
                .ToList();

            dvgAsignacionHorario.DataSource = rows;
            dvgAsignacionHorario.DataBind();
        }

        // ── Catálogos ────────────────────────────────────────────────────────────
        private void CargarEmpleadoDDL()
        {
            var lista = db.tUsuario
                .Where(u => u.Estatus == 1)
                .Select(u => new
                {
                    u.IdUsuario,
                    Nombre = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno
                })
                .OrderBy(u => u.Nombre)
                .ToList();

            ddlEmpleadoSemana.DataSource    = lista;
            ddlEmpleadoSemana.DataTextField  = "Nombre";
            ddlEmpleadoSemana.DataValueField = "IdUsuario";
            ddlEmpleadoSemana.DataBind();
            ddlEmpleadoSemana.Items.Insert(0, new ListItem("-- Seleccione empleado --", "0"));
        }

        private void CargarHorariosDDLs()
        {
            var horarios = db.tHorario
                .Where(h => h.Estatus == 1)
                .Select(h => new { h.IdHorario, h.Descripcion })
                .OrderBy(h => h.Descripcion)
                .ToList();

            var ddls = new[] { ddlLunes, ddlMartes, ddlMiercoles, ddlJueves, ddlViernes, ddlSabado, ddlDomingo };
            foreach (var ddl in ddls)
            {
                ddl.DataSource    = horarios;
                ddl.DataTextField  = "Descripcion";
                ddl.DataValueField = "IdHorario";
                ddl.DataBind();
                ddl.Items.Insert(0, new ListItem("Sin horario", "0"));
            }
        }

        // ── Pre-cargar asignaciones actuales de un empleado en los 7 DDLs ───────
        private void PreCargarAsignacionesEmpleado(int idUsuario)
        {
            if (idUsuario == 0) return;

            var asignaciones = db.tAsignarHorario
                .Where(a => a.IdUsuario == idUsuario && a.Estatus == 1)
                .Select(a => new { a.IdDia, a.IdHorario })
                .ToList();

            // IdDia → DDL correspondiente
            var mapDia = new Dictionary<int, DropDownList>
            {
                { 1, ddlDomingo   },
                { 2, ddlLunes     },
                { 3, ddlMartes    },
                { 4, ddlMiercoles },
                { 5, ddlJueves    },
                { 6, ddlViernes   },
                { 7, ddlSabado    }
            };

            // Reset todos los DDLs
            foreach (var ddl in mapDia.Values)
                ddl.SelectedValue = "0";

            // Setear valores actuales
            foreach (var a in asignaciones)
            {
                int idDia     = a.IdDia.GetValueOrDefault();
                int idHorario = a.IdHorario.GetValueOrDefault();

                if (mapDia.TryGetValue(idDia, out var ddl))
                {
                    var item = ddl.Items.FindByValue(idHorario.ToString());
                    if (item != null)
                        ddl.SelectedValue = idHorario.ToString();
                }
            }

            // Reflejar empleado en el DDL selector y en el hidden
            hfIdUsuarioEditar.Value = idUsuario.ToString();
            var selItem = ddlEmpleadoSemana.Items.FindByValue(idUsuario.ToString());
            if (selItem != null)
                ddlEmpleadoSemana.SelectedValue = idUsuario.ToString();
        }

        // ── Postback proxy: abre modal desde botón "Editar semana" del grid ─────
        protected void btnCargarEmpleado_Click(object sender, EventArgs e)
        {
            string arg = Request["__EVENTARGUMENT"];
            if (!int.TryParse(arg, out int idUsuario) || idUsuario == 0)
            {
                MostrarAlerta("warning", "Empleado no válido", "No se pudo identificar al empleado.");
                return;
            }

            // Asegurar que los DDLs estén poblados (en postback persisten via ViewState,
            // pero si por alguna razón están vacíos se re-cargan)
            if (ddlLunes.Items.Count == 0) CargarHorariosDDLs();
            if (ddlEmpleadoSemana.Items.Count == 0) CargarEmpleadoDDL();

            PreCargarAsignacionesEmpleado(idUsuario);

            // jQuery no está disponible aún (carga después del </form> en Site.Master).
            // Usamos un flag que el window.load handler del ASPX detecta para abrir el modal.
            ScriptManager.RegisterStartupScript(this, GetType(), "openModalSemana",
                "window.__abrirModalSemana = true;", true);
        }

        // ── AutoPostBack al seleccionar empleado en el modal "Agregar nuevo" ────
        protected void ddlEmpleadoSemana_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idUsuario = Convert.ToInt32(ddlEmpleadoSemana.SelectedValue);
            if (idUsuario == 0) return;

            if (ddlLunes.Items.Count == 0) CargarHorariosDDLs();

            PreCargarAsignacionesEmpleado(idUsuario);

            ScriptManager.RegisterStartupScript(this, GetType(), "openModalAfterSelect",
                "window.__abrirModalSemana = true;", true);
        }

        // ── Guardar semana completa (upsert por día) ─────────────────────────────
        protected void btnGuardarSemana_Click(object sender, EventArgs e)
        {
            int idUsuario = Convert.ToInt32(hfIdUsuarioEditar.Value);
            if (idUsuario == 0)
            {
                MostrarAlerta("warning", "Empleado requerido", "Debe seleccionar un empleado.");
                ScriptManager.RegisterStartupScript(this, GetType(), "reopenModal",
                    "window.__abrirModalSemana = true;", true);
                return;
            }

            // Construir lista de intenciones para los 7 días
            var nuevasSemana = new List<DiaAsignacion>
            {
                new DiaAsignacion { IdDia = 1, IdHorario = Convert.ToInt32(ddlDomingo.SelectedValue)   },
                new DiaAsignacion { IdDia = 2, IdHorario = Convert.ToInt32(ddlLunes.SelectedValue)     },
                new DiaAsignacion { IdDia = 3, IdHorario = Convert.ToInt32(ddlMartes.SelectedValue)    },
                new DiaAsignacion { IdDia = 4, IdHorario = Convert.ToInt32(ddlMiercoles.SelectedValue) },
                new DiaAsignacion { IdDia = 5, IdHorario = Convert.ToInt32(ddlJueves.SelectedValue)    },
                new DiaAsignacion { IdDia = 6, IdHorario = Convert.ToInt32(ddlViernes.SelectedValue)   },
                new DiaAsignacion { IdDia = 7, IdHorario = Convert.ToInt32(ddlSabado.SelectedValue)    }
            };

            // Obtener todos los registros del empleado (activos e inactivos)
            var actuales = db.tAsignarHorario
                .Where(a => a.IdUsuario == idUsuario)
                .ToList();

            // Mapa IdHorario → Descripcion para los mensajes
            var nombresHorario = db.tHorario
                .Where(h => h.Estatus == 1)
                .ToDictionary(h => h.IdHorario, h => h.Descripcion);

            var nombresDia = new Dictionary<int, string>
            {
                { 1, "Domingo"   }, { 2, "Lunes"    }, { 3, "Martes"   },
                { 4, "Miércoles" }, { 5, "Jueves"   }, { 6, "Viernes"  },
                { 7, "Sábado"    }
            };

            var cambios = new StringBuilder();

            try
            {
                foreach (var nuevo in nuevasSemana)
                {
                    var registroActivo   = actuales.FirstOrDefault(a => a.IdDia == nuevo.IdDia && a.Estatus == 1);
                    var registroInactivo = actuales.FirstOrDefault(a => a.IdDia == nuevo.IdDia && a.Estatus == 0);
                    string dia = nombresDia[nuevo.IdDia];

                    if (nuevo.IdHorario == 0)
                    {
                        // Usuario eligió "sin horario" → soft-delete si había uno activo
                        if (registroActivo != null)
                        {
                            string viejo = nombresHorario.TryGetValue(registroActivo.IdHorario.GetValueOrDefault(), out var vn) ? vn : "?";
                            registroActivo.Estatus = 0;
                            cambios.AppendLine($"• {dia}: se eliminó '{viejo}'.");
                        }
                    }
                    else
                    {
                        if (registroActivo == null)
                        {
                            // No había asignación activa para este día
                            if (registroInactivo != null && registroInactivo.IdHorario == nuevo.IdHorario)
                            {
                                // Reactivar el mismo horario en lugar de crear duplicado
                                registroInactivo.Estatus = 1;
                                string nombre = nombresHorario.TryGetValue(nuevo.IdHorario, out var nn) ? nn : "?";
                                cambios.AppendLine($"• {dia}: se reactivó '{nombre}'.");
                            }
                            else
                            {
                                // Insertar nuevo registro
                                db.tAsignarHorario.InsertOnSubmit(new tAsignarHorario
                                {
                                    IdUsuario = idUsuario,
                                    IdHorario = nuevo.IdHorario,
                                    IdDia     = nuevo.IdDia,
                                    Estatus   = 1
                                });
                                string nombre = nombresHorario.TryGetValue(nuevo.IdHorario, out var nn) ? nn : "?";
                                cambios.AppendLine($"• {dia}: se asignó '{nombre}'.");
                            }
                        }
                        else if (registroActivo.IdHorario == nuevo.IdHorario)
                        {
                            // Mismo horario ya activo → sin cambio
                        }
                        else
                        {
                            // Horario diferente → actualizar el registro existente
                            string viejo = nombresHorario.TryGetValue(registroActivo.IdHorario.GetValueOrDefault(), out var vn) ? vn : "?";
                            string nuevo2 = nombresHorario.TryGetValue(nuevo.IdHorario, out var nn) ? nn : "?";
                            registroActivo.IdHorario = nuevo.IdHorario;
                            cambios.AppendLine($"• {dia}: cambió de '{viejo}' a '{nuevo2}'.");
                        }
                    }
                }

                db.SubmitChanges();
            }
            catch (Exception ex)
            {
                MostrarAlerta("error", "Error al guardar", ex.Message);
                ScriptManager.RegisterStartupScript(this, GetType(), "reopenOnError",
                    "window.__abrirModalSemana = true;", true);
                return;
            }

            ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(),
                "window.__cerrarModalSemana = true;", true);

            CargarGridSemanal(FiltroActual);

            // Construir SweetAlert con resumen de cambios
            string resumen = cambios.Length > 0
                ? cambios.ToString().Trim().Replace("'", "\\'").Replace("\r", "").Replace("\n", "\\n")
                : "No se realizaron cambios.";

            string swal = $@"
                Swal.fire({{
                    icon: 'success',
                    title: 'Guardado',
                    html: '<pre style=""text-align:left;font-size:0.85rem;margin:0"">{resumen}</pre>',
                    confirmButtonText: 'Cerrar'
                }});";
            ScriptManager.RegisterStartupScript(this, GetType(), "alertSaved", swal, true);
        }

        // ── Búsqueda ─────────────────────────────────────────────────────────────
        protected void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            FiltroActual = txtBuscar.Text.Trim();
            dvgAsignacionHorario.PageIndex = 0;
            CargarGridSemanal(FiltroActual);
        }

        // ── Paginación ───────────────────────────────────────────────────────────
        protected void dvgAsignacionHorario_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dvgAsignacionHorario.PageIndex = e.NewPageIndex;
            CargarGridSemanal(FiltroActual);
        }

        // ── Helper alerta ────────────────────────────────────────────────────────
        private void MostrarAlerta(string icono, string titulo, string mensaje)
        {
            string script = $@"
                Swal.fire({{
                    icon: '{icono}',
                    title: '{titulo}',
                    text: '{mensaje.Replace("'", "\\'")}',
                    showConfirmButton: false,
                    timer: 2500
                }});";
            ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), script, true);
        }
    }
}
