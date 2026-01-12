using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlTypes;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;

namespace GrupoAnkhalAsistencia
{
    public partial class Papeleta : System.Web.UI.Page
    {
        private dbAsistenciaDataContext db = new dbAsistenciaDataContext(System.Configuration.ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            // Validar acceso según roles permitidos
            string[] rolesPermitidos = { "Administrador", "Rh" };
            if (SesionState.usuario == null || !rolesPermitidos.Contains(SesionState.usuario.tRol.Rol))
            {
                Response.Redirect("login.aspx");
            }

            if (!IsPostBack)
            {
                CargarUsuarios();
                CargarPapeletas();
            }
        }

        private void CargarUsuarios()
        {
            var usuarios = (from u in db.tUsuario
                            where u.Estatus == 1
                            orderby u.Nombre
                            select new
                            {
                                u.IdUsuario,
                                NombreCompleto = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno
                            }).ToList();

            ddlUsuario.DataSource = usuarios;
            ddlUsuario.DataTextField = "NombreCompleto";
            ddlUsuario.DataValueField = "IdUsuario";
            ddlUsuario.DataBind();
            ddlUsuario.Items.Insert(0, new ListItem("-- Seleccione un empleado --", ""));
        }

        private void CargarPapeletas()
        {
            var papeletas = from p in db.V_PAPELETAS
                            orderby p.FechaPago descending
                            select p;

            dvgPapeleta.DataSource = papeletas.ToList();
            dvgPapeleta.DataBind();
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                // Validaciones básicas
                if (string.IsNullOrEmpty(ddlUsuario.SelectedValue))
                {
                    MostrarAlerta("error", "Error", "Debe seleccionar un empleado");
                    return;
                }

                if (string.IsNullOrEmpty(txtPeriodoPago.Text.Trim()))
                {
                    MostrarAlerta("error", "Error", "El periodo de pago es obligatorio");
                    return;
                }

                if (string.IsNullOrEmpty(txtDiasPagados.Text.Trim()))
                {
                    MostrarAlerta("error", "Error", "Los días pagados son obligatorios");
                    return;
                }

                if (string.IsNullOrEmpty(txtFechaPago.Text.Trim()))
                {
                    MostrarAlerta("error", "Error", "La fecha de pago es obligatoria");
                    return;
                }

                int idPapeleta = Convert.ToInt32(hdIdPapeleta.Value);

                if (idPapeleta == 0) // Nuevo registro
                {
                    tPapeleta nuevaPapeleta = new tPapeleta
                    {
                        IdUsuario = Convert.ToInt32(ddlUsuario.SelectedValue),
                        PeriodoPago = txtPeriodoPago.Text.Trim(),
                        DiasPagados = Convert.ToInt32(txtDiasPagados.Text.Trim()),
                        FechaPago = Convert.ToDateTime(txtFechaPago.Text),
                        SueldoPeriodo = ConvertirDecimal(txtSueldoPeriodo.Text),
                        HorasExtras = ConvertirDecimal(txtHorasExtras.Text),
                        Bonos = ConvertirDecimal(txtBonos.Text),
                        DiasPendientesPago = ConvertirDecimal(txtDiasPendientesPago.Text),
                        Veladas = ConvertirDecimal(txtVeladas.Text),
                        OtrosIngresos = ConvertirDecimal(txtOtrosIngresos.Text),
                        DiasNoLaborados = ConvertirDecimal(txtDiasNoLaborados.Text),
                        DescuentoHoras = ConvertirDecimal(txtDescuentoHoras.Text),
                        OtrosDescuentos = ConvertirDecimal(txtOtrosDescuentos.Text),
                        Estatus = 1
                    };

                    db.tPapeleta.InsertOnSubmit(nuevaPapeleta);
                    db.SubmitChanges();

                    MostrarAlerta("success", "Éxito", "Papeleta registrada correctamente");
                }
                else // Editar registro existente
                {
                    var papeleta = db.tPapeleta.FirstOrDefault(p => p.IdPapeleta == idPapeleta);
                    if (papeleta != null)
                    {
                        papeleta.IdUsuario = Convert.ToInt32(ddlUsuario.SelectedValue);
                        papeleta.PeriodoPago = txtPeriodoPago.Text.Trim();
                        papeleta.DiasPagados = Convert.ToInt32(txtDiasPagados.Text.Trim());
                        papeleta.FechaPago = Convert.ToDateTime(txtFechaPago.Text);
                        papeleta.SueldoPeriodo = ConvertirDecimal(txtSueldoPeriodo.Text);
                        papeleta.HorasExtras = ConvertirDecimal(txtHorasExtras.Text);
                        papeleta.Bonos = ConvertirDecimal(txtBonos.Text);
                        papeleta.DiasPendientesPago = ConvertirDecimal(txtDiasPendientesPago.Text);
                        papeleta.Veladas = ConvertirDecimal(txtVeladas.Text);
                        papeleta.OtrosIngresos = ConvertirDecimal(txtOtrosIngresos.Text);
                        papeleta.DiasNoLaborados = ConvertirDecimal(txtDiasNoLaborados.Text);
                        papeleta.DescuentoHoras = ConvertirDecimal(txtDescuentoHoras.Text);
                        papeleta.OtrosDescuentos = ConvertirDecimal(txtOtrosDescuentos.Text);

                        db.SubmitChanges();

                        MostrarAlerta("success", "Éxito", "Papeleta actualizada correctamente");
                    }
                }

                CargarPapeletas();
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MostrarAlerta("error", "Error", "Ocurrió un error: " + ex.Message);
            }
        }

        protected void btnEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                int idPapeleta = Convert.ToInt32(btn.CommandArgument);

                var papeleta = db.tPapeleta.FirstOrDefault(p => p.IdPapeleta == idPapeleta);
                if (papeleta != null)
                {
                    papeleta.Estatus = 0; // Soft delete
                    db.SubmitChanges();

                    MostrarAlerta("success", "Éxito", "Papeleta eliminada correctamente");
                    CargarPapeletas();
                }
            }
            catch (Exception ex)
            {
                MostrarAlerta("error", "Error", "Ocurrió un error al eliminar: " + ex.Message);
            }
        }

        protected void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(filtro))
            {
                CargarPapeletas();
            }
            else
            {
                var papeletas = from p in db.V_PAPELETAS
                                where p.NombreCompleto.ToLower().Contains(filtro) ||
                                      p.RFC.ToLower().Contains(filtro) ||
                                      p.NTrabajador.ToLower().Contains(filtro)
                                orderby p.FechaPago descending
                                select p;

                dvgPapeleta.DataSource = papeletas.ToList();
                dvgPapeleta.DataBind();
            }
        }

        protected void dvgPapeleta_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dvgPapeleta.PageIndex = e.NewPageIndex;

            if (!string.IsNullOrEmpty(txtBuscar.Text.Trim()))
            {
                txtBuscar_TextChanged(sender, e);
            }
            else
            {
                CargarPapeletas();
            }
        }

        private decimal ConvertirDecimal(string texto)
        {
            decimal resultado = 0;
            if (!string.IsNullOrEmpty(texto))
            {
                decimal.TryParse(texto, out resultado);
            }
            return resultado;
        }

        private void LimpiarFormulario()
        {
            hdIdPapeleta.Value = "0";
            ddlUsuario.SelectedIndex = 0;
            txtPeriodoPago.Text = "";
            txtDiasPagados.Text = "";
            txtFechaPago.Text = "";
            txtSueldoPeriodo.Text = "";
            txtHorasExtras.Text = "";
            txtBonos.Text = "";
            txtDiasPendientesPago.Text = "";
            txtVeladas.Text = "";
            txtOtrosIngresos.Text = "";
            txtDiasNoLaborados.Text = "";
            txtDescuentoHoras.Text = "";
            txtOtrosDescuentos.Text = "";
        }

        private void MostrarAlerta(string icon, string title, string text)
        {
            string script = $@"
                Swal.fire({{
                    icon: '{icon}',
                    title: '{title}',
                    text: '{text}',
                    timer: 3000
                }});
            ";
            ScriptManager.RegisterStartupScript(this, GetType(), "showalert", script, true);
        }
    }
}