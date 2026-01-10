using GrupoAnkhalAsistencia.Modelo;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class GraficaEmpleado : System.Web.UI.Page
    {
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
            ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                CargarEmpleados();
        }

        private void CargarEmpleados()
        {
            var empleados = db.tUsuario
                .Select(u => new
                {
                    u.IdUsuario,
                    NombreCompleto = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno
                })
                .OrderBy(u => u.NombreCompleto)
                .ToList();

            ddlEmpleados.DataSource = empleados;
            ddlEmpleados.DataTextField = "NombreCompleto";
            ddlEmpleados.DataValueField = "IdUsuario";
            ddlEmpleados.DataBind();

            ddlEmpleados.Items.Insert(0, new ListItem("-- Seleccione un empleado --", "0"));
        }

        protected void btnBuscar_Click(object sender, EventArgs e)
        {
            lblError.Text = "";

            if (ddlEmpleados.SelectedValue == "0")
                return;

            int idEmpleado = int.Parse(ddlEmpleados.SelectedValue);
            hfEmpleadoId.Value = idEmpleado.ToString();

            if (!DateTime.TryParseExact(txtFechaInicio.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaInicio) ||
                !DateTime.TryParseExact(txtFechaFin.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaFin))
            {
                lblError.Text = "Selecciona correctamente las fechas.";
                return;
            }

            var estadisticasAsistencia = ObtenerEstadisticasEmpleado(idEmpleado, fechaInicio, fechaFin);
            var estadisticasComida = ObtenerEstadisticasComida(idEmpleado, fechaInicio, fechaFin);
            var estadisticasPermisos = ObtenerEstadisticasPermisosComisiones(idEmpleado, fechaInicio, fechaFin);


            // Limpiar Literal antes de concatenar
            ltScript.Text = "";
            ltScript.Text += GenerarGraficaAsistenciaScript(estadisticasAsistencia);
            ltScript.Text += GenerarGraficaComidaScript(estadisticasComida);
            ltScript.Text += GenerarGraficaPermisosComisionesScript(estadisticasPermisos);

        }

        private (Dictionary<string, int> entrada, Dictionary<string, int> salida) ObtenerEstadisticasEmpleado(int idEmpleado, DateTime inicio, DateTime fin)
        {
            var registros = db.tAsistencia
                .Where(a => a.IdUsuario == idEmpleado && a.Fecha >= inicio && a.Fecha <= fin)
                .ToList();

            Dictionary<string, int> entrada = new Dictionary<string, int>()
            {
                {"A tiempo",0},
                {"Retardo",0},
                {"Horario no cumplido",0}
            };

            Dictionary<string, int> salida = new Dictionary<string, int>()
            {
                {"Horario cumplido",0},
                {"Horario no cumplido",0}
            };

            foreach (var r in registros)
            {
                if (!string.IsNullOrEmpty(r.EstatusEntrada) && entrada.ContainsKey(r.EstatusEntrada))
                    entrada[r.EstatusEntrada]++;
                if (!string.IsNullOrEmpty(r.EstatusSalida) && salida.ContainsKey(r.EstatusSalida))
                    salida[r.EstatusSalida]++;
            }

            return (entrada, salida);
        }

        private Dictionary<string, int> ObtenerEstadisticasComida(int idEmpleado, DateTime inicio, DateTime fin)
        {
            var registros = db.tAsistencia
                .Where(a => a.IdUsuario == idEmpleado && a.Fecha >= inicio && a.Fecha <= fin)
                .ToList();

            Dictionary<string, int> comida = new Dictionary<string, int>()
            {
                {"Comida a tiempo",0},
                {"Retardo Comida",0}
            };

            foreach (var r in registros)
            {
                if (!string.IsNullOrEmpty(r.EstatusComida) && comida.ContainsKey(r.EstatusComida))
                    comida[r.EstatusComida]++;
            }

            return comida;
        }


        private Dictionary<string, int> ObtenerEstadisticasPermisosComisiones(int idEmpleado, DateTime inicio, DateTime fin)
        {
            var registros = db.V_PermisosComisiones
                .Where(p => p.IdUsuario == idEmpleado &&
                            p.Fecha >= inicio &&
                            p.Fecha <= fin)
                .ToList();

            Dictionary<string, int> resultado = new Dictionary<string, int>()
    {
        {"Permiso por días",0},
        {"Permiso por horas",0},
        {"Comisión por días",0},
        {"Comisión por horas",0}
    };

            foreach (var r in registros)
            {
                if (resultado.ContainsKey(r.Tipo))
                    resultado[r.Tipo] += r.Total ?? 0;

            }

            return resultado;
        }


        private string GenerarGraficaAsistenciaScript((Dictionary<string, int> entrada, Dictionary<string, int> salida) estadisticas)
        {
            int total = estadisticas.entrada.Values.Sum() + estadisticas.salida.Values.Sum();
            if (total == 0) total = 1;

            int aTiempo = estadisticas.entrada["A tiempo"] * 100 / total;
            int retardo = estadisticas.entrada["Retardo"] * 100 / total;
            int cumplidoSalida = estadisticas.salida["Horario cumplido"] * 100 / total;
            int noCumplidoSalida = estadisticas.salida["Horario no cumplido"] * 100 / total;

            return $@"
<script>
if(window.graficaAsistenciaChart) window.graficaAsistenciaChart.destroy();
const ctxAsistencia = document.getElementById('graficaAsistencia');
window.graficaAsistenciaChart = new Chart(ctxAsistencia, {{
    type: 'pie',
    data: {{
        labels: ['Entrada - A tiempo','Entrada - Retardo','Salida - Horario cumplido','Salida - Horario no cumplido'],
        datasets: [{{
            data: [{aTiempo},{retardo},{cumplidoSalida},{noCumplidoSalida}],
            backgroundColor: ['#4ade80','#facc15','#f87171','#60a5fa'],
            borderWidth: 2,
            borderColor: '#ffffff',
            hoverOffset: 12
        }}]
    }},
    options: {{
        responsive: true,
        plugins: {{
            legend: {{ position: 'bottom', labels: {{ padding: 20 }} }},
            tooltip: {{
                callbacks: {{
                    label: (context) => context.label + ': ' + context.raw + '%'
                }}
            }}
        }}
    }}
}});
</script>";
        }

        private string GenerarGraficaComidaScript(Dictionary<string, int> estadisticasComida)
        {
            int total = estadisticasComida.Values.Sum();
            if (total == 0) total = 1;

            int aTiempo = estadisticasComida["Comida a tiempo"] * 100 / total;
            int retardo = estadisticasComida["Retardo Comida"] * 100 / total;

            return $@"
<script>
if(window.graficaComidaChart) window.graficaComidaChart.destroy();
const ctxComida = document.getElementById('graficaComida');
window.graficaComidaChart = new Chart(ctxComida, {{
    type: 'pie',
    data: {{
        labels: ['Comida a tiempo','Retardo Comida'],
        datasets: [{{
            data: [{aTiempo},{retardo}],
            backgroundColor: ['#34d399','#fbbf24'],
            borderWidth: 2,
            borderColor: '#ffffff',
            hoverOffset: 12
        }}]
    }},
    options: {{
        responsive: true,
        plugins: {{
            legend: {{ position: 'bottom', labels: {{ padding: 20 }} }},
            tooltip: {{
                callbacks: {{
                    label: (context) => context.label + ': ' + context.raw + '%'
                }}
            }}
        }}
    }}
}});
</script>";
        }

        private string GenerarGraficaPermisosComisionesScript(Dictionary<string, int> datos)
        {
            int total = datos.Values.Sum();
            if (total == 0) total = 1;

            int permisoDia = datos["Permiso por días"] * 100 / total;
            int permisoHora = datos["Permiso por horas"] * 100 / total;
            int comDia = datos["Comisión por días"] * 100 / total;
            int comHora = datos["Comisión por horas"] * 100 / total;

            return $@"
<script>
if(window.graficaPermisosChart) window.graficaPermisosChart.destroy();
const ctxPermisos = document.getElementById('graficaPermisos');
window.graficaPermisosChart = new Chart(ctxPermisos, {{
    type: 'pie',
    data: {{
        labels: ['Permiso por días','Permiso por horas','Comisión por días','Comisión por horas'],
        datasets: [{{
            data: [{permisoDia},{permisoHora},{comDia},{comHora}],
            backgroundColor: ['#60a5fa','#a78bfa','#f87171','#34d399'],
            borderWidth: 2,
            borderColor: '#ffffff',
            hoverOffset: 12
        }}]
    }},
    options: {{
        responsive: true,
        plugins: {{
            legend: {{ position: 'bottom', labels: {{ padding: 20 }} }},
            tooltip: {{
                callbacks: {{
                    label: (context) => context.label + ': ' + context.raw + '%'
                }}
            }}
        }}
    }}
}});
</script>";
        }


    }
}
