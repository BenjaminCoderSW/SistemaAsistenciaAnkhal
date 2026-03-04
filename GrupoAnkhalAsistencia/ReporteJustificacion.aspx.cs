using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Configuration;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class ReporteJustificacion : System.Web.UI.Page
    {
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
            ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        // -------------------------------------------------------
        // Helper para badge (llamado desde el ASPX)
        // -------------------------------------------------------
        public string ObtenerBadge(string estatus)
        {
            switch (estatus)
            {
                case "1": return "<span class='badge-pendiente'>Pendiente</span>";
                case "2": return "<span class='badge-aceptada'>Aceptada</span>";
                case "3": return "<span class='badge-rechazada'>Rechazada</span>";
                default: return "<span>—</span>";
            }
        }

        // -------------------------------------------------------
        // Ciclo de vida
        // -------------------------------------------------------
        protected void Page_Load(object sender, EventArgs e)
        {
            if (SesionState.usuario == null)
            {
                Response.Redirect("login.aspx"); return;
            }

            string rol = SesionState.usuario.tRol.Rol;
            if (rol != "Administrador" && rol != "Rh")
            {
                Response.Redirect("login.aspx"); return;
            }

            if (!IsPostBack)
            {
                // Período por defecto: mes actual
                txtFechaInicio.Text = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("yyyy-MM-dd");
                txtFechaFin.Text = DateTime.Now.ToString("yyyy-MM-dd");
                CargarReporte();
            }
        }

        // -------------------------------------------------------
        // Carga de datos
        // -------------------------------------------------------
        private void CargarReporte()
        {
            DateTime fechaInicio, fechaFin;
            bool tieneInicio = DateTime.TryParse(txtFechaInicio.Text, out fechaInicio);
            bool tieneFin = DateTime.TryParse(txtFechaFin.Text, out fechaFin);
            int estatusFiltro;
            int.TryParse(ddlEstatus.SelectedValue, out estatusFiltro);
            string tipofiltro = ddlTipo.SelectedValue;
            string busqueda = txtBuscar.Text.Trim();

            var query = from j in db.tJustificacion
                        join a in db.tAsistencia on j.IdAsistencia equals a.IdAsistencia
                        join u in db.tUsuario on j.IdUsuario equals u.IdUsuario
                        join ah in db.tAsignarHorario on a.IdAsignarHorario equals ah.IdAsignarHorario into ahJoin
                        from ah in ahJoin.DefaultIfEmpty()
                        join h in db.tHorario on (ah != null ? ah.IdHorario : 0) equals h.IdHorario into hJoin
                        from h in hJoin.DefaultIfEmpty()
                        join p in db.tPlanta on u.IdPlanta equals p.IdPlanta into pJoin
                        from p in pJoin.DefaultIfEmpty()
                        select new
                        {
                            NombreCompleto = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,
                            u.NumeroEmpleado,
                            Planta = p != null ? p.Planta : "Sin asignar",
                            FechaAsistencia = a.Fecha,
                            TipoRegistro = a.EstatusEntrada,
                            HoraEntrada = a.HoraEntrada,
                            HoraProgramada = h != null ? h.HoraInicio : (TimeSpan?)null,
                            j.Motivo,
                            j.Observaciones,
                            FechaSolicitud = j.Fecha,
                            Estatus = j.Estatus
                        };

            // Filtros
            if (tieneInicio) query = query.Where(x => x.FechaAsistencia >= fechaInicio);
            if (tieneFin) query = query.Where(x => x.FechaAsistencia <= fechaFin);
            if (estatusFiltro > 0) query = query.Where(x => x.Estatus == estatusFiltro);
            if (!string.IsNullOrEmpty(tipofiltro)) query = query.Where(x => x.TipoRegistro == tipofiltro);
            if (!string.IsNullOrEmpty(busqueda))
                query = query.Where(x =>
                    System.Data.Linq.SqlClient.SqlMethods.Like(x.NombreCompleto, "%" + busqueda + "%") ||
                    System.Data.Linq.SqlClient.SqlMethods.Like(x.NumeroEmpleado, "%" + busqueda + "%"));

            var lista = query.OrderByDescending(x => x.FechaSolicitud).ToList();

            // Actualizar contadores (sobre TODA la base, no solo la página actual)
            ActualizarResumen();

            dvgReporte.DataSource = lista;
            dvgReporte.DataBind();
        }

        private void ActualizarResumen()
        {
            lblTotalPendientes.Text = db.tJustificacion.Count(j => j.Estatus == 1).ToString();
            lblTotalAceptadas.Text = db.tJustificacion.Count(j => j.Estatus == 2).ToString();
            lblTotalRechazadas.Text = db.tJustificacion.Count(j => j.Estatus == 3).ToString();
            lblTotal.Text = db.tJustificacion.Count().ToString();
        }

        // -------------------------------------------------------
        // Eventos
        // -------------------------------------------------------
        protected void btnBuscar_Click(object sender, EventArgs e)
        {
            dvgReporte.PageIndex = 0;
            CargarReporte();
        }

        protected void dvgReporte_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dvgReporte.PageIndex = e.NewPageIndex;
            CargarReporte();
        }
    }
}