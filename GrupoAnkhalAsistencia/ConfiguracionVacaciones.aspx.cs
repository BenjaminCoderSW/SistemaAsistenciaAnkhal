using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Configuration;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class ConfiguracionVacaciones : System.Web.UI.Page
    {
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
            ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (SesionState.usuario == null)
            {
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
                CargarConfiguracion();
            }
        }

        private void CargarConfiguracion()
        {
            var config = from c in db.tConfiguracionVacaciones
                         where c.Estatus == 1
                         orderby c.AñosAntiguedad
                         select new
                         {
                             c.IdConfigVacaciones,
                             c.AñosAntiguedad,
                             c.DiasCorresponden,
                             c.Estatus
                         };

            dvgConfiguracion.DataSource = config.ToList();
            dvgConfiguracion.DataBind();
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAnios.Text) || string.IsNullOrWhiteSpace(txtDias.Text))
            {
                MostrarAlerta("warning", "Campos obligatorios", "Debe completar todos los campos.");
                return;
            }

            try
            {
                int anios = Convert.ToInt32(txtAnios.Text);
                int dias = Convert.ToInt32(txtDias.Text);

                // Validar que no exista ya
                if (db.tConfiguracionVacaciones.Any(c => c.AñosAntiguedad == anios && c.Estatus == 1))
                {
                    MostrarAlerta("error", "Duplicado", "Ya existe una configuración para " + anios + " años.");
                    return;
                }

                tConfiguracionVacaciones nueva = new tConfiguracionVacaciones
                {
                    AñosAntiguedad = anios,
                    DiasCorresponden = dias,
                    Estatus = 1
                };

                db.tConfiguracionVacaciones.InsertOnSubmit(nueva);
                db.SubmitChanges();

                LimpiarCampos();
                CargarConfiguracion();

                ScriptManager.RegisterStartupScript(this, GetType(), "cerrarModal",
                    "$('#modalAgregar').modal('hide');", true);

                MostrarAlerta("success", "Guardado", "Configuración agregada correctamente.");
            }
            catch (Exception ex)
            {
                MostrarAlerta("error", "Error", "No se pudo guardar: " + ex.Message);
            }
        }

        protected void btnGuardarModal_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(hfIdConfig.Value))
            {
                MostrarAlerta("error", "Error", "No se encontró el ID de configuración.");
                return;
            }

            int id = Convert.ToInt32(hfIdConfig.Value);

            try
            {
                var config = db.tConfiguracionVacaciones.FirstOrDefault(c => c.IdConfigVacaciones == id);
                if (config == null)
                {
                    MostrarAlerta("error", "Error", "Configuración no encontrada.");
                    return;
                }

                int anios = Convert.ToInt32(txtAniosModal.Text);
                int dias = Convert.ToInt32(txtDiasModal.Text);

                // Validar duplicados (excluyendo el registro actual)
                if (db.tConfiguracionVacaciones.Any(c =>
                    c.AñosAntiguedad == anios &&
                    c.Estatus == 1 &&
                    c.IdConfigVacaciones != id))
                {
                    MostrarAlerta("error", "Duplicado", "Ya existe una configuración para " + anios + " años.");
                    return;
                }

                config.AñosAntiguedad = anios;
                config.DiasCorresponden = dias;

                db.SubmitChanges();
                CargarConfiguracion();

                ScriptManager.RegisterStartupScript(this, GetType(), "cerrarModalEditar",
                    "$('#modalEditar').modal('hide');", true);

                MostrarAlerta("success", "Actualizado", "Configuración actualizada correctamente.");
            }
            catch (Exception ex)
            {
                MostrarAlerta("error", "Error", "No se pudo actualizar: " + ex.Message);
            }
        }

        protected void btnEliminar_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int id = Convert.ToInt32(btn.CommandArgument);

            try
            {
                var config = db.tConfiguracionVacaciones.FirstOrDefault(c => c.IdConfigVacaciones == id);
                if (config != null)
                {
                    config.Estatus = 0;
                    db.SubmitChanges();

                    CargarConfiguracion();
                    MostrarAlerta("success", "Eliminado", "Configuración eliminada correctamente.");
                }
            }
            catch (Exception ex)
            {
                MostrarAlerta("error", "Error", "No se pudo eliminar: " + ex.Message);
            }
        }

        protected void btnActualizarTodos_Click(object sender, EventArgs e)
        {
            try
            {
                // Solo asignar días a empleados que NUNCA han tenido una actualización (empleados nuevos)
                // NO tocar a quienes ya tienen FechaUltimaActualizacionVacaciones, porque
                // sus días disponibles pueden estar reducidos por vacaciones ya autorizadas.
                int usuariosActualizados = db.ExecuteQuery<int>(@"
                    UPDATE tUsuario
                    SET 
                        DiasVacacionesDisponibles = cv.DiasCorresponden,
                        FechaUltimaActualizacionVacaciones = GETDATE()
                    OUTPUT 1
                    FROM tUsuario u
                    CROSS APPLY (
                        SELECT TOP 1 DiasCorresponden
                        FROM tConfiguracionVacaciones
                        WHERE AñosAntiguedad <= DATEDIFF(YEAR, u.FechaIngreso, GETDATE())
                          AND Estatus = 1
                        ORDER BY AñosAntiguedad DESC
                    ) cv
                    WHERE u.Estatus = 1
                      AND u.FechaIngreso IS NOT NULL
                      AND u.FechaUltimaActualizacionVacaciones IS NULL
                ").Count();

                MostrarAlerta("success", "Actualización completa",
                    $"Se asignaron días a {usuariosActualizados} empleados nuevos. " +
                    "Los empleados con vacaciones ya autorizadas no fueron modificados.");
            }
            catch (Exception ex)
            {
                MostrarAlerta("error", "Error", "No se pudo actualizar: " + ex.Message);
            }
        }

        private void LimpiarCampos()
        {
            txtAnios.Text = "";
            txtDias.Text = "";
        }

        private void MostrarAlerta(string icono, string titulo, string mensaje)
        {
            string script = $@"
                Swal.fire({{
                    icon: '{icono}',
                    title: '{titulo}',
                    text: '{mensaje}',
                    showConfirmButton: false,
                    timer: 2500
                }});";
            ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), script, true);
        }
    }
}