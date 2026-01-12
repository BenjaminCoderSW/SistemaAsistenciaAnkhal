using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using GrupoAnkhalAsistencia.Modelo;

namespace GrupoAnkhalAsistencia
{
    public partial class ImprimirPapeleta : System.Web.UI.Page
    {
        private dbAsistenciaDataContext db = new dbAsistenciaDataContext(System.Configuration.ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int idPapeleta = 0;
                if (Request.QueryString["id"] != null)
                {
                    int.TryParse(Request.QueryString["id"], out idPapeleta);
                }

                if (idPapeleta > 0)
                {
                    CargarDatosPapeleta(idPapeleta);
                }
                else
                {
                    Response.Write("<script>alert('ID de papeleta no válido'); window.close();</script>");
                }
            }
        }

        private void CargarDatosPapeleta(int idPapeleta)
        {
            try
            {
                var papeleta = (from p in db.V_PAPELETAS
                                where p.IdPapeleta == idPapeleta
                                select p).FirstOrDefault();

                if (papeleta != null)
                {
                    // Información del empleado
                    litNTrabajador.Text = papeleta.NTrabajador ?? "N/A";
                    litNombre.Text = papeleta.NombreCompleto ?? "N/A";
                    litPuesto.Text = papeleta.Puesto ?? "N/A";
                    litPeriodo.Text = papeleta.PeriodoPago ?? "N/A";
                    litFechaPago.Text = papeleta.FechaPago.HasValue ?
                        papeleta.FechaPago.Value.ToString("dd/MM/yyyy") : "N/A";
                    litDiasPagados.Text = papeleta.DiasPagados.HasValue ?
                        papeleta.DiasPagados.Value.ToString() : "0";

                    // Percepciones
                    litSueldoPeriodo.Text = FormatearDecimal(papeleta.SueldoPeriodo);
                    litHorasExtras.Text = FormatearDecimal(papeleta.HorasExtras);
                    litBonos.Text = FormatearDecimal(papeleta.Bonos);
                    litDiasPendientes.Text = FormatearDecimal(papeleta.DiasPendientesPago);
                    litVeladas.Text = FormatearDecimal(papeleta.Veladas);
                    litOtrosIngresos.Text = FormatearDecimal(papeleta.OtrosIngresos);
                    litTotalPercepciones.Text = FormatearDecimal(papeleta.TotalPercepciones);

                    // Deducciones
                    litDiasNoLaborados.Text = FormatearDecimal(papeleta.DiasNoLaborados);
                    litDescuentoHoras.Text = FormatearDecimal(papeleta.DescuentoHoras);
                    litOtrosDescuentos.Text = FormatearDecimal(papeleta.OtrosDescuentos);
                    litTotalDeducciones.Text = FormatearDecimal(papeleta.TotalDeducciones);

                    // Resumen final
                    litSubtotal.Text = FormatearDecimal(papeleta.Subtotal);
                    litDescuento.Text = FormatearDecimal(papeleta.Descuento);
                    litNetoPagar.Text = FormatearDecimal(papeleta.NetoPagar);
                }
                else
                {
                    Response.Write("<script>alert('No se encontró la papeleta'); window.close();</script>");
                }
            }
            catch (Exception ex)
            {
                Response.Write($"<script>alert('Error al cargar la papeleta: {ex.Message}'); window.close();</script>");
            }
        }

        private string FormatearDecimal(decimal? valor)
        {
            if (valor.HasValue)
            {
                return valor.Value.ToString("N2");
            }
            return "-";
        }
    }
}