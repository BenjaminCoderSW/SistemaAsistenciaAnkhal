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
            }
        }

        private void CargarDashboard()
        {
            DateTime hoy = DateTime.Today;

            // Total empleados registrados en el sistema
            int totalEmpleados = db.tUsuario.Where(u => u.Estatus == 1).Count();

            // Llegaron a tiempo
            int llegaronTiempo = db.tAsistencia
                .Where(a => a.Fecha == hoy && a.EstatusEntrada == "A TIEMPO")
                .Count();

            // Llegaron tarde
            int llegaronTarde = db.tAsistencia
                .Where(a => a.Fecha == hoy && a.EstatusEntrada == "RETARDO")
                .Count();

            // Faltaron = empleados que NO tienen asistencia hoy
            int faltaron =
                totalEmpleados -
                db.tAsistencia.Where(a => a.Fecha == hoy).Select(a => a.IdUsuario).Distinct().Count();

            // Asignar a los labels
            lblTotalEmpleados.Text = totalEmpleados.ToString();
            lblLlegaronTiempo.Text = llegaronTiempo.ToString();
            lblLlegaronTarde.Text = llegaronTarde.ToString();
            lblFaltaron.Text = faltaron.ToString();
        }

        private void CargarAsistenciaHoy(string filtro = "")
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

            // Aplicar filtro si existe
            if (!string.IsNullOrWhiteSpace(filtro))
            {
                query = query.Where(x => x.Empleado.Contains(filtro));
            }

            gvAsistenciaHoy.DataSource = query.ToList();
            gvAsistenciaHoy.DataBind();
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
                {
                    return "";
                }

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
            CargarAsistenciaHoy(txtBuscar.Text.Trim());
        }

        protected void gvAsistenciaHoy_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvAsistenciaHoy.PageIndex = e.NewPageIndex;
            CargarAsistenciaHoy(txtBuscar.Text.Trim());
        }
    }
}