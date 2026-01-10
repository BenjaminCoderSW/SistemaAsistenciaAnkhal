using GrupoAnkhalAsistencia.Modelo;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using MedicaMedens.Sesion;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;


namespace GrupoAnkhalAsistencia
{
    public partial class HistorialEmpleado : System.Web.UI.Page
    {
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
        ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        public int UsuarioSesion = SesionState.usuario.IdUsuario;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarHistorialEmpleado();

            }
        }

        private void CargarHistorialEmpleado()
        {
            var empleado = from m in db.V_HISTORIAL_EMPLEADO
                           where m.IdUsuario == UsuarioSesion
                           orderby m.Fecha descending
                           select new
                           {
                               m.EMPLEADO,
                               m.Planta,
                               m.IdUsuario,
                               m.Fecha,
                               m.HoraEntrada,
                               m.HoraSalida,
                               m.HoraSalidaComer,
                               m.HoraEntradaComer,
                               m.HorasTrabajadas,
                               m.tiempoComida,
                               m.EstatusEntrada,
                               m.EstatusSalida,
                               m.EstatusComida,
                               m.UbicacionEntrada,
                               m.UbicacionSalida,
                               m.MacEntrada,
                               m.MacSalida,
                               m.IP
                           };

            dvgHistorialEmpleado.DataSource = empleado.ToList();
            dvgHistorialEmpleado.DataBind();
        }

        private void BuscarEmpleado(string filtro = "")
        {
            var query = from m in db.V_HISTORIAL_EMPLEADO
                           where m.IdUsuario == UsuarioSesion
                           orderby m.Fecha descending
                           select new
                           {
                               m.EMPLEADO,
                               m.Planta,
                               m.IdUsuario,
                               m.Fecha,
                               m.HoraEntrada,
                               m.HoraSalida,
                               m.HoraSalidaComer,
                               m.HoraEntradaComer,
                               m.HorasTrabajadas,
                               m.tiempoComida,
                               m.EstatusEntrada,
                               m.EstatusSalida,
                               m.EstatusComida,
                               m.UbicacionEntrada,
                               m.UbicacionSalida,
                               m.MacEntrada,
                               m.MacSalida,
                               m.IP
                           };

            if (!string.IsNullOrEmpty(filtro))
            {
                query = query.Where(x =>
                    System.Data.Linq.SqlClient.SqlMethods.Like(x.EMPLEADO, "%" + filtro + "%")
                );
            }

            dvgHistorialEmpleado.DataSource = query.ToList();
            dvgHistorialEmpleado.DataBind();
        }

        protected void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            BuscarEmpleado(txtBuscar.Text.Trim());
        }

        protected void dvgHistorialEmpleado_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dvgHistorialEmpleado.PageIndex = e.NewPageIndex;
            BuscarEmpleado(txtBuscar.Text.Trim()); // mantiene el filtro
        }

        public string GetMapaLink(string ubicacion)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ubicacion))
                    return "";

                // Ubicacion viene como "lat,lng"
                var partes = ubicacion.Split(',');
                if (partes.Length != 2)
                    return "";

                string lat = partes[0];
                string lng = partes[1];

                string url = $"https://www.google.com/maps?q={lat},{lng}";

                return $"<a href='{url}' target='_blank'><img src='/img/mapa.png' width='25' /></a>";
            }
            catch
            {
                return "";
            }
        }

        protected void btnExportPDF_Click(object sender, EventArgs e)
        {
            // CONFIGURACIÓN PDF
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=Asistencia.pdf");
            Response.CacheControl = "no-cache";

            Document pdfDoc = new Document(PageSize.A4.Rotate(), 20f, 20f, 20f, 20f);
            PdfWriter writer = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
            pdfDoc.Open();

            // ================================
            // 1) AGREGAR LOGO ANKHAL
            // ================================
            string rutaLogo = Server.MapPath("~/img/ankhal.png");

            if (File.Exists(rutaLogo))
            {
                iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(rutaLogo);
                logo.ScaleAbsolute(120, 60);          // Tamaño del logo
                logo.Alignment = Element.ALIGN_LEFT;  // Alineación izquierda
                pdfDoc.Add(logo);

                // Texto debajo del logo
                Paragraph textoEmpresa = new Paragraph("GRUPO ANKHAL",
                    new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 15, iTextSharp.text.Font.BOLD, BaseColor.BLACK));

                textoEmpresa.Alignment = Element.ALIGN_LEFT;  // alineado a la izquierda
                textoEmpresa.SpacingBefore = 5f;              // espacio entre el logo y el texto

                pdfDoc.Add(textoEmpresa);
            }

            // ================================
            // 2) TÍTULO DEL REPORTE
            // ================================
            Paragraph titulo = new Paragraph("HISTORIAL DEL EMPLEADO\n\n",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLACK));
            titulo.Alignment = Element.ALIGN_CENTER;
            pdfDoc.Add(titulo);

            // ================================
            // 3) CREAR TABLA PDF
            // ================================
            PdfPTable tabla = new PdfPTable(dvgHistorialEmpleado.Columns.Count - 2); // Quitamos MacEntrada & MacSalida
            tabla.WidthPercentage = 100;

            // COLORES
            BaseColor headerColor = new BaseColor(0, 51, 102); // azul marino
            BaseColor headerTextColor = BaseColor.WHITE;

            // ================================
            // ENCABEZADOS DE LA TABLA
            // ================================
            for (int i = 0; i < dvgHistorialEmpleado.Columns.Count; i++)
            {
                if (i == 12 || i == 13) continue; // omitir columnas MAC

                PdfPCell celda = new PdfPCell(new Phrase(dvgHistorialEmpleado.Columns[i].HeaderText,
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 5, headerTextColor)));

                celda.BackgroundColor = headerColor;
                celda.HorizontalAlignment = Element.ALIGN_CENTER;
                celda.Padding = 5;

                tabla.AddCell(celda);
            }

            // ================================
            // DATOS DE LAS FILAS
            // ================================
            foreach (GridViewRow row in dvgHistorialEmpleado.Rows)
            {
                for (int j = 0; j < dvgHistorialEmpleado.Columns.Count; j++)
                {
                    if (j == 12 || j == 13) continue; // las columnas que no quieres

                    string texto = "";

                    // TemplateField: buscar el primer control dentro de la celda
                    if (dvgHistorialEmpleado.Columns[j] is TemplateField)
                    {
                        if (row.Cells[j].Controls.Count > 0)
                        {
                            // el texto real está en el LiteralControl 0
                            var literal = row.Cells[j].Controls[0] as LiteralControl;
                            if (literal != null)
                            {
                                texto = literal.Text.Trim();
                            }
                        }
                    }
                    else
                    {
                        texto = row.Cells[j].Text.Replace("&nbsp;", "").Trim();
                    }

                    PdfPCell celda = new PdfPCell(new Phrase(
                        texto,
                        FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.BLACK)
                    ));

                    celda.HorizontalAlignment = Element.ALIGN_CENTER;
                    celda.VerticalAlignment = Element.ALIGN_MIDDLE;
                    celda.Padding = 4;

                    tabla.AddCell(celda);
                }
            }




            pdfDoc.Add(tabla);
            pdfDoc.Close();

            Response.End();
        }

        





        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            Response.Clear();
            Response.Buffer = true;

            Response.AddHeader("content-disposition", "attachment;filename=Asistencia.xls");
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-excel";

            StringWriter sw = new StringWriter();
            HtmlTextWriter hw = new HtmlTextWriter(sw);

            dvgHistorialEmpleado.GridLines = GridLines.Both;   // tu GridView
            dvgHistorialEmpleado.HeaderStyle.Font.Bold = true;

            dvgHistorialEmpleado.RenderControl(hw);

            Response.Output.Write(sw.ToString());
            Response.Flush();
            Response.End();
        }

        public override void VerifyRenderingInServerForm(Control control)
        {
            // Necesario para exportar
        }
    }
}