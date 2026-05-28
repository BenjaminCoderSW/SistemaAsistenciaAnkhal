using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Configuration;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class CargarHorasExtraManual : System.Web.UI.Page
    {
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
            ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (SesionState.usuario == null) { Response.Redirect("login.aspx"); return; }

            string rol = SesionState.usuario.tRol.Rol;
            if (rol != "Administrador" && rol != "Rh" && rol != "Jefe de Planta")
            {
                Response.Redirect("login.aspx"); return;
            }

            if (!IsPostBack)
            {
                txtFecha.Text = DateTime.Now.ToString("yyyy-MM-dd");
                CargarEmpleados();
                CargarRecientes();
            }

            // Forzar el campo de horas como input numérico (0–23) sin AM/PM
            txtHorasH.Attributes["type"] = "number";
            txtHorasH.Attributes["min"]  = "0";
            txtHorasH.Attributes["max"]  = "23";
            txtHorasH.Attributes["step"] = "1";
        }

        private void CargarEmpleados()
        {
            string rol = SesionState.usuario.tRol.Rol;
            int? idPlanta = SesionState.usuario.IdPlanta;

            var query = db.tUsuario.Where(u => u.Estatus == 1);

            if (rol == "Jefe de Planta" && idPlanta != null)
                query = query.Where(u => u.IdPlanta == idPlanta);

            var lista = query
                .OrderBy(u => u.ApellidoPaterno).ThenBy(u => u.Nombre)
                .Select(u => new
                {
                    u.IdUsuario,
                    NombreCompleto = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno
                }).ToList();

            ddlEmpleado.DataSource = lista;
            ddlEmpleado.DataTextField = "NombreCompleto";
            ddlEmpleado.DataValueField = "IdUsuario";
            ddlEmpleado.DataBind();
            ddlEmpleado.Items.Insert(0, new ListItem("-- Seleccione empleado --", "0"));
        }

        private void CargarRecientes()
        {
            int idCapturador = SesionState.usuario.IdUsuario;
            DateTime desde = DateTime.Now.AddDays(-30).Date;

            var raw = (from m in db.tHorasExtraManual
                       join u in db.tUsuario on m.IdUsuario equals u.IdUsuario
                       where m.IdCapturador == idCapturador && m.Fecha >= desde
                       orderby m.FechaCaptura descending
                       select new
                       {
                           m.IdHorasExtraManual,
                           Empleado = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,
                           m.Fecha,
                           m.HorasExtras,
                           m.Descripcion,
                           m.FechaCaptura,
                           m.EstatusAprobacion
                       }).ToList();

            var datos = raw.Select(m => new
            {
                m.IdHorasExtraManual,
                m.Empleado,
                Fecha = m.Fecha.ToString("dd/MM/yyyy"),
                HorasFormato = FormatearHoras(m.HorasExtras),
                m.Descripcion,
                FechaCaptura = m.FechaCaptura.ToString("dd/MM/yyyy HH:mm"),
                EstatusTexto = m.EstatusAprobacion == 2 ? "Aprobado"
                             : m.EstatusAprobacion == 3 ? "Rechazado"
                             : "Pendiente",
                m.EstatusAprobacion
            }).ToList();

            gvRecientes.DataSource = datos;
            gvRecientes.DataBind();
        }

        protected void btnRegistrar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!int.TryParse(ddlEmpleado.SelectedValue, out int idEmpleado) || idEmpleado == 0)
                {
                    MostrarAlerta("warning", "Atención", "Seleccione un empleado.");
                    return;
                }

                if (!DateTime.TryParse(txtFecha.Text, out DateTime fecha))
                {
                    MostrarAlerta("warning", "Atención", "Ingrese una fecha válida.");
                    return;
                }

                if (fecha.Date > DateTime.Now.Date)
                {
                    MostrarAlerta("warning", "Atención", "La fecha no puede ser futura.");
                    return;
                }

                if (!int.TryParse(txtHorasH.Text.Trim(), out int horasEnteras) || horasEnteras < 0 || horasEnteras > 23)
                {
                    MostrarAlerta("warning", "Atención", "Ingrese las horas (valor entre 0 y 23).");
                    return;
                }

                int minutosEnteros = int.Parse(ddlMinutos.SelectedValue);
                decimal horas = horasEnteras + (minutosEnteros / 60m);

                if (horas <= 0)
                {
                    MostrarAlerta("warning", "Atención", "Debe ingresar al menos 5 minutos de horas extra.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtDescripcion.Text))
                {
                    MostrarAlerta("warning", "Atención", "Ingrese una descripción o motivo.");
                    return;
                }

                var empleado = db.tUsuario.FirstOrDefault(u => u.IdUsuario == idEmpleado);
                if (empleado == null || empleado.IdPlanta == null)
                {
                    MostrarAlerta("error", "Error", "El empleado no tiene planta asignada.");
                    return;
                }

                var nuevo = new tHorasExtraManual
                {
                    IdUsuario = idEmpleado,
                    IdPlanta = empleado.IdPlanta.Value,
                    Fecha = fecha.Date,
                    HorasExtras = horas,
                    Descripcion = txtDescripcion.Text.Trim(),
                    IdCapturador = SesionState.usuario.IdUsuario,
                    FechaCaptura = DateTime.Now,
                    EstatusAprobacion = 1
                };

                db.tHorasExtraManual.InsertOnSubmit(nuevo);
                db.SubmitChanges();

                LimpiarFormulario();
                CargarRecientes();
                MostrarAlerta("success", "Registrado", "Las horas extra se registraron correctamente y quedan pendientes de aprobación.");
            }
            catch (Exception ex)
            {
                MostrarAlerta("error", "Error", ex.Message);
            }
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        protected void gvRecientes_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Eliminar") return;

            try
            {
                int id = Convert.ToInt32(e.CommandArgument);
                var reg = db.tHorasExtraManual.FirstOrDefault(m => m.IdHorasExtraManual == id);

                if (reg == null) return;
                if (reg.EstatusAprobacion != 1)
                {
                    MostrarAlerta("warning", "No permitido", "Solo se pueden eliminar registros en estado Pendiente.");
                    return;
                }
                if (reg.IdCapturador != SesionState.usuario.IdUsuario)
                {
                    MostrarAlerta("warning", "No permitido", "Solo puede eliminar registros que usted capturó.");
                    return;
                }

                db.tHorasExtraManual.DeleteOnSubmit(reg);
                db.SubmitChanges();
                CargarRecientes();
                MostrarAlerta("success", "Eliminado", "El registro fue eliminado.");
            }
            catch (Exception ex)
            {
                MostrarAlerta("error", "Error", ex.Message);
            }
        }

        protected void gvRecientes_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            int estatus = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "EstatusAprobacion"));
            if (estatus == 2)
                e.Row.CssClass = "table-success";
            else if (estatus == 3)
                e.Row.CssClass = "table-danger";
        }

        private void LimpiarFormulario()
        {
            ddlEmpleado.SelectedIndex = 0;
            txtFecha.Text = DateTime.Now.ToString("yyyy-MM-dd");
            txtHorasH.Text = "";
            ddlMinutos.SelectedIndex = 0;
            txtDescripcion.Text = "";
        }

        private string FormatearHoras(decimal horas)
        {
            if (horas <= 0) return "00:00";
            var ts = TimeSpan.FromHours((double)horas);
            return string.Format("{0:D2}:{1:D2}", (int)ts.TotalHours, ts.Minutes);
        }

        private void MostrarAlerta(string icon, string titulo, string mensaje)
        {
            string safe = mensaje.Replace("'", "\\'");
            string script = string.Format(@"
                Swal.fire({{
                    icon: '{0}',
                    title: '{1}',
                    text: '{2}',
                    showConfirmButton: true
                }});", icon, titulo, safe);
            ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), script, true);
        }
    }
}
