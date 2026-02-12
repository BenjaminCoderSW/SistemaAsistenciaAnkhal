using GrupoAnkhalAsistencia.Modelo;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class ReporteAsistencia : System.Web.UI.Page
    {
        public dbAsistenciaDataContext db =
            new dbAsistenciaDataContext(ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BuscarEmpleado();

                string hoy = DateTime.Now.ToString("yyyy-MM-dd");
                CargarHistorialPorFecha(hoy);


             

            }
        }

        // ======================================================
        //    MÉTODO PRINCIPAL DE FILTRO
        // ======================================================

        private void CargarHistorialPorFecha(string fecha)
        {


            DateTime hoy = DateTime.Now.Date;

            var asistencia = from m in db.V_REPORTE_ASISTENCIA
                             where m.Fecha == hoy
                             orderby m.Fecha descending
                             select m;

            dvgHistorialEmpleado.DataSource = asistencia.ToList();
            dvgHistorialEmpleado.DataBind();


        }

        private void BuscarEmpleado()
        {
            // VALIDAR CAMPOS OBLIGATORIOS
            if (string.IsNullOrWhiteSpace(txtFechaInicio.Text) ||
                string.IsNullOrWhiteSpace(txtFechaFin.Text))
            {
                // Mensaje al usuario
                MostrarSwal("warning", "Alerta", "debe seleccionar fecha inicio y fecha fin.");
                return; // detener ejecución
            }

            // PARSEAR FECHAS
            DateTime? fechaInicio = null;
            DateTime? fechaFin = null;

            if (DateTime.TryParse(txtFechaInicio.Text, out DateTime fi))
                fechaInicio = fi.Date;

            if (DateTime.TryParse(txtFechaFin.Text, out DateTime ff))
                fechaFin = ff.Date;

            string empleadoFiltro = txtEmpleado.Text.Trim();

            var query = from m in db.V_REPORTE_ASISTENCIA
                        select m;

            // FILTROS
            query = query.Where(x => x.Fecha >= fechaInicio && x.Fecha <= fechaFin);

            if (!string.IsNullOrWhiteSpace(empleadoFiltro))
                query = query.Where(x => x.EMPLEADO.Contains(empleadoFiltro));

            query = query.OrderByDescending(x => x.Fecha);

            dvgHistorialEmpleado.DataSource = query.ToList();
            dvgHistorialEmpleado.DataBind();
        }

        // ======================================================
        //  EVENTOS DE PÁGINA Y BÚSQUEDA
        // ======================================================
        protected void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            BuscarEmpleado();
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            BuscarEmpleado();
        }

        private void MostrarSwal(string tipo, string titulo, string mensaje)
        {
            string script = $@"
        Swal.fire({{
            icon: '{tipo}',
            title: '{titulo}',
            text: '{mensaje}',
            timer: 2000,
            showConfirmButton: false
        }});

        function speakText(text) {{
            var synth = window.speechSynthesis;

            var interval = setInterval(function() {{
                var voices = synth.getVoices();
                if (voices.length !== 0) {{
                    clearInterval(interval);

                    // PRIORIDAD: voces de mujer de Google
                    var preferidas = [
                        'Google español (Latinoamérica)',
                        'Google español',
                        'es-MX-Standard-A',
                        'es-US-Standard-A',
                        'es-ES-Standard-A'
                    ];

                    var selectedVoice = null;

                    for (var i = 0; i < preferidas.length; i++) {{
                        selectedVoice = voices.find(v => v.name.includes(preferidas[i]));
                        if (selectedVoice) break;
                    }}

                    var utter = new SpeechSynthesisUtterance(text);
                    utter.voice = selectedVoice;
                    utter.lang = 'es-MX';
                    utter.rate = 1; 
                    utter.pitch = 1.1; // Más femenina

                    synth.speak(utter);
                }}
            }}, 200);
        }}

        // SOLO VOZ DE MUJER
        speakText('{titulo}. {mensaje}');
    ";

            ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(), script, true);
        }


        protected void dvgHistorialEmpleado_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dvgHistorialEmpleado.PageIndex = e.NewPageIndex;
            BuscarEmpleado();
        }


        // ======================================================
        //       EXPORTAR A PDF
        // ======================================================
        protected void btnExportPDF_Click(object sender, EventArgs e)
        {
            // ✅ VALIDAR FECHAS
            if (string.IsNullOrWhiteSpace(txtFechaInicio.Text) ||
                string.IsNullOrWhiteSpace(txtFechaFin.Text))
            {
                MostrarSwal("warning", "Alerta", "Debe seleccionar fecha inicio y fecha fin.");
                return;
            }

            DateTime fechaInicio = DateTime.Parse(txtFechaInicio.Text).Date;
            DateTime fechaFin = DateTime.Parse(txtFechaFin.Text).Date;
            string empleadoFiltro = txtEmpleado.Text.Trim();

            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=Asistencia.pdf");
            Response.CacheControl = "no-cache";

            Document pdfDoc = new Document(PageSize.A4.Rotate(), 20f, 20f, 20f, 20f);
            PdfWriter writer = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
            pdfDoc.Open();

            // LOGO
            string rutaLogo = Server.MapPath("~/img/ankhal.png");
            if (File.Exists(rutaLogo))
            {
                iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(rutaLogo);
                logo.ScaleAbsolute(120, 60);
                logo.Alignment = Element.ALIGN_LEFT;
                pdfDoc.Add(logo);

                Paragraph textoEmpresa = new Paragraph("GRUPO ANKHAL",
                    new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 15, iTextSharp.text.Font.BOLD));
                textoEmpresa.Alignment = Element.ALIGN_LEFT;
                textoEmpresa.SpacingBefore = 5f;
                pdfDoc.Add(textoEmpresa);
            }

            // TÍTULO
            Paragraph titulo = new Paragraph("REPORTE DE ASISTENCIA\n\n",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18));
            titulo.Alignment = Element.ALIGN_CENTER;
            pdfDoc.Add(titulo);

            // INFORMACIÓN DEL FILTRO
            Paragraph filtroInfo = new Paragraph(
                $"Período: {fechaInicio.ToString("dd/MM/yyyy")} - {fechaFin.ToString("dd/MM/yyyy")}\n\n",
                FontFactory.GetFont(FontFactory.HELVETICA, 10));
            filtroInfo.Alignment = Element.ALIGN_CENTER;
            pdfDoc.Add(filtroInfo);

            // ✅ OBTENER DATOS CON FILTRO - CORREGIDO
            var query = from m in db.V_REPORTE_ASISTENCIA
                        where m.Fecha >= fechaInicio && m.Fecha <= fechaFin
                        select m;

            // Aplicar filtro de empleado si existe
            if (!string.IsNullOrWhiteSpace(empleadoFiltro))
            {
                query = query.Where(x => x.EMPLEADO.Contains(empleadoFiltro));
            }

            // ✅ APLICAR ORDENAMIENTO AL FINAL
            var datos = query.OrderByDescending(x => x.Fecha).ToList();

            // VERIFICAR SI HAY DATOS
            if (datos.Count == 0)
            {
                Paragraph sinDatos = new Paragraph("No se encontraron registros para el período seleccionado.",
                    FontFactory.GetFont(FontFactory.HELVETICA, 12));
                sinDatos.Alignment = Element.ALIGN_CENTER;
                pdfDoc.Add(sinDatos);
                pdfDoc.Close();
                Response.End();
                return;
            }

            // TABLA PDF - 27 columnas según tu GridView
            PdfPTable tabla = new PdfPTable(27);
            tabla.WidthPercentage = 100;

            BaseColor headerColor = new BaseColor(0, 51, 102);
            BaseColor headerTextColor = BaseColor.WHITE;

            // ENCABEZADOS
            string[] encabezados = new string[] {
        "EMPLEADO", "Planta", "Fecha", "HoraEntrada", "HoraSalida",
        "HoraSalidaComer", "HoraEntradaComer", "HorasTrabajadas", "Horas Trabajadas (Decimal)",
        "Tiempo Comida", "EstatusEntrada", "EstatusSalida", "EstatusComida",
        "TipoPermiso", "HoraSalidaPermiso", "HoraEntradaPermiso", "HorasPermiso",
        "horaSalidaComision", "horaEntradaComision", "horasComision",
        "Horas Extras", "Estatus Horas Extras",
        "Ubicación Entrada", "Ubicación Salida", "MacEntrada", "MacSalida", "IP"
    };

            foreach (string encabezado in encabezados)
            {
                PdfPCell celda = new PdfPCell(new Phrase(encabezado,
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 6, headerTextColor)));
                celda.BackgroundColor = headerColor;
                celda.HorizontalAlignment = Element.ALIGN_CENTER;
                celda.Padding = 4;
                tabla.AddCell(celda);
            }

            // AGREGAR DATOS
            foreach (var registro in datos)
            {
                // Empleado
                tabla.AddCell(CrearCelda(registro.EMPLEADO ?? ""));
                // Planta
                tabla.AddCell(CrearCelda(registro.Planta ?? ""));
                // Fecha
                tabla.AddCell(CrearCelda(registro.Fecha.HasValue ? registro.Fecha.Value.ToString("dd/MM/yyyy") : ""));
                // HoraEntrada
                tabla.AddCell(CrearCelda(registro.HoraEntrada.HasValue ? registro.HoraEntrada.Value.ToString(@"hh\:mm") : ""));
                // HoraSalida
                tabla.AddCell(CrearCelda(registro.HoraSalida.HasValue ? registro.HoraSalida.Value.ToString(@"hh\:mm") : ""));
                // HoraSalidaComer
                tabla.AddCell(CrearCelda(registro.HoraSalidaComer.HasValue ? registro.HoraSalidaComer.Value.ToString(@"hh\:mm") : ""));
                // HoraEntradaComer
                tabla.AddCell(CrearCelda(registro.HoraEntradaComer.HasValue ? registro.HoraEntradaComer.Value.ToString(@"hh\:mm") : ""));
                // HorasTrabajadas
                tabla.AddCell(CrearCelda(registro.HorasTrabajadas.HasValue ? registro.HorasTrabajadas.Value.ToString(@"hh\:mm") : ""));
                // HorasTrabajadasDecimal
                tabla.AddCell(CrearCelda(registro.HorasTrabajadasDecimal.HasValue ? registro.HorasTrabajadasDecimal.Value.ToString("N2") : ""));
                // TiempoComida
                tabla.AddCell(CrearCelda(registro.tiempoComida.HasValue ? registro.tiempoComida.Value.ToString("N2") : ""));
                // EstatusEntrada
                tabla.AddCell(CrearCelda(registro.EstatusEntrada ?? ""));
                // EstatusSalida
                tabla.AddCell(CrearCelda(registro.EstatusSalida ?? ""));
                // EstatusComida
                tabla.AddCell(CrearCelda(registro.EstatusComida ?? ""));
                // TipoPermiso
                tabla.AddCell(CrearCelda(registro.TipoPermiso ?? ""));
                // HoraSalidaPermiso
                tabla.AddCell(CrearCelda(registro.HoraSalidaPermiso.HasValue ? registro.HoraSalidaPermiso.Value.ToString(@"hh\:mm") : ""));
                // HoraEntradaPermiso
                tabla.AddCell(CrearCelda(registro.HoraEntradaPermiso.HasValue ? registro.HoraEntradaPermiso.Value.ToString(@"hh\:mm") : ""));
                // HorasPermiso
                tabla.AddCell(CrearCelda(registro.HorasPermiso.HasValue ? registro.HorasPermiso.Value.ToString("N2") : ""));
                // HoraSalidaComision
                tabla.AddCell(CrearCelda(registro.HoraSalidaComision.HasValue ? registro.HoraSalidaComision.Value.ToString(@"hh\:mm") : ""));
                // HoraEntradaComision
                tabla.AddCell(CrearCelda(registro.HoraEntradaComision.HasValue ? registro.HoraEntradaComision.Value.ToString(@"hh\:mm") : ""));
                // HorasComision
                tabla.AddCell(CrearCelda(registro.horasComision.HasValue ? registro.horasComision.Value.ToString(@"hh\:mm") : ""));
                // HorasExtras
                tabla.AddCell(CrearCelda(registro.HorasExtras.HasValue ? registro.HorasExtras.Value.ToString("N2") : ""));
                // EstatusHorasExtras
                tabla.AddCell(CrearCelda(registro.EstatusHorasExtras ?? ""));
                // UbicacionEntrada
                tabla.AddCell(CrearCelda(registro.UbicacionEntrada ?? ""));
                // UbicacionSalida
                tabla.AddCell(CrearCelda(registro.UbicacionSalida ?? ""));
                // MacEntrada
                tabla.AddCell(CrearCelda(registro.MacEntrada ?? ""));
                // MacSalida
                tabla.AddCell(CrearCelda(registro.MacSalida ?? ""));
                // IP
                tabla.AddCell(CrearCelda(registro.IP ?? ""));
            }

            pdfDoc.Add(tabla);
            pdfDoc.Close();
            Response.End();
        }

        // MÉTODO AUXILIAR PARA CREAR CELDAS
        private PdfPCell CrearCelda(string texto)
        {
            PdfPCell celda = new PdfPCell(new Phrase(
                texto,
                FontFactory.GetFont(FontFactory.HELVETICA, 7, BaseColor.BLACK)
            ));

            celda.HorizontalAlignment = Element.ALIGN_CENTER;
            celda.VerticalAlignment = Element.ALIGN_MIDDLE;
            celda.Padding = 3;

            return celda;
        }


        // ======================================================
        //        EXPORTAR A EXCEL
        // ======================================================

        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFechaInicio.Text) ||
                string.IsNullOrWhiteSpace(txtFechaFin.Text))
            {
                MostrarSwal("warning", "Alerta", "Debe seleccionar fecha inicio y fecha fin.");
                return;
            }

            DateTime fechaInicio = DateTime.Parse(txtFechaInicio.Text).Date;
            DateTime fechaFin = DateTime.Parse(txtFechaFin.Text).Date;

            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=Asistencia.xls");
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-excel";

            StringWriter sw = new StringWriter();
            HtmlTextWriter hw = new HtmlTextWriter(sw);

            GridView gvExport = new GridView();
            gvExport.AutoGenerateColumns = true;
            gvExport.EnableViewState = false;
            gvExport.GridLines = GridLines.Both;
            gvExport.HeaderStyle.Font.Bold = true;

            var data = db.V_REPORTE_ASISTENCIA
                .Where(x => x.Fecha >= fechaInicio && x.Fecha <= fechaFin)
                .OrderByDescending(x => x.Fecha)
                .Select(x => new
                {
                    x.EMPLEADO,
                    x.Planta,
                    x.Fecha,
                    x.HoraEntrada,
                    x.HoraSalidaComer,
                    x.HoraEntradaComer,
                    x.HoraSalida,
                    x.EstatusHorasExtras
                })
                .ToList();

            gvExport.DataSource = data;
            gvExport.DataBind();
            gvExport.RenderControl(hw);

            Response.Write(sw.ToString());
            Response.End();
        }




        public override void VerifyRenderingInServerForm(Control control)
        {
            // Requerido para exportar GridView a Excel
        }
        

        // ======================================================
        //      CONVERSIÓN DE LAT/LNG A LINK DE GOOGLE MAPS
        // ======================================================
        public string GetMapaLink(string ubicacion)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ubicacion))
                    return "";

                var partes = ubicacion.Split(',');
                if (partes.Length != 2) return "";

                string lat = partes[0];
                string lng = partes[1];

                string url = $"https://www.google.com/maps?q={lat},{lng}'";

                return $"<a href='{url}' target='_blank'><img src='/img/mapa.png' width='25' /></a>";
            }
            catch
            {
                return "";
            }
        }
    }
}
