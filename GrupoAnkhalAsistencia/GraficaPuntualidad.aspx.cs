using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web.UI;

namespace GrupoAnkhalAsistencia
{
    public partial class GraficaPuntualidad : System.Web.UI.Page
    {
        dbAsistenciaDataContext db = new dbAsistenciaDataContext(
            ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            // ✅ Validación de sesión
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

            if (!IsPostBack)
            {
                // ✅ Establecer fechas por defecto (últimos 30 días)
                txtFechaFin.Text = DateTime.Today.ToString("yyyy-MM-dd");
                txtFechaInicio.Text = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");
            }
        }

        protected void btnBuscar_Click(object sender, EventArgs e)
        {
            lblMensaje.Text = "";
            ltScript.Text = "";

            // ✅ Validación de fechas
            if (string.IsNullOrWhiteSpace(txtFechaInicio.Text) || string.IsNullOrWhiteSpace(txtFechaFin.Text))
            {
                lblMensaje.Text = "Por favor seleccione ambas fechas.";
                return;
            }

            DateTime inicio, fin;

            if (!DateTime.TryParseExact(txtFechaInicio.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out inicio) ||
                !DateTime.TryParseExact(txtFechaFin.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fin))
            {
                lblMensaje.Text = "Formato de fecha inválido.";
                return;
            }

            // ✅ Validar que la fecha inicio sea menor o igual a la fecha fin
            if (inicio > fin)
            {
                lblMensaje.Text = "La fecha de inicio no puede ser mayor a la fecha fin.";
                return;
            }

            // ✅ Validar que no sean fechas futuras
            if (inicio > DateTime.Today || fin > DateTime.Today)
            {
                lblMensaje.Text = "No se pueden seleccionar fechas futuras.";
                return;
            }

            try
            {
                var data = ObtenerTop5Puntuales(inicio, fin);

                if (data == null || data.Count == 0)
                {
                    lblMensaje.Text = "No se encontraron registros de empleados puntuales en el período seleccionado.";
                    ltScript.Text = GenerarGraficaVacia();
                    return;
                }

                ltScript.Text = GenerarGraficaScript(data);
            }
            catch (Exception ex)
            {
                lblMensaje.Text = "Error al generar la gráfica: " + ex.Message;
            }
        }

        private List<(string empleado, int cantidad)> ObtenerTop5Puntuales(DateTime inicio, DateTime fin)
        {
            try
            {
                // ✅ CORRECCIÓN: Usar "A tiempo" en mayúsculas y minúsculas según la BD
                var consulta = db.tAsistencia
                    .Where(a => a.Fecha >= inicio
                             && a.Fecha <= fin
                             && (a.EstatusEntrada == "A tiempo" || a.EstatusEntrada == "A TIEMPO"))
                    .Join(
                        db.tUsuario.Where(u => u.Estatus == 1),
                        asistencia => asistencia.IdUsuario,
                        usuario => usuario.IdUsuario,
                        (asistencia, usuario) => new { asistencia, usuario }
                    )
                    .GroupBy(x => new
                    {
                        x.usuario.IdUsuario,
                        Nombre = x.usuario.Nombre + " " + x.usuario.ApellidoPaterno + " " + x.usuario.ApellidoMaterno
                    })
                    .Select(g => new
                    {
                        Empleado = g.Key.Nombre,
                        Total = g.Count()
                    })
                    .OrderByDescending(x => x.Total)
                    .Take(5)
                    .ToList();

                return consulta.Select(x => (x.Empleado.Trim(), x.Total)).ToList();
            }
            catch (Exception ex)
            {
                // Log del error para debugging
                System.Diagnostics.Debug.WriteLine("Error en ObtenerTop5Puntuales: " + ex.Message);
                throw;
            }
        }

        private string GenerarGraficaScript(List<(string empleado, int cantidad)> datos)
        {
            if (datos == null || datos.Count == 0)
                return GenerarGraficaVacia();

            // ✅ Escapar comillas simples en nombres
            string etiquetas = string.Join(",", datos.Select(d => $"'{d.empleado.Replace("'", "\\'")}'"));
            string valores = string.Join(",", datos.Select(d => d.cantidad));

            // ✅ Colores degradados para las barras
            var colores = new[]
            {
                "rgba(54, 162, 235, 0.8)",   // Azul
                "rgba(75, 192, 192, 0.8)",   // Verde agua
                "rgba(255, 206, 86, 0.8)",   // Amarillo
                "rgba(153, 102, 255, 0.8)",  // Púrpura
                "rgba(255, 159, 64, 0.8)"    // Naranja
            };

            var coloresBorde = new[]
            {
                "rgba(54, 162, 235, 1)",
                "rgba(75, 192, 192, 1)",
                "rgba(255, 206, 86, 1)",
                "rgba(153, 102, 255, 1)",
                "rgba(255, 159, 64, 1)"
            };

            string backgroundColors = string.Join(",", colores.Take(datos.Count).Select(c => $"'{c}'"));
            string borderColors = string.Join(",", coloresBorde.Take(datos.Count).Select(c => $"'{c}'"));

            return $@"
<script>
(function() {{
    try {{
        // Destruir gráfica anterior si existe
        if (window.graficaPuntualidadChart) {{
            window.graficaPuntualidadChart.destroy();
        }}

        const ctx = document.getElementById('graficaPuntualidad');
        
        if (!ctx) {{
            console.error('No se encontró el elemento canvas');
            return;
        }}

        window.graficaPuntualidadChart = new Chart(ctx, {{
            type: 'bar',
            data: {{
                labels: [{etiquetas}],
                datasets: [{{
                    label: 'Entradas a Tiempo',
                    data: [{valores}],
                    backgroundColor: [{backgroundColors}],
                    borderColor: [{borderColors}],
                    borderWidth: 2,
                    borderRadius: 8,
                    barThickness: 50
                }}]
            }},
            options: {{
                responsive: true,
                maintainAspectRatio: true,
                plugins: {{
                    legend: {{ 
                        display: true,
                        position: 'top',
                        labels: {{
                            padding: 20,
                            font: {{
                                size: 14,
                                weight: 'bold'
                            }}
                        }}
                    }},
                    tooltip: {{
                        callbacks: {{
                            label: function(context) {{
                                return context.dataset.label + ': ' + context.parsed.y + ' veces';
                            }}
                        }},
                        backgroundColor: 'rgba(0,0,0,0.8)',
                        padding: 12,
                        titleFont: {{ size: 14, weight: 'bold' }},
                        bodyFont: {{ size: 13 }}
                    }}
                }},
                scales: {{
                    y: {{
                        beginAtZero: true,
                        ticks: {{
                            stepSize: 1,
                            font: {{ size: 12 }}
                        }},
                        grid: {{
                            color: 'rgba(0, 0, 0, 0.05)'
                        }}
                    }},
                    x: {{
                        ticks: {{
                            font: {{ size: 11 }},
                            maxRotation: 45,
                            minRotation: 0
                        }},
                        grid: {{
                            display: false
                        }}
                    }}
                }}
            }}
        }});
    }} catch (error) {{
        console.error('Error al crear la gráfica:', error);
    }}
}})();
</script>";
        }

        private string GenerarGraficaVacia()
        {
            return @"
<script>
(function() {
    try {
        if (window.graficaPuntualidadChart) {
            window.graficaPuntualidadChart.destroy();
        }

        const ctx = document.getElementById('graficaPuntualidad');
        
        if (!ctx) {
            console.error('No se encontró el elemento canvas');
            return;
        }

        window.graficaPuntualidadChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: ['Sin datos'],
                datasets: [{
                    label: 'Entradas a Tiempo',
                    data: [0],
                    backgroundColor: 'rgba(200, 200, 200, 0.5)',
                    borderColor: 'rgba(200, 200, 200, 1)',
                    borderWidth: 2
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                plugins: {
                    legend: { display: false }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        max: 10
                    }
                }
            }
        });
    } catch (error) {
        console.error('Error al crear la gráfica vacía:', error);
    }
})();
</script>";
        }
    }
}