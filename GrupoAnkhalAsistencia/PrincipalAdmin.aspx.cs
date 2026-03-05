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
    public partial class PrincipalAdmin : System.Web.UI.Page
    {
        dbAsistenciaDataContext db = new dbAsistenciaDataContext(
           System.Configuration.ConfigurationManager
           .ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        // Enum para los tipos de filtro
        private enum TipoFiltro
        {
            Todos,
            ATiempo,
            Retardo,
            Faltaron
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // ¿Sesion válida?
            if (SesionState.usuario == null)
            {
                SesionState.usuario = null;
                Response.Redirect("login.aspx");
                return;
            }

            string rolUsuario = SesionState.usuario.tRol.Rol;
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
                CargarDashboard();
                CargarAsistenciaHoy();
                ActualizarContadorRegistros();
            }
        }

        private void CargarDashboard()
        {
            DateTime hoy = DateTime.Today;

            // Total empleados registrados en el sistema
            int totalEmpleados = db.tUsuario.Where(u => u.Estatus == 1).Count();

            // Llegaron a tiempo
            int llegaronTiempo = db.tAsistencia
                .Where(a => a.Fecha == hoy &&
                       (a.EstatusEntrada == "A TIEMPO" || a.EstatusEntrada == "A tiempo"))
                .Count();

            // Llegaron tarde
            int llegaronTarde = db.tAsistencia
                .Where(a => a.Fecha == hoy &&
                       (a.EstatusEntrada == "RETARDO" || a.EstatusEntrada == "Retardo"))
                .Count();

            //    (ya no calcula empleados sin registro, ahora existen filas reales de falta)
            int faltaron = db.tAsistencia
                .Where(a => a.Fecha == hoy &&
                       (a.EstatusEntrada == "Falta" || a.EstatusEntrada == "FALTA"))
                .Count();

            lblTotalEmpleados.Text = totalEmpleados.ToString();
            lblLlegaronTiempo.Text = llegaronTiempo.ToString();
            lblLlegaronTarde.Text = llegaronTarde.ToString();
            lblFaltaron.Text = faltaron.ToString();
        }

        private void CargarAsistenciaHoy(string filtro = "", TipoFiltro tipoFiltro = TipoFiltro.Todos)
        {
            DateTime hoy = DateTime.Today;

            var query = from a in db.tAsistencia
                        join u in db.tUsuario on a.IdUsuario equals u.IdUsuario
                        join p in db.tPlanta on a.IdPlanta equals p.IdPlanta into plantaJoin
                        from p in plantaJoin.DefaultIfEmpty()
                        where a.Fecha == hoy
                        orderby a.HoraEntrada
                        select new
                        {
                            a.IdUsuario,
                            Empleado = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,
                            Planta = p.Planta ?? "Sin planta",
                            a.Fecha,
                            a.HoraEntrada,
                            a.HoraSalidaComer,
                            a.HoraEntradaComer,
                            a.HoraSalida,
                            a.EstatusEntrada,
                            a.EstatusComida,
                            a.EstatusSalida,
                            UbicacionEntrada = (a.latitud != null && a.longitud != null)
                                ? a.latitud.ToString() + ", " + a.longitud.ToString()
                                : "",
                            UbicacionSalida = (a.latitudSalida != null && a.longitudSalida != null)
                                ? a.latitudSalida.ToString() + ", " + a.longitudSalida.ToString()
                                : ""
                        };

            // ✅ CORREGIDO: Faltaron ahora filtra igual que los demás,
            //    ya no necesita lógica especial porque existen registros reales con EstatusEntrada = 'Falta'
            switch (tipoFiltro)
            {
                case TipoFiltro.ATiempo:
                    query = query.Where(x => x.EstatusEntrada == "A TIEMPO" || x.EstatusEntrada == "A tiempo");
                    break;
                case TipoFiltro.Retardo:
                    query = query.Where(x => x.EstatusEntrada == "RETARDO" || x.EstatusEntrada == "Retardo");
                    break;
                case TipoFiltro.Faltaron:
                    query = query.Where(x => x.EstatusEntrada == "Falta" || x.EstatusEntrada == "FALTA");
                    break;
            }

            // Filtro por búsqueda de nombre
            if (!string.IsNullOrWhiteSpace(filtro))
                query = query.Where(x => x.Empleado.Contains(filtro));

            gvAsistenciaHoy.DataSource = query.ToList();
            gvAsistenciaHoy.DataBind();
            ActualizarContadorRegistros();
        }

        public string GetMapaLink(string ubicacion)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ubicacion))
                    return "";

                var partes = ubicacion.Split(',');
                if (partes.Length != 2)
                    return "";

                string lat = partes[0].Trim();
                string lng = partes[1].Trim();

                decimal latNum, lngNum;
                if (!decimal.TryParse(lat, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out latNum) ||
                    !decimal.TryParse(lng, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out lngNum))
                    return "";

                if (latNum == 0 && lngNum == 0)
                    return "";

                string url = $"https://www.google.com/maps?q={lat},{lng}";
                return $"<a href='{url}' target='_blank'><img src='/img/mapa.png' width='25' /></a>";
            }
            catch
            {
                return "";
            }
        }

        protected void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            TipoFiltro filtroActual = ObtenerFiltroActual();
            CargarAsistenciaHoy(txtBuscar.Text.Trim(), filtroActual);
        }

        protected void gvAsistenciaHoy_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvAsistenciaHoy.PageIndex = e.NewPageIndex;
            TipoFiltro filtroActual = ObtenerFiltroActual();
            CargarAsistenciaHoy(txtBuscar.Text.Trim(), filtroActual);
        }

        // ========== EVENTOS DE LOS CARDS ==========
        protected void lnkTotalEmpleados_Click(object sender, EventArgs e)
        {
            GuardarFiltroActual(TipoFiltro.Todos);
            txtBuscar.Text = "";
            CargarAsistenciaHoy("", TipoFiltro.Todos);
            MostrarFiltroActivo("Todos los empleados", "cardTotal");
        }

        protected void lnkLlegaronTiempo_Click(object sender, EventArgs e)
        {
            GuardarFiltroActual(TipoFiltro.ATiempo);
            txtBuscar.Text = "";
            CargarAsistenciaHoy("", TipoFiltro.ATiempo);
            MostrarFiltroActivo("Empleados que llegaron a tiempo", "cardTiempo");
        }

        protected void lnkLlegaronTarde_Click(object sender, EventArgs e)
        {
            GuardarFiltroActual(TipoFiltro.Retardo);
            txtBuscar.Text = "";
            CargarAsistenciaHoy("", TipoFiltro.Retardo);
            MostrarFiltroActivo("Empleados que llegaron tarde", "cardTarde");
        }

        protected void lnkFaltaron_Click(object sender, EventArgs e)
        {
            GuardarFiltroActual(TipoFiltro.Faltaron);
            txtBuscar.Text = "";
            CargarAsistenciaHoy("", TipoFiltro.Faltaron);
            MostrarFiltroActivo("Empleados que faltaron", "cardFaltaron");
        }

        protected void btnLimpiarFiltro_Click(object sender, EventArgs e)
        {
            GuardarFiltroActual(TipoFiltro.Todos);
            txtBuscar.Text = "";
            CargarAsistenciaHoy("", TipoFiltro.Todos);
            pnlFiltroActivo.Visible = false;
            ResaltarCard("");
        }

        // ========== MÉTODOS AUXILIARES ==========
        private void GuardarFiltroActual(TipoFiltro filtro)
        {
            ViewState["FiltroActual"] = filtro;
        }

        private TipoFiltro ObtenerFiltroActual()
        {
            if (ViewState["FiltroActual"] != null)
                return (TipoFiltro)ViewState["FiltroActual"];
            return TipoFiltro.Todos;
        }

        private void MostrarFiltroActivo(string textoFiltro, string cardId)
        {
            if (ObtenerFiltroActual() == TipoFiltro.Todos)
                pnlFiltroActivo.Visible = false;
            else
            {
                pnlFiltroActivo.Visible = true;
                lblFiltroActivo.Text = textoFiltro;
            }

            ResaltarCard(cardId);
        }

        private void ResaltarCard(string cardId)
        {
            string script = $@"
            <script>
                $(document).ready(function() {{
                    $('.card-info').removeClass('active');
                    if ('{cardId}' !== '') {{
                        $('#{cardId}').addClass('active');
                    }}
                }});
            </script>";

            ltScriptCard.Text = script;
        }

        private void ActualizarContadorRegistros()
        {
            lblTotalRegistros.Text = gvAsistenciaHoy.Rows.Count.ToString();
        }
    }
}