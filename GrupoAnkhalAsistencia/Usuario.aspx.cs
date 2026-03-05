using GrupoAnkhalAsistencia.Modelo;
using MedicaMedens.Sesion;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class Usuario : System.Web.UI.Page
    {
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
            ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        // ── ViewState para mantener el filtro al paginar ──────────────────────────
        private string FiltroActual
        {
            get { return ViewState["FiltroUsuario"] as string ?? ""; }
            set { ViewState["FiltroUsuario"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
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
                CargarUsuarioFiltrado();
                CargarRol();
                CargarPuesto();
                CargarArea();
                CargarPlanta();
            }
        }

        // ── Carga con filtro opcional ────────────────────────────────────────────
        private void CargarUsuarioFiltrado(string filtro = "")
        {
            var query = from m in db.tUsuario
                        join r in db.tRol on m.IdRol equals r.IdRol
                        join p in db.tPuesto on m.IdPuesto equals p.IdPuesto
                        join a in db.tArea on m.IdArea equals a.IdArea
                        join pl in db.tPlanta on m.IdPlanta equals pl.IdPlanta into plantaJoin
                        from pl in plantaJoin.DefaultIfEmpty()
                        where m.Estatus == 1
                        orderby m.Nombre
                        select new
                        {
                            m.IdUsuario,
                            m.IdRol,
                            m.IdPuesto,
                            m.IdArea,
                            m.IdPlanta,
                            Rol = r.Rol,
                            Puesto = p.Puesto,
                            Area = a.Area,
                            Planta = pl.Planta ?? "Sin asignar",
                            m.Nombre,
                            m.ApellidoPaterno,
                            m.ApellidoMaterno,
                            m.Curp,
                            m.RfC,
                            m.FechaNacimiento,
                            m.FechaIngreso,
                            m.Genero,
                            m.EstadoSocial,
                            m.Telefono,
                            m.SeguroSocial,
                            m.NumeroEmpleado,
                            m.Email,
                            m.Direccion,
                            m.NombreFamilia,
                            m.TelefonoFamiliar,
                            m.Usuario,
                            m.Clave,
                            m.Edad,
                            m.Dispositivo1,
                            m.Mac1,
                            m.Dispositivo2,
                            m.Mac2
                        };

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                query = query.Where(x =>
                    System.Data.Linq.SqlClient.SqlMethods.Like(x.Curp, "%" + filtro + "%") ||
                    System.Data.Linq.SqlClient.SqlMethods.Like(x.Nombre, "%" + filtro + "%") ||
                    System.Data.Linq.SqlClient.SqlMethods.Like(x.ApellidoPaterno, "%" + filtro + "%") ||
                    System.Data.Linq.SqlClient.SqlMethods.Like(x.ApellidoMaterno, "%" + filtro + "%"));
            }

            dvgUsuario.DataSource = query.ToList();
            dvgUsuario.DataBind();
        }

        // ── Búsqueda ─────────────────────────────────────────────────────────────
        protected void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            FiltroActual = txtBuscar.Text.Trim();
            dvgUsuario.PageIndex = 0;               // volver a primera página al buscar
            CargarUsuarioFiltrado(FiltroActual);
        }

        // ── Paginación ───────────────────────────────────────────────────────────
        protected void dvgUsuario_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dvgUsuario.PageIndex = e.NewPageIndex;
            CargarUsuarioFiltrado(FiltroActual);    // mantiene el filtro
        }

        // ── Catálogos ────────────────────────────────────────────────────────────
        private void CargarPlanta()
        {
            var planta = db.tPlanta
                .Select(t => new { t.IdPlanta, t.Planta })
                .ToList();

            ddlPlanta.DataSource = planta;
            ddlPlanta.DataTextField = "Planta";
            ddlPlanta.DataValueField = "IdPlanta";
            ddlPlanta.DataBind();
            ddlPlanta.Items.Insert(0, new ListItem("-- Seleccione --", "0"));

            ddlPlantaModal.DataSource = planta;
            ddlPlantaModal.DataTextField = "Planta";
            ddlPlantaModal.DataValueField = "IdPlanta";
            ddlPlantaModal.DataBind();
            ddlPlantaModal.Items.Insert(0, new ListItem("-- Seleccione --", "0"));
        }

        private void CargarRol()
        {
            var rol = db.tRol
                .Select(r => new { r.IdRol, r.Rol })
                .ToList();

            ddlRol.DataSource = rol;
            ddlRol.DataTextField = "Rol";
            ddlRol.DataValueField = "IdRol";
            ddlRol.DataBind();
            ddlRol.Items.Insert(0, new ListItem("-- Seleccione --", "0"));

            ddlRolModal.DataSource = rol;
            ddlRolModal.DataTextField = "Rol";
            ddlRolModal.DataValueField = "IdRol";
            ddlRolModal.DataBind();
            ddlRolModal.Items.Insert(0, new ListItem("-- Seleccione --", "0"));
        }

        private void CargarPuesto()
        {
            var puesto = db.tPuesto
                .Select(t => new { t.IdPuesto, t.Puesto })
                .ToList();

            ddlPuesto.DataSource = puesto;
            ddlPuesto.DataTextField = "Puesto";
            ddlPuesto.DataValueField = "IdPuesto";
            ddlPuesto.DataBind();
            ddlPuesto.Items.Insert(0, new ListItem("-- Seleccione --", "0"));

            ddlPuestoModal.DataSource = puesto;
            ddlPuestoModal.DataTextField = "Puesto";
            ddlPuestoModal.DataValueField = "IdPuesto";
            ddlPuestoModal.DataBind();
            ddlPuestoModal.Items.Insert(0, new ListItem("-- Seleccione --", "0"));
        }

        private void CargarArea()
        {
            var area = db.tArea
                .Select(t => new { t.IdArea, t.Area })
                .ToList();

            ddlArea.DataSource = area;
            ddlArea.DataTextField = "Area";
            ddlArea.DataValueField = "IdArea";
            ddlArea.DataBind();
            ddlArea.Items.Insert(0, new ListItem("-- Seleccione --", "0"));

            ddlAreaModal.DataSource = area;
            ddlAreaModal.DataTextField = "Area";
            ddlAreaModal.DataValueField = "IdArea";
            ddlAreaModal.DataBind();
            ddlAreaModal.Items.Insert(0, new ListItem("-- Seleccione --", "0"));
        }

        // ── Guardar nuevo usuario ────────────────────────────────────────────────
        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarCampos()) return;

            try
            {
                string curp = txtCurp.Text.Trim();
                string rfc = txtRFC.Text.Trim();
                string usuario = txtUsuario.Text.Trim();
                string numeroEmpleado = txtNumeroEmpleado.Text.Trim();
                string email = txtEmail.Text.Trim();

                var duplicados = new List<string>();

                if (db.tUsuario.Any(u => u.Curp == curp && u.Estatus == 1))
                    duplicados.Add("CURP");

                if (!string.IsNullOrEmpty(rfc) && db.tUsuario.Any(u => u.RfC == rfc && u.Estatus == 1))
                    duplicados.Add("RFC");

                if (db.tUsuario.Any(u => u.Usuario == usuario && u.Estatus == 1))
                    duplicados.Add("Usuario");

                if (!string.IsNullOrEmpty(numeroEmpleado) && db.tUsuario.Any(u => u.NumeroEmpleado == numeroEmpleado && u.Estatus == 1))
                    duplicados.Add("Número de Empleado");

                if (!string.IsNullOrEmpty(email) && db.tUsuario.Any(u => u.Email == email && u.Estatus == 1))
                    duplicados.Add("Correo Electrónico");

                if (duplicados.Count > 0)
                {
                    string campos = string.Join(", ", duplicados);
                    MostrarAlerta("error", "Datos duplicados",
                        $"Ya existe un usuario registrado con el mismo: {campos}. Por favor verifica la información.");
                    return;
                }

                DateTime fechaNacimiento = Convert.ToDateTime(FechaNacimiento.Text.Trim());
                DateTime fechaIngreso = Convert.ToDateTime(FechaIngreso.Text.Trim());

                string base64 = hdFoto.Value;
                if (string.IsNullOrEmpty(base64))
                {
                    MostrarAlerta("warning", "Foto requerida", "Debes tomar una foto antes de guardar.");
                    return;
                }

                base64 = base64
                    .Replace("data:image/png;base64,", "")
                    .Replace("data:image/jpeg;base64,", "")
                    .Replace("data:image/jpg;base64,", "");

                byte[] fotoBytes = Convert.FromBase64String(base64);

                tUsuario nuevo = new tUsuario
                {
                    IdRol = Convert.ToInt32(ddlRol.SelectedValue),
                    IdPuesto = Convert.ToInt32(ddlPuesto.SelectedValue),
                    IdArea = Convert.ToInt32(ddlArea.SelectedValue),
                    IdPlanta = ddlPlanta.SelectedValue != "0" ? (int?)Convert.ToInt32(ddlPlanta.SelectedValue) : null,
                    Nombre = txtNombre.Text.Trim(),
                    ApellidoPaterno = txtApellidoPaterno.Text.Trim(),
                    ApellidoMaterno = txtApellidoMaterno.Text.Trim(),
                    Curp = curp,
                    RfC = rfc,
                    FechaNacimiento = fechaNacimiento,
                    FechaIngreso = fechaIngreso,
                    Genero = ddlGenero.Text,
                    EstadoSocial = EstadoSocial.Text,
                    Telefono = txtTelefono.Text.Trim(),
                    SeguroSocial = txtSeguroSocial.Text.Trim(),
                    NumeroEmpleado = numeroEmpleado,
                    Email = email,
                    Direccion = txtDireccion.Text.Trim(),
                    NombreFamilia = txtNombreFamilia.Text.Trim(),
                    TelefonoFamiliar = txtTelefonoFamiliar.Text.Trim(),
                    Usuario = usuario,
                    Clave = txtClave.Text,
                    Edad = ConvertirEntero(txtEdad.Text),
                    Dispositivo1 = txtDispositivo1.Text.Trim(),
                    Mac1 = txtMac1.Text.Trim(),
                    Dispositivo2 = txtDispositivo2.Text.Trim(),
                    Mac2 = txtMac2.Text.Trim(),
                    Foto = fotoBytes,
                    Estatus = 1
                };

                db.tUsuario.InsertOnSubmit(nuevo);
                db.SubmitChanges();

                LimpiarCampos();
                CargarUsuarioFiltrado(FiltroActual);
                MostrarAlerta("success", "Guardado exitoso", "El usuario fue registrado correctamente.");
            }
            catch (Exception ex)
            {
                MostrarAlerta("error", "Error", "No se pudo guardar el usuario: " + ex.Message);
            }
        }

        // ── Guardar edición ──────────────────────────────────────────────────────
        protected void btnGuardarModal_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(hfIdUsuario.Value))
            {
                MostrarAlerta("error", "Error", "No se encontró el ID del usuario.");
                return;
            }

            int id = Convert.ToInt32(hfIdUsuario.Value);

            try
            {
                var usuarioReg = db.tUsuario.FirstOrDefault(t => t.IdUsuario == id);
                if (usuarioReg == null)
                {
                    MostrarAlerta("error", "Error", "El usuario no existe.");
                    return;
                }

                string curp = txtCurpModal.Text.Trim();
                string rfc = txtRfcModal.Text.Trim();
                string usuarioNombre = txtUsuarioModal.Text.Trim();
                string numeroEmpleado = txtNumeroEmpleadoModal.Text.Trim();
                string email = txtEmailModal.Text.Trim();

                var duplicados = new List<string>();

                if (db.tUsuario.Any(u => u.Curp == curp && u.Estatus == 1 && u.IdUsuario != id))
                    duplicados.Add("CURP");

                if (!string.IsNullOrEmpty(rfc) && db.tUsuario.Any(u => u.RfC == rfc && u.Estatus == 1 && u.IdUsuario != id))
                    duplicados.Add("RFC");

                if (db.tUsuario.Any(u => u.Usuario == usuarioNombre && u.Estatus == 1 && u.IdUsuario != id))
                    duplicados.Add("Usuario");

                if (!string.IsNullOrEmpty(numeroEmpleado) && db.tUsuario.Any(u => u.NumeroEmpleado == numeroEmpleado && u.Estatus == 1 && u.IdUsuario != id))
                    duplicados.Add("Número de Empleado");

                if (!string.IsNullOrEmpty(email) && db.tUsuario.Any(u => u.Email == email && u.Estatus == 1 && u.IdUsuario != id))
                    duplicados.Add("Correo Electrónico");

                if (duplicados.Count > 0)
                {
                    string campos = string.Join(", ", duplicados);
                    MostrarAlerta("error", "Datos duplicados",
                        $"Ya existe otro usuario registrado con el mismo: {campos}. Por favor verifica la información.");
                    return;
                }

                DateTime fechaNacimiento;
                if (!DateTime.TryParse(FechaNacimientoModal.Text.Trim(), out fechaNacimiento))
                {
                    MostrarAlerta("error", "Error", "La fecha de nacimiento no es válida.");
                    return;
                }

                DateTime fechaIngreso;
                if (!DateTime.TryParse(FechaIngreModal.Text.Trim(), out fechaIngreso))
                {
                    MostrarAlerta("error", "Error", "La fecha de ingreso no es válida.");
                    return;
                }

                usuarioReg.IdRol = Convert.ToInt32(ddlRolModal.SelectedValue);
                usuarioReg.IdPuesto = Convert.ToInt32(ddlPuestoModal.SelectedValue);
                usuarioReg.IdArea = Convert.ToInt32(ddlAreaModal.SelectedValue);
                usuarioReg.IdPlanta = ddlPlantaModal.SelectedValue != "0" ? (int?)Convert.ToInt32(ddlPlantaModal.SelectedValue) : null;
                usuarioReg.Nombre = txtNombreModal.Text.Trim();
                usuarioReg.ApellidoPaterno = txtApellidoPaternoModal.Text.Trim();
                usuarioReg.ApellidoMaterno = txtApellidoMaternoModal.Text.Trim();
                usuarioReg.Curp = curp;
                usuarioReg.RfC = rfc;
                usuarioReg.FechaNacimiento = fechaNacimiento;
                usuarioReg.FechaIngreso = fechaIngreso;
                usuarioReg.Genero = ddlGeneroModal.Text;
                usuarioReg.EstadoSocial = ddlEstadoSocialModal.Text;
                usuarioReg.Telefono = txtTelefonoModal.Text;
                usuarioReg.SeguroSocial = txtSeguroSocialModal.Text;
                usuarioReg.NumeroEmpleado = numeroEmpleado;
                usuarioReg.Email = email;
                usuarioReg.Direccion = txtDireccionModal.Text;
                usuarioReg.NombreFamilia = txtNombreFamiliaModal.Text;
                usuarioReg.TelefonoFamiliar = txtTelefonoFamiliarModal.Text;
                usuarioReg.Usuario = usuarioNombre;
                usuarioReg.Clave = txtClaveModal.Text;
                usuarioReg.Edad = Convert.ToInt32(txtEdadModal.Text);
                usuarioReg.Dispositivo1 = txtDispositivo1Modal.Text;
                usuarioReg.Mac1 = txtMac1Modal.Text;
                usuarioReg.Dispositivo2 = txtDispositivo2Modal.Text;
                usuarioReg.Mac2 = txtMac2Modal.Text;

                db.SubmitChanges();

                ScriptManager.RegisterStartupScript(this, GetType(),
                    Guid.NewGuid().ToString(),
                    "$('#modalEditar').modal('hide');", true);

                CargarUsuarioFiltrado(FiltroActual);
                MostrarAlerta("success", "Actualizado", "El usuario se actualizó correctamente.");
            }
            catch (Exception ex)
            {
                MostrarAlerta("error", "Error", "No se pudo actualizar: " + ex.Message);
            }
        }

        // ── Eliminar (cambio de estatus) ─────────────────────────────────────────
        protected void btnEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                int id = Convert.ToInt32(btn.CommandArgument);

                var medico = db.tUsuario.FirstOrDefault(t => t.IdUsuario == id);
                if (medico != null)
                {
                    medico.Estatus = 0;
                    db.SubmitChanges();
                    CargarUsuarioFiltrado(FiltroActual);
                    MostrarAlerta("success", "Inactivado", "El Usuario se marcó como inactivo correctamente.");
                }
                else
                {
                    MostrarAlerta("warning", "No encontrado", "No se encontró el Usuario seleccionado.");
                }
            }
            catch (Exception ex)
            {
                MostrarAlerta("error", "Error", "No se pudo actualizar el estado del Usuario: " + ex.Message);
            }
        }

        // ── Validación de campos ─────────────────────────────────────────────────
        private bool ValidarCampos()
        {
            if (ddlRol.SelectedValue == "0") { MostrarAlerta("warning", "Campo obligatorio", "Debe seleccionar un rol."); return false; }
            if (ddlPuesto.SelectedValue == "0") { MostrarAlerta("warning", "Campo obligatorio", "Debe seleccionar un puesto."); return false; }
            if (ddlArea.SelectedValue == "0") { MostrarAlerta("warning", "Campo obligatorio", "Debe seleccionar una área."); return false; }
            if (string.IsNullOrWhiteSpace(txtNombre.Text)) { MostrarAlerta("warning", "Campo obligatorio", "Debe ingresar un nombre."); return false; }
            if (string.IsNullOrWhiteSpace(txtApellidoPaterno.Text)) { MostrarAlerta("warning", "Campo obligatorio", "Debe ingresar el apellido paterno."); return false; }
            if (string.IsNullOrWhiteSpace(txtCurp.Text)) { MostrarAlerta("warning", "Campo obligatorio", "Debe ingresar el CURP."); return false; }
            if (string.IsNullOrWhiteSpace(txtEmail.Text)) { MostrarAlerta("warning", "Campo obligatorio", "Debe ingresar el correo."); return false; }
            if (string.IsNullOrWhiteSpace(txtUsuario.Text)) { MostrarAlerta("warning", "Campo obligatorio", "Debe ingresar el usuario de acceso."); return false; }
            if (string.IsNullOrWhiteSpace(txtClave.Text)) { MostrarAlerta("warning", "Campo obligatorio", "Debe ingresar la contraseña."); return false; }
            return true;
        }

        private int ConvertirEntero(string valor)
        {
            int resultado;
            return int.TryParse(valor, out resultado) ? resultado : 0;
        }

        private void LimpiarCampos()
        {
            ddlRol.SelectedIndex = 0;
            ddlPuesto.SelectedIndex = 0;
            ddlArea.SelectedIndex = 0;
            ddlGenero.SelectedIndex = 0;
            txtNombre.Text = "";
            txtApellidoPaterno.Text = "";
            txtApellidoMaterno.Text = "";
            txtEdad.Text = "";
            txtCurp.Text = "";
            txtRFC.Text = "";
            txtTelefono.Text = "";
            txtDireccion.Text = "";
            txtSeguroSocial.Text = "";
            txtTelefonoFamiliar.Text = "";
            txtDispositivo1.Text = "";
            txtUsuario.Text = "";
            txtClave.Text = "";
            txtEmail.Text = "";
            FechaNacimiento.Text = "";
            txtDispositivo2.Text = "";
            txtMac2.Text = "";
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