using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class Justificacion : System.Web.UI.Page
    {
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
           ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        public int UsuarioSesion;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (SesionState.usuario == null)
            {
                Response.Redirect("login.aspx");
                return;
            }

            UsuarioSesion = SesionState.usuario.IdUsuario;

            if (!IsPostBack)
            {
                // Por defecto mostrar el mes actual
                txtFechaInicio.Text = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("yyyy-MM-dd");
                txtFechaFin.Text = DateTime.Now.ToString("yyyy-MM-dd");
                CargarJustificacion();
            }
        }

        // -------------------------------------------------------
        // Helpers para el GridView (llamados desde el ASPX)
        // -------------------------------------------------------

        /// <summary>
        /// Devuelve el HTML del badge según el estado de la justificación.
        /// </summary>
        public string ObtenerBadgeJustificacion(object justificacion, object estatusObj)
        {
            string valor = justificacion?.ToString() ?? "";
            int estatus = 0;
            int.TryParse(estatusObj?.ToString(), out estatus);

            if (string.IsNullOrEmpty(valor) || valor == "Sin justificar")
                return "<span class='badge-pendiente'>Sin solicitar</span>";

            if (estatus == 1)
                return "<span class='badge-pendiente'>Pendiente</span>";
            if (estatus == 2)
                return "<span class='badge-aceptada'>Aceptada</span>";
            if (estatus == 3)
                return "<span class='badge-rechazada'>Rechazada</span>";

            return "<span class='badge-pendiente'>Pendiente</span>";
        }

        /// <summary>
        /// Determina si se puede enviar una nueva solicitud (solo si no hay ninguna pendiente/aceptada).
        /// </summary>
        public bool PuedeJustificar(object justificacion, object estatusObj)
        {
            string valor = justificacion?.ToString() ?? "";
            int estatus = 0;
            int.TryParse(estatusObj?.ToString(), out estatus);

            // Puede justificar si: no tiene justificación, o la anterior fue rechazada
            if (string.IsNullOrEmpty(valor) || valor == "Sin justificar")
                return true;

            if (estatus == 3) // Rechazada → puede reintentar
                return true;

            return false; // Pendiente (1) o Aceptada (2) → no puede
        }

        // -------------------------------------------------------
        // Carga de datos
        // -------------------------------------------------------

        private void CargarJustificacion()
        {
            DateTime fechaInicio, fechaFin;
            bool tieneInicio = DateTime.TryParse(txtFechaInicio.Text, out fechaInicio);
            bool tieneFin = DateTime.TryParse(txtFechaFin.Text, out fechaFin);

            string tipoFiltro = ddlTipoFiltro.SelectedValue;

            // Consulta base: asistencias del usuario con retardo o falta
            var query = from a in db.tAsistencia
                        join ah in db.tAsignarHorario on a.IdAsignarHorario equals ah.IdAsignarHorario
                        join h in db.tHorario on ah.IdHorario equals h.IdHorario
                        join u in db.tUsuario on a.IdUsuario equals u.IdUsuario
                        where a.IdUsuario == UsuarioSesion
                           && (a.EstatusEntrada == "Retardo" || a.EstatusEntrada == "Falta")
                        select new
                        {
                            a.IdAsistencia,
                            a.Fecha,
                            Horario = h.HoraInicio.ToString().Substring(0, 5) + " - " + h.HoraFin.ToString().Substring(0, 5),
                            a.HoraEntrada,
                            a.HoraSalida,
                            a.EstatusEntrada,
                            // Justificación: tomamos la última del registro si existe
                            Justificacion = a.Justificacion ?? "Sin justificar",
                            EstatusJustificacion = (int?)db.tJustificacion
                                                        .Where(j => j.IdAsistencia == a.IdAsistencia && j.IdUsuario == UsuarioSesion)
                                                        .OrderByDescending(j => j.IdJustificacion)
                                                        .Select(j => j.Estatus)
                                                        .FirstOrDefault(),
                            MotivoJustificacion = db.tJustificacion
                                                        .Where(j => j.IdAsistencia == a.IdAsistencia && j.IdUsuario == UsuarioSesion)
                                                        .OrderByDescending(j => j.IdJustificacion)
                                                        .Select(j => j.Motivo)
                                                        .FirstOrDefault()
                        };

            // Filtros opcionales
            if (tieneInicio)
                query = query.Where(m => m.Fecha >= fechaInicio);

            if (tieneFin)
                query = query.Where(m => m.Fecha <= fechaFin);

            if (!string.IsNullOrEmpty(tipoFiltro))
                query = query.Where(m => m.EstatusEntrada == tipoFiltro);

            dvgJustificaionHoras.DataSource = query.OrderByDescending(m => m.Fecha).ToList();
            dvgJustificaionHoras.DataBind();
        }

        // -------------------------------------------------------
        // Eventos de UI
        // -------------------------------------------------------

        protected void btnBuscar_Click(object sender, EventArgs e)
        {
            dvgJustificaionHoras.PageIndex = 0;
            CargarJustificacion();
        }

        protected void dvgJustificaion_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dvgJustificaionHoras.PageIndex = e.NewPageIndex;
            CargarJustificacion();
        }

        /// <summary>
        /// Al hacer clic en "Solicitar Justificación" abre el modal con los datos del registro.
        /// </summary>
        protected void btnJustificar_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int idAsistencia = Convert.ToInt32(btn.CommandArgument);

            // Guardamos el IdAsistencia en el HiddenField
            hfIdAsistencia.Value = idAsistencia.ToString();

            // Rellenar datos del modal
            var asistencia = (from a in db.tAsistencia
                              join ah in db.tAsignarHorario on a.IdAsignarHorario equals ah.IdAsignarHorario
                              where a.IdAsistencia == idAsistencia
                              select new { a.Fecha, a.EstatusEntrada }).FirstOrDefault();

            var usuario = SesionState.usuario;
            lblNombreEmpleado.Text = usuario.Nombre + " " + usuario.ApellidoPaterno + " " + usuario.ApellidoMaterno;

            if (asistencia != null)
            {
                lblFechaRegistro.Text = asistencia.Fecha?.ToString("dd/MM/yyyy") ?? "";
                lblTipoRegistro.Text = asistencia.EstatusEntrada ?? "";
            }

            // Limpiar campos
            ddlMotivo.SelectedIndex = 0;
            txtComentarios.Text = "";

            ScriptManager.RegisterStartupScript(this, GetType(), "ShowModal", "abrirModalJustificar();", true);
        }

        /// <summary>
        /// Guarda la justificación en tJustificacion y marca el registro de asistencia como "Pendiente".
        /// </summary>
        protected void btnGuardarJustificacion_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ddlMotivo.SelectedValue))
            {
                string warn = "Swal.fire({ icon: 'warning', title: 'Campo obligatorio', text: 'Debes seleccionar un motivo.' });";
                ScriptManager.RegisterStartupScript(this, GetType(), "alertWarn", warn, true);
                ScriptManager.RegisterStartupScript(this, GetType(), "ShowModalAgain", "abrirModalJustificar();", true);
                return;
            }

            int idAsistencia = Convert.ToInt32(hfIdAsistencia.Value);

            // Verificar que no exista ya una justificación pendiente o aceptada para este registro
            bool yaExiste = db.tJustificacion.Any(j =>
                j.IdAsistencia == idAsistencia &&
                j.IdUsuario == UsuarioSesion &&
                (j.Estatus == 1 || j.Estatus == 2));

            if (yaExiste)
            {
                string warn = "Swal.fire({ icon: 'info', title: 'Ya existe', text: 'Este registro ya tiene una justificación pendiente o aceptada.' });";
                ScriptManager.RegisterStartupScript(this, GetType(), "alertExiste", warn, true);
                return;
            }

            try
            {
                // Insertar en tJustificacion
                tJustificacion nueva = new tJustificacion
                {
                    IdAsistencia = idAsistencia,
                    IdUsuario = UsuarioSesion,
                    Fecha = DateTime.Now,
                    Motivo = ddlMotivo.SelectedValue,
                    Observaciones = txtComentarios.Text.Trim(),
                    Estatus = 1  // 1 = Pendiente
                };
                db.tJustificacion.InsertOnSubmit(nueva);

                // Actualizar campo Justificacion en tAsistencia → "Pendiente"
                var asistencia = db.tAsistencia.FirstOrDefault(a => a.IdAsistencia == idAsistencia);
                if (asistencia != null)
                    asistencia.Justificacion = "Pendiente";

                db.SubmitChanges();

                ScriptManager.RegisterStartupScript(this, GetType(), "HideModal", "cerrarModalJustificar();", true);
                CargarJustificacion();

                string ok = @"Swal.fire({
                    icon: 'success',
                    title: 'Solicitud enviada',
                    text: 'Tu solicitud de justificación fue enviada. Espera la respuesta de RH.',
                    showConfirmButton: false,
                    timer: 2500
                });";
                ScriptManager.RegisterStartupScript(this, GetType(), "alertOk", ok, true);
            }
            catch (Exception ex)
            {
                string err = $"Swal.fire({{ icon: 'error', title: 'Error', text: 'No se pudo guardar: {ex.Message.Replace("'", "")}' }});";
                ScriptManager.RegisterStartupScript(this, GetType(), "alertErr", err, true);
            }
        }
    }
}