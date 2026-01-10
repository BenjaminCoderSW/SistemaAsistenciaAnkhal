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
            Paragraph titulo = new Paragraph("HISTORIAL DEL EMPLEADO\n\n",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18));
            titulo.Alignment = Element.ALIGN_CENTER;
            pdfDoc.Add(titulo);

            // TABLA PDF
            PdfPTable tabla = new PdfPTable(dvgHistorialEmpleado.Columns.Count);
            tabla.WidthPercentage = 100;

            BaseColor headerColor = new BaseColor(0, 51, 102);
            BaseColor headerTextColor = BaseColor.WHITE;

            // ENCABEZADOS
            foreach (DataControlField col in dvgHistorialEmpleado.Columns)
            {
                PdfPCell celda = new PdfPCell(new Phrase(col.HeaderText,
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 6, headerTextColor)));

                celda.BackgroundColor = headerColor;
                celda.HorizontalAlignment = Element.ALIGN_CENTER;
                celda.Padding = 4;

                tabla.AddCell(celda);
            }

            // FILAS
            foreach (GridViewRow row in dvgHistorialEmpleado.Rows)
            {
                for (int j = 0; j < dvgHistorialEmpleado.Columns.Count; j++)
                {
                    string texto = "";

                    if (dvgHistorialEmpleado.Columns[j] is TemplateField)
                    {
                        if (row.Cells[j].Controls.Count > 0)
                        {
                            var literal = row.Cells[j].Controls[0] as LiteralControl;
                            if (literal != null)
                                texto = literal.Text.Trim();
                        }
                    }
                    else
                    {
                        texto = row.Cells[j].Text.Replace("&nbsp;", "").Trim();
                    }

                    PdfPCell celda = new PdfPCell(new Phrase(
                        texto,
                        FontFactory.GetFont(FontFactory.HELVETICA, 7, BaseColor.BLACK)
                    ));

                    celda.HorizontalAlignment = Element.ALIGN_CENTER;
                    celda.VerticalAlignment = Element.ALIGN_MIDDLE;
                    celda.Padding = 3;

                    tabla.AddCell(celda);
                }
            }

            pdfDoc.Add(tabla);
            pdfDoc.Close();
            Response.End();
        }


        // ======================================================
        //        EXPORTAR A EXCEL
        // ======================================================
        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=Asistencia.xls");
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-excel";

            StringWriter sw = new StringWriter();
            HtmlTextWriter hw = new HtmlTextWriter(sw);

            dvgHistorialEmpleado.GridLines = GridLines.Both;
            dvgHistorialEmpleado.HeaderStyle.Font.Bold = true;

            dvgHistorialEmpleado.RenderControl(hw);

            Response.Output.Write(sw.ToString());
            Response.Flush();
            Response.End();
        }

        public override void VerifyRenderingInServerForm(Control control)
        {
            // requerido para exportación
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
