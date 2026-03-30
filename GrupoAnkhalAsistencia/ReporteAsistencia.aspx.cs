using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
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
            if (SesionState.usuario == null) { Response.Redirect("login.aspx"); return; }
            string[] rolesPermitidos = { "Administrador", "Rh", "Jefe de Planta" };
            if (!rolesPermitidos.Contains(SesionState.usuario.tRol.Rol))
            {
                Response.Redirect("login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                CargarPlantasFiltro();
                string hoy = DateTime.Now.ToString("yyyy-MM-dd");
                CargarHistorialPorFecha(hoy);
            }
        }

        // ======================================================
        //    CARGA DROPDOWN DE PLANTAS
        // ======================================================

        private void CargarPlantasFiltro()
        {
            var plantas = db.tPlanta
                .OrderBy(p => p.Planta)
                .Select(p => new { p.IdPlanta, p.Planta })
                .ToList();

            ddlPlanta.Items.Clear();
            ddlPlanta.Items.Add(new System.Web.UI.WebControls.ListItem("-- Todas --", "0"));
            foreach (var pl in plantas)
                ddlPlanta.Items.Add(new System.Web.UI.WebControls.ListItem(pl.Planta, pl.IdPlanta.ToString()));
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
            string plantaFiltro = ddlPlanta.SelectedValue != "0" ? ddlPlanta.SelectedItem.Text : null;

            var query = from m in db.V_REPORTE_ASISTENCIA
                        select m;

            // FILTROS
            query = query.Where(x => x.Fecha >= fechaInicio && x.Fecha <= fechaFin);

            if (!string.IsNullOrWhiteSpace(empleadoFiltro))
                query = query.Where(x => x.EMPLEADO.Contains(empleadoFiltro));

            if (!string.IsNullOrEmpty(plantaFiltro))
                query = query.Where(x => x.Planta == plantaFiltro);

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
            string plantaFiltroPdf = ddlPlanta.SelectedValue != "0" ? ddlPlanta.SelectedItem.Text : null;

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

            if (!string.IsNullOrWhiteSpace(empleadoFiltro))
                query = query.Where(x => x.EMPLEADO.Contains(empleadoFiltro));

            if (!string.IsNullOrEmpty(plantaFiltroPdf))
                query = query.Where(x => x.Planta == plantaFiltroPdf);

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

            // TABLA PDF - 26 columnas según tu GridView
            PdfPTable tabla = new PdfPTable(27);
            tabla.WidthPercentage = 100;

            BaseColor headerColor = new BaseColor(0, 51, 102);
            BaseColor headerTextColor = BaseColor.WHITE;

            // ENCABEZADOS
            string[] encabezados = new string[] {
                "EMPLEADO", "Planta", "Fecha", "HoraEntrada", "HoraSalida",
                "HoraSalidaComer", "HoraEntradaComer", "HorasTrabajadas", "Tiempo Comida",
                "EstatusEntrada", "EstatusSalida", "EstatusComida",
                "TipoPermiso", "HoraSalidaPermiso", "HoraEntradaPermiso", "HorasPermiso",
                "horaSalidaComision", "horaEntradaComision", "horasComision",
                "Horas Extras", "Estatus Horas Extras",
                "Ubicación Entrada", "Ubicación Salida", "MacEntrada", "MacSalida", "IP", "Estatus Checada"
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
                // TiempoComida
                tabla.AddCell(CrearCelda(registro.tiempoComida.HasValue ? TimeSpan.FromHours((double)registro.tiempoComida.Value).ToString(@"hh\:mm") : ""));
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
                tabla.AddCell(CrearCelda(registro.HorasExtras.HasValue ? TimeSpan.FromHours((double)registro.HorasExtras.Value).ToString(@"hh\:mm") : ""));
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
                // Estatus Checada
                string estatusChecada =
                    !registro.HoraEntrada.HasValue ? "No checó nada" :
                    !registro.HoraSalida.HasValue ? "No checó salida" :
                    !registro.HoraSalidaComer.HasValue ? "No checó salida a comer" :
                    !registro.HoraEntradaComer.HasValue ? "No checó regreso de comer" :
                    "Completo";
                tabla.AddCell(CrearCelda(estatusChecada));
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
            string empleadoFiltro = txtEmpleado.Text.Trim();
            string plantaFiltroXls = ddlPlanta.SelectedValue != "0" ? ddlPlanta.SelectedItem.Text : null;

            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=Asistencia.xls");
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-excel";

            var baseQuery = db.V_REPORTE_ASISTENCIA
                .Where(x => x.Fecha >= fechaInicio && x.Fecha <= fechaFin);

            if (!string.IsNullOrWhiteSpace(empleadoFiltro))
                baseQuery = baseQuery.Where(x => x.EMPLEADO.Contains(empleadoFiltro));

            if (!string.IsNullOrEmpty(plantaFiltroXls))
                baseQuery = baseQuery.Where(x => x.Planta == plantaFiltroXls);

            var data = baseQuery.OrderByDescending(x => x.Fecha).ToList();

            // Transformar datos para formato correcto
            var dataTransformada = data.Select(x => new
            {
                x.EMPLEADO,
                x.Planta,
                Fecha = x.Fecha.HasValue ? x.Fecha.Value.ToString("dd/MM/yyyy") : "",
                HoraEntrada = x.HoraEntrada.HasValue ? x.HoraEntrada.Value.ToString(@"hh\:mm") : "",
                HoraSalida = x.HoraSalida.HasValue ? x.HoraSalida.Value.ToString(@"hh\:mm") : "",
                HoraSalidaComer = x.HoraSalidaComer.HasValue ? x.HoraSalidaComer.Value.ToString(@"hh\:mm") : "",
                HoraEntradaComer = x.HoraEntradaComer.HasValue ? x.HoraEntradaComer.Value.ToString(@"hh\:mm") : "",
                HorasTrabajadas = x.HorasTrabajadas.HasValue ? x.HorasTrabajadas.Value.ToString(@"hh\:mm") : "",
                // Convertir decimal de horas a HH:MM
                tiempoComida = x.tiempoComida.HasValue
                    ? TimeSpan.FromHours((double)x.tiempoComida.Value).ToString(@"hh\:mm")
                    : "",
                x.EstatusEntrada,
                x.EstatusSalida,
                x.EstatusComida,
                x.TipoPermiso,
                HoraSalidaPermiso = x.HoraSalidaPermiso.HasValue ? x.HoraSalidaPermiso.Value.ToString(@"hh\:mm") : "",
                HoraEntradaPermiso = x.HoraEntradaPermiso.HasValue ? x.HoraEntradaPermiso.Value.ToString(@"hh\:mm") : "",
                HorasPermiso = x.HorasPermiso.HasValue ? x.HorasPermiso.Value.ToString("N2") : "",
                horaSalidaComision = x.HoraSalidaComision.HasValue ? x.HoraSalidaComision.Value.ToString(@"hh\:mm") : "",
                horaEntradaComision = x.HoraEntradaComision.HasValue ? x.HoraEntradaComision.Value.ToString(@"hh\:mm") : "",
                horasComision = x.horasComision.HasValue ? x.horasComision.Value.ToString(@"hh\:mm") : "",
                // Convertir decimal de horas extras a HH:MM
                HorasExtras = x.HorasExtras.HasValue
                    ? TimeSpan.FromHours((double)x.HorasExtras.Value).ToString(@"hh\:mm")
                    : "",
                x.EstatusHorasExtras,
                x.UbicacionEntrada,
                x.UbicacionSalida,
                x.MacEntrada,
                x.MacSalida,
                x.IP,
                EstatusChecada =
                !x.HoraEntrada.HasValue ? "No checó nada" :
                !x.HoraSalida.HasValue ? "No checó salida" :
                !x.HoraSalidaComer.HasValue ? "No checó salida a comer" :
                !x.HoraEntradaComer.HasValue ? "No checó regreso de comer" :
                "Completo",
            }).ToList();

            GridView gvExport = new GridView();
            gvExport.AutoGenerateColumns = false;
            gvExport.EnableViewState = false;
            gvExport.GridLines = GridLines.Both;
            gvExport.HeaderStyle.Font.Bold = true;

            // Definir columnas (sin HorasTrabajadasDecimal)
            string[] campos = { "EMPLEADO", "Planta", "Fecha", "HoraEntrada", "HoraSalida",
        "HoraSalidaComer", "HoraEntradaComer", "HorasTrabajadas", "tiempoComida",
        "EstatusEntrada", "EstatusSalida", "EstatusComida", "TipoPermiso",
        "HoraSalidaPermiso", "HoraEntradaPermiso", "HorasPermiso",
        "horaSalidaComision", "horaEntradaComision", "horasComision",
        "HorasExtras", "EstatusHorasExtras", "UbicacionEntrada", "UbicacionSalida",
        "MacEntrada", "MacSalida", "IP", "EstatusChecada" };

            string[] encabezados = { "EMPLEADO", "Planta", "Fecha", "Hora Entrada", "Hora Salida",
        "Salida Comer", "Entrada Comer", "Horas Trabajadas", "Tiempo Comida",
        "Estatus Entrada", "Estatus Salida", "Estatus Comida", "Tipo Permiso",
        "Salida Permiso", "Entrada Permiso", "Horas Permiso",
        "Salida Comisión", "Entrada Comisión", "Horas Comisión",
        "Horas Extras", "Estatus Horas Extras", "Ubicación Entrada", "Ubicación Salida",
        "Mac Entrada", "Mac Salida", "IP", "Estatus Checada" };

            for (int i = 0; i < campos.Length; i++)
            {
                gvExport.Columns.Add(new BoundField
                {
                    DataField = campos[i],
                    HeaderText = encabezados[i]
                });
            }

            gvExport.DataSource = dataTransformada;
            gvExport.DataBind();

            StringWriter sw = new StringWriter();
            HtmlTextWriter hw = new HtmlTextWriter(sw);
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

        public string GetEstatusChecada(object horaEntrada, object horaSalida,
                                 object horaSalidaComer, object horaEntradaComer)
        {
            bool tieneEntrada = horaEntrada != null && horaEntrada != DBNull.Value;
            bool tieneSalida = horaSalida != null && horaSalida != DBNull.Value;
            bool tieneSalidaComer = horaSalidaComer != null && horaSalidaComer != DBNull.Value;
            bool tieneEntradaComer = horaEntradaComer != null && horaEntradaComer != DBNull.Value;

            if (!tieneEntrada)
                return "<span style='color:red;font-weight:bold;'>No checó nada</span>";

            if (tieneEntrada && !tieneSalida)
                return "<span style='color:orange;font-weight:bold;'>No checó salida</span>";

            if (tieneEntrada && tieneSalida && !tieneSalidaComer)
                return "<span style='color:#cc8800;font-weight:bold;'>No checó salida a comer</span>";

            if (tieneEntrada && tieneSalidaComer && !tieneEntradaComer)
                return "<span style='color:#cc8800;font-weight:bold;'>No checó regreso de comer</span>";

            return "<span style='color:green;font-weight:bold;'>Completo</span>";
        }
    }
}