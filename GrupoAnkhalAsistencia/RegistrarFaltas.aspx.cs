using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class RegistrarFaltas : System.Web.UI.Page
    {
        private dbAsistenciaDataContext db = new dbAsistenciaDataContext(
            ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (SesionState.usuario == null)
            {
                Response.Redirect("login.aspx");
                return;
            }

            string rol = SesionState.usuario.tRol.Rol;
            if (rol != "Administrador" && rol != "Rh")
            {
                Response.Redirect("login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                txtFechaProcesar.Text = DateTime.Today.ToString("yyyy-MM-dd");
                txtFiltroInicio.Text = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).ToString("yyyy-MM-dd");
                txtFiltroFin.Text = DateTime.Today.ToString("yyyy-MM-dd");
                ActualizarResumen();
                CargarFaltas();
            }
        }

        // -------------------------------------------------------
        // BOTON PRINCIPAL: ejecutar el stored procedure
        // -------------------------------------------------------
        protected void btnRegistrarFaltas_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime fecha = DateTime.Today;
                if (!string.IsNullOrWhiteSpace(txtFechaProcesar.Text))
                    DateTime.TryParse(txtFechaProcesar.Text, out fecha);

                string connStr = ConfigurationManager
                    .ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString;

                int faltasInsertadas = 0;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_RegistrarFaltasDelDia", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Fecha", fecha.Date);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                                faltasInsertadas = reader.GetInt32(0);
                        }
                    }
                }

                // Mostrar resultado
                pnlResultado.Visible = true;
                if (faltasInsertadas == 0)
                {
                    lblResultado.Text = $"✔ No se encontraron empleados sin registro para el {fecha:dd/MM/yyyy}. " +
                                        "Todos ya tenian asistencia registrada.";
                    lblResultado.ForeColor = System.Drawing.Color.DarkGreen;
                }
                else
                {
                    lblResultado.Text = $"✔ Se registraron {faltasInsertadas} falta(s) para el {fecha:dd/MM/yyyy}.";
                    lblResultado.ForeColor = System.Drawing.Color.DarkRed;
                }

                ActualizarResumen();
                CargarFaltas();

                MostrarSwal(faltasInsertadas == 0 ? "success" : "warning",
                    "Proceso completado",
                    faltasInsertadas == 0
                        ? "No habia empleados sin registro ese dia."
                        : $"Se registraron {faltasInsertadas} falta(s) para el {fecha:dd/MM/yyyy}.");
            }
            catch (Exception ex)
            {
                MostrarSwal("error", "Error", ex.Message);
            }
        }

        // -------------------------------------------------------
        // FILTRAR historial de faltas
        // -------------------------------------------------------
        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            CargarFaltas();
        }

        protected void dvgFaltas_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dvgFaltas.PageIndex = e.NewPageIndex;
            CargarFaltas();
        }

        // -------------------------------------------------------
        // CARGAR historial en el GridView
        // -------------------------------------------------------
        private void CargarFaltas()
        {
            DateTime inicio = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime fin = DateTime.Today;
            string empleado = txtFiltroEmpleado.Text.Trim();

            if (!string.IsNullOrWhiteSpace(txtFiltroInicio.Text))
                DateTime.TryParse(txtFiltroInicio.Text, out inicio);
            if (!string.IsNullOrWhiteSpace(txtFiltroFin.Text))
                DateTime.TryParse(txtFiltroFin.Text, out fin);

            var query = from a in db.tAsistencia
                        join u in db.tUsuario on a.IdUsuario equals u.IdUsuario
                        join ah in db.tAsignarHorario on a.IdAsignarHorario equals ah.IdAsignarHorario into ahJoin
                        from ah in ahJoin.DefaultIfEmpty()
                        join h in db.tHorario on ah.IdHorario equals h.IdHorario into hJoin
                        from h in hJoin.DefaultIfEmpty()
                        join p in db.tPlanta on u.IdPlanta equals p.IdPlanta into pJoin
                        from p in pJoin.DefaultIfEmpty()
                        where a.EstatusEntrada == "Falta"
                           && a.Fecha >= inicio.Date
                           && a.Fecha <= fin.Date
                        select new
                        {
                            Empleado = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,
                            NumeroEmpleado = u.NumeroEmpleado,
                            Planta = p.Planta,
                            a.Fecha,
                            HorarioInicio = h.HoraInicio,
                            HorarioFin = h.HoraFin
                        };

            if (!string.IsNullOrWhiteSpace(empleado))
                query = query.Where(x =>
                    (x.Empleado).Contains(empleado));

            query = query.OrderByDescending(x => x.Fecha);

            dvgFaltas.DataSource = query.ToList();
            dvgFaltas.DataBind();
        }

        // -------------------------------------------------------
        // RESUMEN: tarjetas superiores
        // -------------------------------------------------------
        private void ActualizarResumen()
        {
            DateTime hoy = DateTime.Today;
            DateTime mesI = new DateTime(hoy.Year, hoy.Month, 1);

            int faltasHoy = db.tAsistencia
                .Count(a => a.EstatusEntrada == "Falta" && a.Fecha == hoy);

            int faltasMes = db.tAsistencia
                .Count(a => a.EstatusEntrada == "Falta"
                         && a.Fecha >= mesI && a.Fecha <= hoy);

            // Ultima fecha que tiene al menos una falta
            var ultima = db.tAsistencia
                .Where(a => a.EstatusEntrada == "Falta")
                .OrderByDescending(a => a.Fecha)
                .Select(a => a.Fecha)
                .FirstOrDefault();

            lblTotalFaltasHoy.Text = faltasHoy.ToString();
            lblTotalFaltasMes.Text = faltasMes.ToString();
            lblUltimaEjecucion.Text = ultima != null
                ? ((DateTime)ultima).ToString("dd/MM/yyyy")
                : "Ninguna";
        }

        private void MostrarSwal(string tipo, string titulo, string mensaje)
        {
            titulo = (titulo ?? "").Replace("'", "\\'").Replace("\n", " ");
            mensaje = (mensaje ?? "").Replace("'", "\\'").Replace("\n", " ");
            string script = $@"
                Swal.fire({{
                    icon: '{tipo}',
                    title: '{titulo}',
                    text: '{mensaje}',
                    timer: 3000,
                    showConfirmButton: false
                }});";
            ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), script, true);
        }
    }
}