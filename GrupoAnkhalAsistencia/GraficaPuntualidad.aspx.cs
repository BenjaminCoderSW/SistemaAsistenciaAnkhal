using GrupoAnkhalAsistencia.Modelo;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class GraficaPuntualidad : System.Web.UI.Page
    {
        dbAsistenciaDataContext db = new dbAsistenciaDataContext(
            ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnBuscar_Click(object sender, EventArgs e)
        {
            DateTime inicio, fin;

            if (!DateTime.TryParse(txtFechaInicio.Text, out inicio) ||
                !DateTime.TryParse(txtFechaFin.Text, out fin))
                return;

            var data = ObtenerTop5Puntuales(inicio, fin);

            ltScript.Text = GenerarGraficaScript(data);
        }

        private List<(string empleado, int cantidad)> ObtenerTop5Puntuales(DateTime inicio, DateTime fin)
        {
            var consulta = db.tAsistencia
                .Where(a => a.Fecha >= inicio
                         && a.Fecha <= fin
                         && a.EstatusEntrada == "A tiempo")
                .Join(
                    db.tUsuario,
                    asistencia => asistencia.IdUsuario,
                    usuario => usuario.IdUsuario,
                    (asistencia, usuario) => new { asistencia, usuario }
                )
                .GroupBy(x => new { x.usuario.IdUsuario, Nombre = x.usuario.Nombre + " " + x.usuario.ApellidoPaterno })
                .Select(g => new
                {
                    Empleado = g.Key.Nombre,
                    Total = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToList();

            return consulta.Select(x => (x.Empleado, x.Total)).ToList();
        }


        private string GenerarGraficaScript(List<(string empleado, int cantidad)> datos)
        {
            string etiquetas = string.Join(",", datos.Select(d => $"'{d.empleado}'"));
            string valores = string.Join(",", datos.Select(d => d.cantidad));

            return $@"
<script>
if (window.graficaPuntualidadChart)
    window.graficaPuntualidadChart.destroy();

const ctx = document.getElementById('graficaPuntualidad');
window.graficaPuntualidadChart = new Chart(ctx, {{
    type: 'bar',
    data: {{
        labels: [{etiquetas}],
        datasets: [{{
            label: 'Entradas A Tiempo',
            data: [{valores}],
            backgroundColor: 'rgba(54, 162, 235, 0.6)',
            borderColor: 'rgba(54, 162, 235, 1)',
            borderWidth: 2
        }}]
    }},
    options: {{
        responsive: true,
        scales: {{
            y: {{
                beginAtZero: true
            }}
        }},
        plugins: {{
            legend: {{ display: true }}
        }}
    }}
}});
</script>";
        }
    }
}
   