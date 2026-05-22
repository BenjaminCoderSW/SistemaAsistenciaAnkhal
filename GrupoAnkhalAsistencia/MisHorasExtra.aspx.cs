using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Configuration;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class MisHorasExtra : System.Web.UI.Page
    {
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
            ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (SesionState.usuario == null) { Response.Redirect("login.aspx"); return; }

            if (!IsPostBack)
            {
                txtFechaInicio.Text = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
                txtFechaFin.Text = DateTime.Now.ToString("yyyy-MM-dd");
                CargarMisHorasExtra();
            }
        }

        private void CargarMisHorasExtra()
        {
            if (!DateTime.TryParse(txtFechaInicio.Text, out DateTime fi) ||
                !DateTime.TryParse(txtFechaFin.Text, out DateTime ff))
                return;

            int idUsuario = SesionState.usuario.IdUsuario;

            var datos = (from a in db.tAsistencia
                         join ap in db.tAprobacionHorasExtra on a.IdAsistencia equals ap.IdAsistencia
                         join aprobador in db.tUsuario on ap.IdAprobador equals aprobador.IdUsuario into apg
                         from aprobador in apg.DefaultIfEmpty()
                         where a.IdUsuario == idUsuario
                            && ap.EstatusAprobacion == 2
                            && a.HorasExtras > 0
                            && a.Fecha >= fi.Date
                            && a.Fecha <= ff.Date
                         orderby a.Fecha descending
                         select new
                         {
                             a.IdAsistencia,
                             FechaRaw = a.Fecha,
                             HorasRedondeadas = RedondearA30Min(a.HorasExtras),
                             Tipo = a.EstatusHorasExtras,
                             ap.Motivo,
                             AprobadoPor = aprobador != null
                                 ? aprobador.Nombre + " " + aprobador.ApellidoPaterno
                                 : "-",
                             ap.FechaAprobacion
                         }).ToList();

            var vista = datos
                .Where(x => x.HorasRedondeadas > 0)
                .Select(x => new
                {
                    x.IdAsistencia,
                    Fecha = x.FechaRaw.HasValue ? x.FechaRaw.Value.ToString("dd/MM/yyyy") : "",
                    HorasExtra = FormatearHoras(x.HorasRedondeadas),
                    Tipo = string.IsNullOrEmpty(x.Tipo) ? "-" : x.Tipo,
                    Motivo = string.IsNullOrEmpty(x.Motivo) ? "-" : x.Motivo,
                    x.AprobadoPor,
                    FechaAprobacion = x.FechaAprobacion.HasValue
                        ? x.FechaAprobacion.Value.ToString("dd/MM/yyyy") : "-",
                    x.HorasRedondeadas
                }).ToList();

            decimal total = vista.Sum(x => x.HorasRedondeadas);
            lblTotalAprobadas.Text = "Total aprobadas en el periodo: " + FormatearHoras(total) + " hrs";

            gvMisHorasExtra.DataSource = vista;
            gvMisHorasExtra.DataBind();
        }

        private decimal RedondearA30Min(decimal? horas)
        {
            if (horas == null || horas <= 0) return 0;
            return (decimal)(Math.Floor((double)horas * 2) / 2);
        }

        private string FormatearHoras(decimal horas)
        {
            if (horas <= 0) return "00:00";
            var ts = TimeSpan.FromHours((double)horas);
            return string.Format("{0:D2}:{1:D2}", (int)ts.TotalHours, ts.Minutes);
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            gvMisHorasExtra.PageIndex = 0;
            CargarMisHorasExtra();
        }

        protected void btnLimpiarFiltros_Click(object sender, EventArgs e)
        {
            txtFechaInicio.Text = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
            txtFechaFin.Text = DateTime.Now.ToString("yyyy-MM-dd");
            gvMisHorasExtra.PageIndex = 0;
            CargarMisHorasExtra();
        }

        protected void gvMisHorasExtra_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvMisHorasExtra.PageIndex = e.NewPageIndex;
            CargarMisHorasExtra();
        }
    }
}
