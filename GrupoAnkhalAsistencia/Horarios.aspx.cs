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
    public partial class Horarios : System.Web.UI.Page
    {
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
        ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        // ── ViewState para mantener el filtro activo al paginar ──────────────────
        private string FiltroActual
        {
            get { return ViewState["FiltroHorario"] as string ?? ""; }
            set { ViewState["FiltroHorario"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarHorario();
            }
        }

        // ── Carga principal (sin filtro) ─────────────────────────────────────────
        private void CargarHorario()
        {
            var query = from t in db.tHorario
                        where t.Estatus == 1
                        select t;

            dvgHorario.DataSource = query.ToList();
            dvgHorario.DataBind();
        }

        // ── Carga con filtro opcional ────────────────────────────────────────────
        private void CargarHorarioFiltrado(string filtro = "")
        {
            var query = from t in db.tHorario
                        where t.Estatus == 1
                        select t;

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                query = query.Where(x =>
                    System.Data.Linq.SqlClient.SqlMethods.Like(x.Descripcion, "%" + filtro + "%"));
            }

            dvgHorario.DataSource = query.ToList();
            dvgHorario.DataBind();
        }

        // ── Búsqueda ─────────────────────────────────────────────────────────────
        protected void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            FiltroActual = txtBuscar.Text.Trim();   // guardar para que la paginación lo recuerde
            dvgHorario.PageIndex = 0;               // volver a la primera página al buscar
            CargarHorarioFiltrado(FiltroActual);
        }

        // ── Paginación ───────────────────────────────────────────────────────────
        protected void dvgHorario_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dvgHorario.PageIndex = e.NewPageIndex;
            CargarHorarioFiltrado(FiltroActual);    // mantiene el filtro guardado en ViewState
        }

        // ── Guardar nuevo horario ────────────────────────────────────────────────
        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDescripcion.Text))
            {
                string script = "Swal.fire({ icon: 'warning', title: 'Campo obligatorio', text: 'El campo descripcion es obligatorio.' });";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertDesc", script, true);
                return;
            }

            if (!TimeSpan.TryParse(txtHoraInicio.Value, out TimeSpan inicio) ||
                !TimeSpan.TryParse(txtHoraFin.Value, out TimeSpan fin))
            {
                string script = "Swal.fire({ icon: 'warning', title: 'Horas inválidas', text: 'Ingrese horas de inicio y fin válidas.' });";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertHoras", script, true);
                return;
            }

            var duplicadoPeriodo = db.tHorario
                .Where(h => h.Estatus == 1 && h.HoraInicio == inicio && h.HoraFin == fin)
                .Select(h => h.Descripcion)
                .FirstOrDefault();

            if (duplicadoPeriodo != null)
            {
                string msg = duplicadoPeriodo.Replace("'", "\\'");
                string script = $"Swal.fire({{ icon: 'error', title: 'Período duplicado', text: 'Ya existe el horario \\'{msg}\\' con ese período de tiempo.' }});";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertPeriodo", script, true);
                return;
            }

            bool existe = db.tHorario.Any(p =>
                p.Descripcion == txtDescripcion.Text.Trim() && p.Estatus == 1);

            if (existe)
            {
                string script = @"
                    Swal.fire({
                        icon: 'error',
                        title: 'Duplicado',
                        text: 'Ya existe un registro con esa descripción.'
                    }).then(() => {
                        document.getElementById('" + txtDescripcion.ClientID + @"').value = '';
                    });";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "SweetAlert", script, true);
                return;
            }

            tHorario pue = new tHorario
            {
                HoraInicio = inicio,
                HoraFin = fin,
                Descripcion = txtDescripcion.Text.Trim(),
                Estatus = 1
            };

            db.tHorario.InsertOnSubmit(pue);
            db.SubmitChanges();
            limpiar();
            CargarHorarioFiltrado(FiltroActual);

            string successScript = @"
                Swal.fire({
                    icon: 'success',
                    title: 'Guardado',
                    text: 'El horario se guardó correctamente.',
                    showConfirmButton: false,
                    timer: 2000
                });";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "alertSuccess", successScript, true);
        }

        // ── Guardar edición ──────────────────────────────────────────────────────
        protected void btnGuardarModal_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DescripcionModal.Text))
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(),
                    "Swal.fire({ icon: 'warning', title: 'Campo obligatorio', text: 'La descripción es obligatoria.' });", true);
                return;
            }

            if (!TimeSpan.TryParse(HorarioInicioModal.Value, out TimeSpan inicio) ||
                !TimeSpan.TryParse(HorarioFinModal.Value, out TimeSpan fin))
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(),
                    "Swal.fire({ icon: 'warning', title: 'Horas inválidas', text: 'Ingrese horas de inicio y fin válidas.' });", true);
                return;
            }

            int id = Convert.ToInt32(hfIdHorario.Value);

            var duplicadoPeriodo = db.tHorario
                .Where(h => h.Estatus == 1 && h.IdHorario != id && h.HoraInicio == inicio && h.HoraFin == fin)
                .Select(h => h.Descripcion)
                .FirstOrDefault();

            if (duplicadoPeriodo != null)
            {
                string msg = duplicadoPeriodo.Replace("'", "\\'");
                string alertScript = $"Swal.fire({{ icon: 'error', title: 'Período duplicado', text: 'Ya existe el horario \\'{msg}\\' con ese período de tiempo.' }});";
                ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(), alertScript, true);
                return;
            }

            var pue = db.tHorario.FirstOrDefault(t => t.IdHorario == id);

            if (pue != null)
            {
                pue.HoraInicio = inicio;
                pue.HoraFin = fin;
                pue.Descripcion = DescripcionModal.Text.Trim();

                db.SubmitChanges();

                ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(),
                    "$('#modalEditar').modal('hide');", true);

                CargarHorarioFiltrado(FiltroActual);

                string successScript = @"
                    Swal.fire({
                        icon: 'success',
                        title: 'Actualizado',
                        text: 'El horario se actualizó correctamente.',
                        showConfirmButton: false,
                        timer: 2000
                    });";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertEdit", successScript, true);
            }
        }

        // ── Eliminar (cambio de estatus) ─────────────────────────────────────────
        protected void btnEliminar_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int id = Convert.ToInt32(btn.CommandArgument);

            var pue = db.tHorario.FirstOrDefault(t => t.IdHorario == id);
            if (pue != null)
            {
                pue.Estatus = 0;
                db.SubmitChanges();
                CargarHorarioFiltrado(FiltroActual);

                string script = @"
                    Swal.fire({
                        icon: 'success',
                        title: 'Desactivado',
                        text: 'El horario se desactivó correctamente.',
                        showConfirmButton: false,
                        timer: 2000
                    });";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertDesactivar", script, true);
            }
        }

        // ── Limpiar formulario de agregar ────────────────────────────────────────
        public void limpiar()
        {
            txtHoraInicio.Value = "";
            txtHoraFin.Value = "";
            txtDescripcion.Text = "";
        }
    }
}