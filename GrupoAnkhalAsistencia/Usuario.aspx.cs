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
       


        protected void Page_Load(object sender, EventArgs e)
        {
            // ¿Sesion válida?
            if (SesionState.usuario == null)
            {
                SesionState.usuario = null;
                Response.Redirect("login.aspx");
                return;
            }


            string rolUsuario = SesionState.usuario.tRol.Rol;  // ajusta al nombre que tengas en tu clase

            // Aquí pones los roles que SI pueden entrar
            string[] rolesPermitidos = { "Administrador", "Rh" };

            if (!rolesPermitidos.Contains(rolUsuario))
            {
                // Si NO tiene rol válido → lo sacamos
                Response.Redirect("login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                CargarUsuario();
                CargarRol();
                CargarPuesto();
                CargarArea();
            }
        }



        private void CargarUsuario()
        {
            var usuario = from m in db.tUsuario
                          join r in db.tRol on m.IdRol equals r.IdRol
                          join p in db.tPuesto on m.IdPuesto equals p.IdPuesto
                          join a in db.tArea on m.IdArea equals a.IdArea
                          where m.Estatus == 1
                          orderby m.Nombre
                          select new
                          {
                              m.IdUsuario,
                              m.IdRol,
                              m.IdPuesto,
                              m.IdArea,
                              Rol = r.Rol,
                              Puesto = p.Puesto,
                              Area = a.Area,
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

            dvgUsuario.DataSource = usuario.ToList();
            dvgUsuario.DataBind();
        }

        private void CargarRol()
        {
            var rol = db.tRol
            .Select(r => new { r.IdRol, r.Rol }) // o los campos que necesites
            .ToList();

            // DropDown principal
            ddlRol.DataSource = rol;
            ddlRol.DataTextField = "Rol";
            ddlRol.DataValueField = "IdRol";
            ddlRol.DataBind();
            ddlRol.Items.Insert(0, new ListItem("-- Seleccione --", "0"));

            // DropDown del modal
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

            // DropDown principal
            ddlPuesto.DataSource = puesto;
            ddlPuesto.DataTextField = "Puesto";
            ddlPuesto.DataValueField = "IdPuesto";
            ddlPuesto.DataBind();
            ddlPuesto.Items.Insert(0, new ListItem("-- Seleccione --", "0"));

            // DropDown del modal
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

            // DropDown principal
            ddlArea.DataSource = area;
            ddlArea.DataTextField = "Area";
            ddlArea.DataValueField = "IdArea";
            ddlArea.DataBind();
            ddlArea.Items.Insert(0, new ListItem("-- Seleccione --", "0"));

            // DropDown del modal
            ddlAreaModal.DataSource = area;
            ddlAreaModal.DataTextField = "Area";
            ddlAreaModal.DataValueField = "IdArea";
            ddlAreaModal.DataBind();
            ddlAreaModal.Items.Insert(0, new ListItem("-- Seleccione --", "0"));
        }


        protected void btnEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                int id = Convert.ToInt32(btn.CommandArgument);

                var medico = db.tUsuario.FirstOrDefault(t => t.IdUsuario == id);
                if (medico != null)
                {
                    // 🔹 Cambiamos el estado en lugar de eliminar
                    medico.Estatus = 0;

                    db.SubmitChanges(); // Guardamos los cambios

                    CargarUsuario(); // Recarga la lista (asegúrate que filtre solo Estatus = 1)

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

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarCampos()) return;

            try
            {
                string curp = txtCurp.Text.Trim();
                string correo = txtEmail.Text.Trim();

                // Validar duplicado por CURP
                if (db.tUsuario.Any(u => u.Curp == curp && u.Estatus == 1))
                {
                    MostrarAlerta("error", "Duplicado", "Ya existe un Usuario con el mismo CURP.");
                    return;
                }

                // ✅ Validar fecha
                DateTime fechaNacimiento = Convert.ToDateTime(FechaNacimiento.Text.Trim());
                DateTime fechaIngreso = Convert.ToDateTime(FechaIngreso.Text.Trim());


                //validar foto 
                string base64 = hdFoto.Value;
                byte[] fotoBytes = null;

                // Validar que sí hay foto
                if (string.IsNullOrEmpty(base64))
                {
                    MostrarAlerta("warning", "Foto requerida", "Debes tomar una foto antes de guardar.");
                    return;
                }

                // Limpiar encabezado del base64
                base64 = base64.Replace("data:image/png;base64,", "")
                               .Replace("data:image/jpeg;base64,", "")
                               .Replace("data:image/jpg;base64,", "");

                // Convertir Base64 → Bytes
                fotoBytes = Convert.FromBase64String(base64);




                tUsuario nuevo = new tUsuario
                {
                    IdRol = Convert.ToInt32(ddlRol.SelectedValue),
                    IdPuesto = Convert.ToInt32(ddlPuesto.SelectedValue),
                    IdArea = Convert.ToInt32(ddlArea.SelectedValue),
                    Nombre = txtNombre.Text.Trim(),
                    ApellidoPaterno = txtApellidoPaterno.Text.Trim(),
                    ApellidoMaterno = txtApellidoMaterno.Text.Trim(),
                    Curp = curp,
                    RfC = txtRFC.Text.Trim(),
                    FechaNacimiento = fechaNacimiento,
                    FechaIngreso = fechaIngreso,
                    Genero = ddlGenero.Text,
                    EstadoSocial=EstadoSocial.Text,
                    Telefono = txtTelefono.Text.Trim(),
                    SeguroSocial=txtSeguroSocial.Text.Trim(),
                    NumeroEmpleado= txtNumeroEmpleado.Text,
                    Email=txtEmail.Text.Trim(),
                    Direccion = txtDireccion.Text.Trim(),
                    NombreFamilia=txtNombreFamilia.Text.Trim(),
                    TelefonoFamiliar=txtTelefonoFamiliar.Text.Trim(),
                    Usuario = txtUsuario.Text,
                    Clave = txtClave.Text,
                    Edad = ConvertirEntero(txtEdad.Text),
                    Dispositivo1=txtDispositivo1.Text.Trim(),
                    Mac1 = txtMac1.Text.Trim(),
                    Dispositivo2 = txtDispositivo2.Text.Trim(),
                    Mac2 = txtMac2.Text.Trim(),
                    Foto = fotoBytes,
                    Estatus = 1
                };

                db.tUsuario.InsertOnSubmit(nuevo);
                db.SubmitChanges();

                // Enviar correo con credenciales
                //bool correoEnviado = EnviarCorreo(usuario, clave, correo);

                LimpiarCampos();
                CargarUsuario();

                //if (correoEnviado)
                //    MostrarAlerta("success", "Guardado", "El Usuario fue guardado y se enviaron las credenciales al correo.");
                //else
                //    MostrarAlerta("warning", "Guardado", "El Usuario fue guardado, pero no se pudo enviar el correo.");
            }
            catch (Exception ex)
            {
                MostrarAlerta("error", "Error", "No se pudo guardar el Usuario: " + ex.Message);
            }
        }


        //private string GenerarClaveAleatoria(int longitud)
        //{
        //    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        //    Random random = new Random();
        //    return new string(Enumerable.Repeat(chars, longitud)
        //        .Select(s => s[random.Next(s.Length)]).ToArray());
        //}


        //private bool EnviarCorreo(string usuario, string clave, string correo)
        //{
        //    try
        //    {
        //        string asunto = "Credenciales de acceso - Medens";
        //        string cuerpo = $@"
        //<h3>Bienvenido a Medens</h3>
        //<p>Sus credenciales de acceso son:</p>
        //<ul>
        //    <li><strong>Usuario:</strong> {usuario}</li>
        //    <li><strong>Contraseña:</strong> {clave}</li>
        //</ul>
        //<br/>
        //<p>Equipo Medens</p>";

        //        MailMessage mail = new MailMessage();
        //        mail.To.Add(correo);
        //        mail.From = new MailAddress("sistemas@mdens.com.mx", "Medens Soporte");
        //        mail.Subject = asunto;
        //        mail.Body = cuerpo;
        //        mail.IsBodyHtml = true;

        //        // Configuración del servidor SMTP
        //        SmtpClient smtp = new SmtpClient("smtp.medens.com.mx");
        //        smtp.Port = 587; // o 465 si usas SSL
        //        smtp.Credentials = new System.Net.NetworkCredential("sistemas@mdens.com.mx", "Javg1892030815#");
        //        smtp.EnableSsl = true;

        //        smtp.Send(mail);
        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}

        private bool ValidarCampos()
        {

            if (ddlRol.SelectedValue == "0")
            {
                MostrarAlerta("warning", "Campo obligatorio", "Debe seleccionar un rol.");
                return false;
            }
            
            if (ddlPuesto.SelectedValue == "0")
            {
                MostrarAlerta("warning", "Campo obligatorio", "Debe seleccionar un puesto.");
                return false;
            }
            if (ddlArea.SelectedValue == "0")
            {
                MostrarAlerta("warning", "Campo obligatorio", "Debe seleccionar una área.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MostrarAlerta("warning", "Campo obligatorio", "Debe ingresar un nombre.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtApellidoPaterno.Text))
            {
                MostrarAlerta("warning", "Campo obligatorio", "Debe ingresar el apellido paterno.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtCurp.Text))
            {
                MostrarAlerta("warning", "Campo obligatorio", "Debe ingresar el CURP.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MostrarAlerta("warning", "Campo obligatorio", "Debe ingresar el correo.");
                return false;
            }
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
            txtEdad.Text = "";
            txtDireccion.Text = "";
            txtSeguroSocial.Text = "";
            txtTelefonoFamiliar.Text = "";
            txtTelefono.Text = "";
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


        protected void btnGuardarModal_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(hfIdUsuario.Value))
            {
                MostrarAlerta("error", "Error", "No se encontró el ID del médico.");
                return;
            }

            // ✅ Validar fecha
            DateTime fechaNacimiento;
            if (!DateTime.TryParse(FechaNacimientoModal.Text.Trim(), out fechaNacimiento))
            {
                MostrarAlerta("error", "Error", "La fecha de nacimiento no es válida.");
                return;
            }

            // ✅ Validar fecha
            DateTime fechaIngreso;
            if (!DateTime.TryParse(FechaIngreModal.Text.Trim(), out fechaIngreso))
            {
                MostrarAlerta("error", "Error", "La fecha de ingreso no es válida.");
                return;
            }


            int id = Convert.ToInt32(hfIdUsuario.Value);

            try
            {
                var medico = db.tUsuario.FirstOrDefault(t => t.IdUsuario == id);
                if (medico == null)
                {
                    MostrarAlerta("error", "Error", "El médico no existe.");
                    return;
                }

                // Actualizar campos
                medico.IdRol = Convert.ToInt32(ddlRolModal.SelectedValue);
                medico.IdPuesto = Convert.ToInt32(ddlPuestoModal.SelectedValue);
                medico.IdArea = Convert.ToInt32(ddlAreaModal.SelectedValue);
                medico.Nombre = txtNombreModal.Text.Trim();
                medico.ApellidoPaterno = txtApellidoPaternoModal.Text.Trim();
                medico.ApellidoMaterno = txtApellidoMaternoModal.Text.Trim();
                medico.Curp = txtCurpModal.Text.Trim();
                medico.RfC = txtRfcModal.Text.Trim();
                medico.FechaNacimiento = fechaNacimiento;
                medico.FechaIngreso = fechaIngreso;
                medico.Genero=ddlGeneroModal.Text;
                medico.EstadoSocial=ddlEstadoSocialModal.Text;
                medico.Telefono=txtTelefonoModal.Text;
                medico.SeguroSocial=txtSeguroSocialModal.Text;
                medico.NumeroEmpleado=txtNumeroEmpleadoModal.Text.Trim();
                medico.Email=txtEmailModal.Text;
                medico.Direccion=txtDireccionModal.Text;
                medico.NombreFamilia=txtNombreFamiliaModal.Text;
                medico.TelefonoFamiliar=txtTelefonoFamiliarModal.Text;
                medico.Usuario=txtUsuarioModal.Text;
                medico.Clave=txtClaveModal.Text;
                medico.Edad=Convert.ToInt32(txtEdadModal.Text);
                medico.Dispositivo1= txtDispositivo1Modal.Text;
                medico.Mac1=txtMac1Modal.Text;
                medico.Dispositivo2=txtDispositivo2Modal.Text;
                medico.Mac2=txtMac2Modal.Text;
                db.SubmitChanges();
                CargarUsuario();

                ScriptManager.RegisterStartupScript(this, GetType(),
                    Guid.NewGuid().ToString(),
                    "$('#modalEditar').modal('hide');", true);

                MostrarAlerta("success", "Actualizado", "El usuario se actualizó correctamente.");
            }
            catch (Exception ex)
            {
                MostrarAlerta("error", "Error", "No se pudo actualizar: " + ex.Message);
            }
        }

        private void CargarMedicos(string filtro = "")
        {
            var query = from t in db.tUsuario
                        join r in db.tRol on t.IdRol equals r.IdRol
                        join p in db.tPuesto on t.IdPuesto equals p.IdPuesto
                        join a in db.tArea on t.IdArea equals a.IdArea
                        where t.IdRol == 2 && t.Estatus == 1
                        select new
                        {
                            t.IdUsuario,
                            t.IdRol,
                            t.IdPuesto,
                            t.IdArea,
                            Rol = r.Rol,
                            Puesto = p.Puesto,
                            Area = a.Area,
                            t.Nombre,
                            t.ApellidoPaterno,
                            t.ApellidoMaterno,
                            t.Curp,
                            t.RfC,
                            t.FechaNacimiento,
                            t.FechaIngreso,
                            t.Genero,
                            t.EstadoSocial,
                            t.Telefono,
                            t.SeguroSocial,
                            t.NumeroEmpleado,
                            t.Email,
                            t.Direccion,
                            t.NombreFamilia,
                            t.TelefonoFamiliar,
                            t.Usuario,
                            t.Clave,
                            t.Edad,
                            t.Dispositivo1,
                            t.Mac1,
                            t.Dispositivo2,
                            t.Mac2
                        };

            if (!string.IsNullOrEmpty(filtro))
            {
                filtro = filtro.Trim();

                query = query.Where(x =>
                    System.Data.Linq.SqlClient.SqlMethods.Like(x.Curp, "%" + filtro + "%") ||
                    System.Data.Linq.SqlClient.SqlMethods.Like(x.Nombre, "%" + filtro + "%") ||
                    System.Data.Linq.SqlClient.SqlMethods.Like(x.ApellidoPaterno, "%" + filtro + "%") ||
                    System.Data.Linq.SqlClient.SqlMethods.Like(x.ApellidoMaterno, "%" + filtro + "%")
                );
            }

            dvgUsuario.DataSource = query.ToList();
            dvgUsuario.DataBind();
        }



        protected void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            CargarMedicos(txtBuscar.Text.Trim());
        }

        protected void dvgUsuario_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dvgUsuario.PageIndex = e.NewPageIndex;
            CargarUsuario();
        }
    }
}