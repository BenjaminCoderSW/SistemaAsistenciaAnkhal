using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
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

        private List<tPlanta> _plantas = new List<tPlanta>();

        private enum TipoFiltro
        {
            Todos,
            ATiempo,
            Retardo,
            Faltaron,
            Vacaciones
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

            _plantas = db.tPlanta
                .Where(p => p.latitud != null && p.longitud != null && p.latitud != "" && p.longitud != "")
                .ToList();

            if (!IsPostBack)
            {
                txtFecha.Text = DateTime.Today.ToString("yyyy-MM-dd");
                CargarDashboard();
                CargarAsistenciaHoy();
                ActualizarContadorRegistros();
            }
        }

        private DateTime ObtenerFechaSeleccionada()
        {
            if (DateTime.TryParse(txtFecha.Text, out DateTime fecha))
                return fecha.Date;
            return DateTime.Today;
        }

        protected void btnVerFecha_Click(object sender, EventArgs e)
        {
            GuardarFiltroActual(TipoFiltro.Todos);
            pnlFiltroActivo.Visible = false;
            ResaltarCard("");
            CargarDashboard();
            CargarAsistenciaHoy();
            ActualizarContadorRegistros();
        }

        private void CargarDashboard()
        {
            DateTime hoy = ObtenerFechaSeleccionada();
            string etiqueta = hoy.Date == DateTime.Today.Date ? "Hoy" : hoy.ToString("dd/MM/yyyy");
            hTituloAsistencia.InnerText = $"Resumen de Asistencia ({etiqueta})";

            int totalEmpleados = db.tUsuario.Where(u => u.Estatus == 1).Count();

            int llegaronTiempo = db.tAsistencia
                .Where(a => a.Fecha == hoy &&
                       (a.EstatusEntrada == "A TIEMPO" || a.EstatusEntrada == "A tiempo"))
                .Count();

            int llegaronTarde = db.tAsistencia
                .Where(a => a.Fecha == hoy &&
                       (a.EstatusEntrada == "RETARDO" || a.EstatusEntrada == "Retardo"))
                .Count();

            // Vacaciones = registro con EstatusEntrada = 'Vacaciones'
            int vacaciones = db.tAsistencia
                .Where(a => a.Fecha == hoy && a.EstatusEntrada == "Vacaciones")
                .Count();

            // Sin registro hoy (posible falta, el SP los marcara a las 11:50 PM)
            var idsConRegistroHoy = db.tAsistencia
                .Where(a => a.Fecha == hoy)
                .Select(a => a.IdUsuario)
                .Distinct();

            int faltaron = db.tUsuario
                .Where(u => u.Estatus == 1 && !idsConRegistroHoy.Contains(u.IdUsuario))
                .Count();

            lblTotalEmpleados.Text = totalEmpleados.ToString();
            lblLlegaronTiempo.Text = llegaronTiempo.ToString();
            lblLlegaronTarde.Text = llegaronTarde.ToString();
            lblVacaciones.Text = vacaciones.ToString();
            lblFaltaron.Text = faltaron.ToString();
        }

        private void CargarAsistenciaHoy(string filtro = "", TipoFiltro tipoFiltro = TipoFiltro.Todos)
        {
            DateTime hoy = ObtenerFechaSeleccionada();

            // Caso especial: empleados SIN registro hoy
            if (tipoFiltro == TipoFiltro.Faltaron)
            {
                var idsConRegistro = db.tAsistencia
                    .Where(a => a.Fecha == hoy)
                    .Select(a => a.IdUsuario)
                    .Distinct();

                var sinRegistro = db.tUsuario
                    .Where(u => u.Estatus == 1 && !idsConRegistro.Contains(u.IdUsuario))
                    .Select(u => new
                    {
                        IdAsistencia = 0,
                        u.IdUsuario,
                        Empleado = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,
                        Planta = u.tPlanta != null ? u.tPlanta.Planta : "Sin planta",
                        Fecha = hoy,
                        HoraEntrada = (TimeSpan?)null,
                        HoraSalidaComer = (TimeSpan?)null,
                        HoraEntradaComer = (TimeSpan?)null,
                        HoraSalida = (TimeSpan?)null,
                        EstatusEntrada = "Sin registro",
                        EstatusComida = "",
                        EstatusSalida = "",
                        UbicacionEntrada = "",
                        UbicacionSalida = ""
                    });

                if (!string.IsNullOrWhiteSpace(filtro))
                    sinRegistro = sinRegistro.Where(x => x.Empleado.Contains(filtro));

                ViewState["IdsConFotoEntrada"] = "";
                ViewState["IdsConFotoSalida"]  = "";
                gvAsistenciaHoy.DataSource = sinRegistro.ToList();
                gvAsistenciaHoy.DataBind();
                ActualizarContadorRegistros();
                return;
            }

            var query = from a in db.tAsistencia
                        join u in db.tUsuario on a.IdUsuario equals u.IdUsuario
                        join p in db.tPlanta on a.IdPlanta equals p.IdPlanta into plantaJoin
                        from p in plantaJoin.DefaultIfEmpty()
                        where a.Fecha == hoy
                        orderby a.HoraEntrada
                        select new
                        {
                            a.IdAsistencia,
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

            switch (tipoFiltro)
            {
                case TipoFiltro.ATiempo:
                    query = query.Where(x => x.EstatusEntrada == "A TIEMPO" || x.EstatusEntrada == "A tiempo");
                    break;
                case TipoFiltro.Retardo:
                    query = query.Where(x => x.EstatusEntrada == "RETARDO" || x.EstatusEntrada == "Retardo");
                    break;
                case TipoFiltro.Vacaciones:
                    query = query.Where(x => x.EstatusEntrada == "Vacaciones");
                    break;
            }

            if (!string.IsNullOrWhiteSpace(filtro))
                query = query.Where(x => x.Empleado.Contains(filtro));

            var lista = query.ToList();

            var idsConFotoEntrada = new HashSet<int>();
            var idsConFotoSalida  = new HashSet<int>();
            ObtenerDisponibilidadFotos(lista.Select(x => x.IdAsistencia).ToList(),
                                       idsConFotoEntrada, idsConFotoSalida);
            ViewState["IdsConFotoEntrada"] = string.Join(",", idsConFotoEntrada);
            ViewState["IdsConFotoSalida"]  = string.Join(",", idsConFotoSalida);

            gvAsistenciaHoy.DataSource = lista;
            gvAsistenciaHoy.DataBind();
            ActualizarContadorRegistros();
        }

        public string GetPlantaHtml(string ubicacion)
        {
            if (string.IsNullOrWhiteSpace(ubicacion)) return "";
            string nombre = GetNombrePlanta(ubicacion);
            if (string.IsNullOrEmpty(nombre)) return "";
            return $"<br /><small class='text-muted'>{nombre}</small>";
        }

        public string GetSinGpsHtml(string ubicacion, string estatusEntrada)
        {
            bool sinGps = string.IsNullOrWhiteSpace(ubicacion);
            if (!sinGps)
            {
                var partes = ubicacion.Split(',');
                if (partes.Length == 2 &&
                    decimal.TryParse(partes[0].Trim(), System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal lat) &&
                    decimal.TryParse(partes[1].Trim(), System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal lng) &&
                    lat == 0 && lng == 0)
                    sinGps = true;
            }

            if (!sinGps) return "";
            if (string.IsNullOrWhiteSpace(estatusEntrada)) return "";
            if (estatusEntrada == "Sin registro" || estatusEntrada == "Vacaciones" || estatusEntrada == "Falta")
                return "";
            return "<span class='badge badge-warning' title='Este empleado no compartió su ubicación GPS'>" +
                   "<i class='fas fa-exclamation-triangle'></i> Sin GPS</span>";
        }

        public string GetNombrePlanta(string ubicacion)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ubicacion)) return "";
                var partes = ubicacion.Split(',');
                if (partes.Length != 2) return "";

                if (!decimal.TryParse(partes[0].Trim(), System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal lat)) return "";
                if (!decimal.TryParse(partes[1].Trim(), System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal lng)) return "";
                if (lat == 0 && lng == 0) return "";

                const double RADIO_METROS = 100.0;
                foreach (var planta in _plantas)
                {
                    if (!decimal.TryParse(planta.latitud, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out decimal pLat)) continue;
                    if (!decimal.TryParse(planta.longitud, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out decimal pLng)) continue;

                    double distancia = HaversineMetros((double)lat, (double)lng, (double)pLat, (double)pLng);
                    if (distancia <= RADIO_METROS)
                        return planta.Planta;
                }
                return "Ubicación no conocida";
            }
            catch { return ""; }
        }

        private static double HaversineMetros(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000;
            double dLat = (lat2 - lat1) * Math.PI / 180;
            double dLon = (lon2 - lon1) * Math.PI / 180;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                     + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180)
                     * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
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

        protected void lnkVacaciones_Click(object sender, EventArgs e)
        {
            GuardarFiltroActual(TipoFiltro.Vacaciones);
            txtBuscar.Text = "";
            CargarAsistenciaHoy("", TipoFiltro.Vacaciones);
            MostrarFiltroActivo("Empleados de vacaciones hoy", "cardVacaciones");
        }

        protected void lnkFaltaron_Click(object sender, EventArgs e)
        {
            GuardarFiltroActual(TipoFiltro.Faltaron);
            txtBuscar.Text = "";
            CargarAsistenciaHoy("", TipoFiltro.Faltaron);
            MostrarFiltroActivo("Empleados sin registro hoy (posible falta)", "cardFaltaron");
        }

        protected void btnLimpiarFiltro_Click(object sender, EventArgs e)
        {
            GuardarFiltroActual(TipoFiltro.Todos);
            txtBuscar.Text = "";
            CargarAsistenciaHoy("", TipoFiltro.Todos);
            pnlFiltroActivo.Visible = false;
            ResaltarCard("");
        }

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

        private void ObtenerDisponibilidadFotos(List<int> ids,
            HashSet<int> idsEntrada, HashSet<int> idsSalida)
        {
            if (ids == null || ids.Count == 0) return;
            var idsPositivos = ids.Where(i => i > 0).ToList();
            if (idsPositivos.Count == 0) return;

            string idList = string.Join(",", idsPositivos);
            string connStr = System.Configuration.ConfigurationManager
                .ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString;

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                string sql = $@"SELECT IdAsistencia,
                    CASE WHEN FotoEntrada IS NOT NULL THEN 1 ELSE 0 END,
                    CASE WHEN FotoSalida  IS NOT NULL THEN 1 ELSE 0 END
                    FROM tAsistencia WHERE IdAsistencia IN ({idList})";
                using (var cmd = new SqlCommand(sql, conn))
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        int id = rdr.GetInt32(0);
                        if (rdr.GetInt32(1) == 1) idsEntrada.Add(id);
                        if (rdr.GetInt32(2) == 1) idsSalida.Add(id);
                    }
                }
            }
        }

        public string GetFotosHtml(object idObj)
        {
            if (idObj == null) return "";
            int id = Convert.ToInt32(idObj);
            if (id <= 0) return "";

            var e = ParseViewStateIds("IdsConFotoEntrada");
            var s = ParseViewStateIds("IdsConFotoSalida");
            var sb = new StringBuilder();

            if (e.Contains(id))
                sb.Append($"<button type='button' class='btn btn-sm btn-info mr-1' " +
                          $"onclick=\"verFoto({id},'entrada')\">Entrada</button>");
            if (s.Contains(id))
                sb.Append($"<button type='button' class='btn btn-sm btn-success' " +
                          $"onclick=\"verFoto({id},'salida')\">Salida</button>");

            return sb.ToString();
        }

        private HashSet<int> ParseViewStateIds(string key)
        {
            var raw = (string)(ViewState[key] ?? "");
            if (string.IsNullOrEmpty(raw)) return new HashSet<int>();
            return new HashSet<int>(raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                       .Select(int.Parse));
        }
    }
}