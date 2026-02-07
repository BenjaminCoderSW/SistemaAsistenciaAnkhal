<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Usuario.aspx.cs" Inherits="GrupoAnkhalAsistencia.Usuario" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
          <link href="css/gridviewPantalla.css" rel="stylesheet" />
<script src="scriptspropios/sweetalert2@11.js"></script>
<script src="scriptspropios/propios.js"></script>
<!-- Librería para generar códigos QR en el navegador -->
<script src="https://cdnjs.cloudflare.com/ajax/libs/qrcodejs/1.0.0/qrcode.min.js"></script>

<style>
    .credencial-card {
        width: 312px;
        height: 200px;
        border-radius: -7px;
        border: 3px solid #ff6a00;
        background: #ffffff;
        overflow: hidden;
        box-shadow: 0 8px 18px rgba(0, 0, 0, 0.25);
        font-family: 'Segoe UI', Arial, sans-serif;
    }

    .credencial-header {
        background: #111111; /* negro */
        color: #ffffff;
        padding: 6px 10px;
        display: flex;
        align-items: center;
    }

    .credencial-header-bar {
        width: 8px;
        height: 32px;
        border-radius: 4px;
        background: #ff6a00;
    }

    .credencial-logo-text {
        margin-left: 10px;
    }

    .credencial-logo-text .empresa {
        font-weight: 700;
        font-size: 15px;
        letter-spacing: 0.5px;
        color: #ff6a00; /* naranja */
    }

    .credencial-logo-text .subtitulo {
        font-size: 11px;
        color: #ffffff;
        opacity: 0.9;
    }

    .credencial-body {
    padding: 3px 2px;
    font-size: 14px;
    color: #111111;
}

    .credencial-body h5 {
        font-size: 15px;
        font-weight: 600;
        margin-bottom: 4px;
    }

    .credencial-datos {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-top: 6px;
    }

    .credencial-datos p {
        margin-bottom: 2px;
    }

    .credencial-footer {
        background: #f5f5f5;
        border-top: 1px solid #e0e0e0;
        padding: 6px 10px;
        font-size: 11px;
        text-align: center;
        color: #555555;
    }
</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
              <h2>Gesti&oacute;n de Usuarios</h2>

    <!-- Botón Agregar -->
    <asp:Button ID="btnAgregar" runat="server" Text="Agregar Nuevo" CssClass="btn btn-primary mb-3"  OnClientClick="abrirModal(); return false;" />

    <!-- Grid de datos -->
  <br />

<div class="table-responsive">
    <div class="col-md-6"> 
        <asp:TextBox ID="txtBuscar" runat="server" 
   CssClass="form-control" 
   Placeholder="Buscar Usuario..."
   AutoPostBack="true"
   OnTextChanged="txtBuscar_TextChanged" />
    </div>
    <br />
<asp:GridView ID="dvgUsuario" runat="server" AutoGenerateColumns="False"
    CssClass="table table-bordered table-striped custom-grid"
    AllowPaging="True" PageSize="5"
    OnPageIndexChanging="dvgUsuario_PageIndexChanging">
    <Columns>
        <asp:BoundField DataField="Rol" HeaderText="Rol" />
        <asp:BoundField DataField="Area" HeaderText="Area" />
        <asp:BoundField DataField="Planta" HeaderText="Planta" />
        <asp:BoundField DataField="Puesto" HeaderText="Puesto" />
                <asp:TemplateField HeaderText="Nombre">
            <ItemTemplate>
                <span class="truncate-text" title='<%# Eval("Nombre") %>'>
                    <%# Eval("Nombre") %>
                </span>
            </ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField DataField="ApellidoPaterno" HeaderText="ApellidoPaterno" />
        <asp:BoundField DataField="ApellidoMaterno" HeaderText="ApellidoMaterno" />
        <asp:BoundField DataField="Curp" HeaderText="Curp" />
        <asp:BoundField DataField="RFC" HeaderText="RFC" />
        <asp:BoundField DataField="FechaNacimiento" HeaderText="FechaNacimiento" />
        <asp:BoundField DataField="FechaIngreso" HeaderText="FechaIngreso" />
        <asp:BoundField DataField="Genero" HeaderText="Genero" />
        <asp:BoundField DataField="EstadoSocial" HeaderText="EstadoSocial" />
        <asp:BoundField DataField="Telefono" HeaderText="Telefono" />
          <asp:BoundField DataField="SeguroSocial" HeaderText="SeguroSocial" />
          <asp:BoundField DataField="NumeroEmpleado" HeaderText="NumeroEmpleado" />
        <asp:BoundField DataField="Email" HeaderText="Email" />
      <asp:TemplateField HeaderText="Dirección">
    <ItemTemplate>
        <span class="truncate-text" title='<%# Eval("Direccion") %>'>
            <%# Eval("Direccion") %>
        </span>
    </ItemTemplate>
</asp:TemplateField>
        <asp:BoundField DataField="NombreFamilia" HeaderText="NombreFamilia" />
        <asp:BoundField DataField="TelefonoFamiliar" HeaderText="TelefonoFamiliar" />
        <asp:BoundField DataField="Usuario" HeaderText="Usuario" />
         <asp:BoundField DataField="Clave" HeaderText="Clave" />
                <asp:BoundField DataField="Edad" HeaderText="Edad" />
        <asp:BoundField DataField="Dispositivo1" HeaderText="Dispositivo1" />
        <asp:BoundField DataField="Mac1" HeaderText="Mac1" />
        <asp:BoundField DataField="Dispositivo2" HeaderText="Dispositivo2" />
        <asp:BoundField DataField="Mac2" HeaderText="Mac2" />
     <asp:TemplateField HeaderText="Acciones">
      <ItemTemplate>
           <button type="button" class="btn btn-info btn-sm"
                   onclick="abrirModalQr(
                       '<%# Eval("Nombre") %>',
                       '<%# Eval("ApellidoPaterno") %>',
                       '<%# Eval("ApellidoMaterno") %>',
                       '<%# Eval("NumeroEmpleado") %>',
                       '<%# Eval("Area") %>'
                   )">
               QR
           </button>
           &nbsp;
           <button type="button" class="btn btn-warning btn-sm"
         onclick="abrirModalEditar('<%# Eval("IdUsuario") %>',
 '<%# Eval("IdRol") %>',
 '<%# Eval("IdArea") %>',
 '<%# Eval("IdPuesto") %>',
 '<%# Eval("IdPlanta") ?? "0" %>',
 '<%# Eval("Nombre") %>',
 '<%# Eval("ApellidoPaterno") %>',
 '<%# Eval("ApellidoMaterno") %>',
 '<%# Eval("Curp") %>',
 '<%# Eval("RfC") %>',
 '<%# Eval("FechaNacimiento") %>',
 '<%# Eval("FechaIngreso") %>',
 '<%# Eval("Genero") %>',
 '<%# Eval("EstadoSocial") %>',
 '<%# Eval("Telefono") %>',
 '<%# Eval("SeguroSocial") %>',
 '<%# Eval("NumeroEmpleado") %>',
 '<%# Eval("Email") %>',
 '<%# Eval("Direccion") %>',
 '<%# Eval("NombreFamilia") %>',
 '<%# Eval("TelefonoFamiliar") %>',
 '<%# Eval("Usuario") %>',
 '<%# Eval("Clave") %>',
 '<%# Eval("Edad") %>',
 '<%# Eval("Dispositivo1") %>',
 '<%# Eval("Mac1") %>',
 '<%# Eval("Dispositivo2") %>',
 '<%# Eval("Mac2") %>')">
     Editar
 </button>

          <asp:Button ID="btnEliminar" runat="server" Text="Eliminar" CssClass="btn btn-danger btn-sm"
                      CommandArgument='<%# Eval("IdUsuario") %>' OnClick="btnEliminar_Click" />
      </ItemTemplate>
  </asp:TemplateField>
    </Columns>
</asp:GridView>
    </div>


    <!-- Formulario para agregar/editar -->
     <!-- Modal Bootstrap -->
    <div class="modal fade" id="modalAgregar" tabindex="-1" role="dialog" aria-labelledby="modalAgregarLabel" aria-hidden="true">
  <div class="modal-dialog modal-xl" role="document">
    <div class="modal-content shadow-lg border-0 rounded-3">
      <div class="modal-header text-white" style="background-color: #003366;">
        <h5 class="modal-title" id="modalAgregarLabel">Agregar Usuario</h5>
        <button type="button" class="close text-white" data-dismiss="modal" aria-label="Cerrar">
          <span aria-hidden="true">&times;</span>
        </button>
      </div>

      <div class="modal-body">
        <div class="container-fluid">
          <div class="row">
            
            <!-- Primera sección -->
            <div class="col-md-6 mb-3">
              <label for="ddlRol">Rol</label>
              <asp:DropDownList ID="ddlRol" runat="server" CssClass="form-control"></asp:DropDownList>
            </div>
            <div class="col-md-6 mb-3">
              <label for="ddlArea">Área</label>
              <asp:DropDownList ID="ddlArea" runat="server" CssClass="form-control"></asp:DropDownList>
            </div>
            <div class="col-md-6 mb-3">
              <label for="ddlPlanta">Planta</label>
              <asp:DropDownList ID="ddlPlanta" runat="server" CssClass="form-control"></asp:DropDownList>
            </div>
            <div class="col-md-6 mb-3">
              <label for="ddlPuesto">Puesto</label>
              <asp:DropDownList ID="ddlPuesto" runat="server" CssClass="form-control"></asp:DropDownList>
            </div>

            <!-- Datos personales -->
            <div class="col-12"><hr><h6 class="text-primary font-weight-bold">Datos Personales</h6></div>

            <div class="col-md-6 mb-3">
              <label>Nombre</label>
              <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" Placeholder="Nombre"></asp:TextBox>
            </div>
            <div class="col-md-6 mb-3">
              <label>Apellido Paterno</label>
              <asp:TextBox ID="txtApellidoPaterno" runat="server" CssClass="form-control" Placeholder="Apellido Paterno"></asp:TextBox>
            </div>
            <div class="col-md-6 mb-3">
              <label>Apellido Materno</label>
              <asp:TextBox ID="txtApellidoMaterno" runat="server" CssClass="form-control" Placeholder="Apellido Materno"></asp:TextBox>
            </div>

            <div class="col-md-6 mb-3">
              <label>CURP</label>
              <asp:TextBox ID="txtCurp" runat="server" CssClass="form-control" Placeholder="CURP"></asp:TextBox>
            </div>
            <div class="col-md-6 mb-3">
              <label>RFC</label>
              <asp:TextBox ID="txtRFC" runat="server" CssClass="form-control" Placeholder="RFC"></asp:TextBox>
            </div>

            <div class="col-md-6 mb-3">
              <label>Fecha de Nacimiento</label>
              <asp:TextBox ID="FechaNacimiento" runat="server" CssClass="form-control" placeholder="Seleccione fecha"></asp:TextBox>
              <ajaxToolkit:CalendarExtender ID="CalendarExtender2" runat="server" TargetControlID="FechaNacimiento" Format="dd/MM/yyyy"></ajaxToolkit:CalendarExtender>
            </div>

            <div class="col-md-6 mb-3">
              <label>Fecha de Ingreso</label>
              <asp:TextBox ID="FechaIngreso" runat="server" CssClass="form-control" placeholder="Seleccione fecha"></asp:TextBox>
              <ajaxToolkit:CalendarExtender ID="CalendarExtender3" runat="server" TargetControlID="FechaIngreso" Format="dd/MM/yyyy"></ajaxToolkit:CalendarExtender>
            </div>

            <div class="col-md-6 mb-3">
              <label>Género</label>
              <asp:DropDownList ID="ddlGenero" runat="server" CssClass="form-control">
                <asp:ListItem Text="Seleccione..." Value="" />
                <asp:ListItem Text="Masculino" Value="Masculino" />
                <asp:ListItem Text="Femenino" Value="Femenino" />
                <asp:ListItem Text="Indefinido" Value="Indefinido" />
              </asp:DropDownList>
            </div>

            <div class="col-md-6 mb-3">
              <label>Estado Civil</label>
              <asp:DropDownList ID="EstadoSocial" runat="server" CssClass="form-control">
                <asp:ListItem Text="Seleccione..." Value="" />
                <asp:ListItem Text="Casad@" Value="Casad@" />
                <asp:ListItem Text="Solter@" Value="Solter@" />
                <asp:ListItem Text="Unión Libre" Value="Union Libre" />
              </asp:DropDownList>
            </div>

            <!-- Contacto -->
            <div class="col-12"><hr><h6 class="text-primary font-weight-bold">Datos de Contacto</h6></div>

            <div class="col-md-6 mb-3">
              <label>Teléfono</label>
              <asp:TextBox ID="txtTelefono" runat="server" CssClass="form-control" Placeholder="Teléfono"></asp:TextBox>
            </div>
            <div class="col-md-6 mb-3">
              <label>Correo Electrónico</label>
              <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" Placeholder="Email"></asp:TextBox>
            </div>
            <div class="col-md-6 mb-3">
              <label>Dirección</label>
              <asp:TextBox ID="txtDireccion" runat="server" CssClass="form-control" Placeholder="Dirección"></asp:TextBox>
            </div>

            <!-- Familia -->
            <div class="col-12"><hr><h6 class="text-primary font-weight-bold">Contacto Familiar</h6></div>

            <div class="col-md-6 mb-3">
              <label>Nombre Familiar</label>
              <asp:TextBox ID="txtNombreFamilia" runat="server" CssClass="form-control" Placeholder="Nombre Familiar"></asp:TextBox>
            </div>
            <div class="col-md-6 mb-3">
              <label>Teléfono Familiar</label>
              <asp:TextBox ID="txtTelefonoFamiliar" runat="server" CssClass="form-control" Placeholder="Teléfono Familiar"></asp:TextBox>
            </div>

            <!-- Información laboral -->
            <div class="col-12"><hr><h6 class="text-primary font-weight-bold">Datos Laborales</h6></div>

            <div class="col-md-6 mb-3">
              <label>Seguro Social</label>
              <asp:TextBox ID="txtSeguroSocial" runat="server" CssClass="form-control" Placeholder="Seguro Social"></asp:TextBox>
            </div>
            <div class="col-md-6 mb-3">
              <label>Número de Empleado</label>
              <asp:TextBox ID="txtNumeroEmpleado" runat="server" CssClass="form-control" Placeholder="Número de Empleado"></asp:TextBox>
            </div>
            <div class="col-md-6 mb-3">
              <label>Edad</label>
              <asp:TextBox ID="txtEdad" runat="server" CssClass="form-control" Placeholder="Edad"></asp:TextBox>
            </div>
            <div class="col-md-6 mb-3">
              <label>Usuario</label>
              <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control" Placeholder="Usuario"></asp:TextBox>
            </div>
            <div class="col-md-6 mb-3">
              <label>Clave</label>
              <asp:TextBox ID="txtClave" runat="server" CssClass="form-control" Placeholder="Clave"></asp:TextBox>
            </div>
            <!-- Dispositivos -->
            <div class="col-12"><hr><h6 class="text-primary font-weight-bold">Dispositivos</h6></div>

            <div class="col-md-6 mb-3">
              <label>Dispositivo 1</label>
              <asp:TextBox ID="txtDispositivo1" runat="server" CssClass="form-control" Placeholder="Dispositivo 1"></asp:TextBox>
            </div>
            <div class="col-md-6 mb-3">
              <label>MAC Dispositivo 1</label>
              <asp:TextBox ID="txtMac1" runat="server" CssClass="form-control" Placeholder="MAC 1"></asp:TextBox>
            </div>
            <div class="col-md-6 mb-3">
              <label>Dispositivo 2</label>
              <asp:TextBox ID="txtDispositivo2" runat="server" CssClass="form-control" Placeholder="Dispositivo 2"></asp:TextBox>
            </div>
            <div class="col-md-6 mb-3">
              <label>MAC Dispositivo 2</label>
              <asp:TextBox ID="txtMac2" runat="server" CssClass="form-control" Placeholder="MAC 2"></asp:TextBox>
            </div>

              <div class="col-12">
                   <video id="videoCamara" width="250" height="200" autoplay></video><br>
                    <button type="button" class="btn btn-primary" onclick="tomarFoto()">Tomar Foto</button>

                    <canvas id="canvasFoto" width="250" height="200" style="display:none;"></canvas>
                  <asp:HiddenField ID="hdFoto" runat="server" />
              </div>

          </div>
        </div>
      </div>

      <div class="modal-footer bg-light">
        <asp:Button ID="btnGuardar" runat="server" Text="Guardar" CssClass="btn btn-success px-4" OnClick="btnGuardar_Click" />
        <button type="button" class="btn btn-secondary px-4" data-dismiss="modal">Cancelar</button>
      </div>
    </div>
  </div>
</div>


<div class="modal fade" id="modalEditar" tabindex="-1" role="dialog" aria-labelledby="modalEditarLabel" aria-hidden="true">
  <div class="modal-dialog modal-xl" role="document">
    <div class="modal-content">
      
      <!-- Header -->
      <div class="modal-header text-white" style="background-color: #003366;">
        <h5 class="modal-title" id="modalEditarLabel">Editar Usuario</h5>
        <button type="button" class="close text-white" data-dismiss="modal" aria-label="Cerrar">
          <span aria-hidden="true">&times;</span>
        </button>
      </div>

      <!-- Body -->
      <div class="modal-body">
        <asp:HiddenField ID="hfIdUsuario" runat="server" />

        <div class="form-row">
          <!-- Rol -->
          <div class="form-group col-md-6">
            <label for="ddlRolModal">Rol</label>
            <asp:DropDownList ID="ddlRolModal" runat="server" CssClass="form-control"></asp:DropDownList>
          </div>

          <!-- Área -->
          <div class="form-group col-md-6">
            <label for="ddlAreaModal">Área</label>
            <asp:DropDownList ID="ddlAreaModal" runat="server" CssClass="form-control"></asp:DropDownList>
          </div>

          <!-- Planta -->
          <div class="form-group col-md-6">
            <label for="ddlPlantaModal">Planta</label>
            <asp:DropDownList ID="ddlPlantaModal" runat="server" CssClass="form-control"></asp:DropDownList>
          </div>

          <!-- Puesto -->
          <div class="form-group col-md-6">
            <label for="ddlPuestoModal">Puesto</label>
            <asp:DropDownList ID="ddlPuestoModal" runat="server" CssClass="form-control"></asp:DropDownList>
          </div>

          <!-- Nombre -->
          <div class="form-group col-md-6">
            <label for="txtNombreModal">Nombre</label>
            <asp:TextBox ID="txtNombreModal" runat="server" CssClass="form-control" Placeholder="Nombre"></asp:TextBox>
          </div>

          <!-- Apellido Paterno -->
          <div class="form-group col-md-6">
            <label for="txtApellidoPaternoModal">Apellido Paterno</label>
            <asp:TextBox ID="txtApellidoPaternoModal" runat="server" CssClass="form-control" Placeholder="Apellido Paterno"></asp:TextBox>
          </div>

          <!-- Apellido Materno -->
          <div class="form-group col-md-6">
            <label for="txtApellidoMaternoModal">Apellido Materno</label>
            <asp:TextBox ID="txtApellidoMaternoModal" runat="server" CssClass="form-control" Placeholder="Apellido Materno"></asp:TextBox>
          </div>

          <!-- CURP -->
          <div class="form-group col-md-6">
            <label for="txtCurpModal">CURP</label>
            <asp:TextBox ID="txtCurpModal" runat="server" CssClass="form-control" Placeholder="CURP"></asp:TextBox>
          </div>

          <!-- RFC -->
          <div class="form-group col-md-6">
            <label for="txtRfcModal">RFC</label>
            <asp:TextBox ID="txtRfcModal" runat="server" CssClass="form-control" Placeholder="RFC"></asp:TextBox>
          </div>

          <!-- Fecha Nacimiento -->
          <div class="form-group col-md-6">
            <label for="FechaNacimientoModal">Fecha de Nacimiento</label>
            <asp:TextBox ID="FechaNacimientoModal" runat="server" CssClass="form-control"
              placeholder="Seleccione fecha" ToolTip="Seleccione la fecha de Nacimiento"></asp:TextBox>
            <ajaxToolkit:CalendarExtender 
              ID="CalendarExtender1" runat="server" 
              TargetControlID="FechaNacimientoModal" Format="dd/MM/yyyy" />
          </div>

          <!-- Fecha Ingreso -->
          <div class="form-group col-md-6">
            <label for="FechaIngreModal">Fecha de Ingreso</label>
            <asp:TextBox ID="FechaIngreModal" runat="server" CssClass="form-control"
              placeholder="Seleccione fecha" ToolTip="Seleccione la fecha de Ingreso"></asp:TextBox>
            <ajaxToolkit:CalendarExtender 
              ID="CalendarExtender4" runat="server" 
              TargetControlID="FechaIngreModal" Format="dd/MM/yyyy" />
          </div>

          <!-- Género -->
          <div class="form-group col-md-6">
            <label for="ddlGeneroModal">Género</label>
            <asp:DropDownList ID="ddlGeneroModal" runat="server" CssClass="form-control">
              <asp:ListItem Text="Seleccione..." Value="" />
              <asp:ListItem Text="Masculino" Value="Masculino" />
              <asp:ListItem Text="Femenino" Value="Femenino" />
              <asp:ListItem Text="Indefinido" Value="Indefinido" />
            </asp:DropDownList>
          </div>

          <!-- Estado Social -->
          <div class="form-group col-md-6">
            <label for="ddlEstadoSocialModal">Estado Social</label>
            <asp:DropDownList ID="ddlEstadoSocialModal" runat="server" CssClass="form-control">
              <asp:ListItem Text="Seleccione..." Value="" />
              <asp:ListItem Text="Casad@" Value="Casad@" />
              <asp:ListItem Text="Solter@" Value="Solter@" />
              <asp:ListItem Text="Unión Libre" Value="Unión Libre" />
            </asp:DropDownList>
          </div>

          <!-- Teléfono -->
          <div class="form-group col-md-6">
            <label for="txtTelefonoModal">Teléfono</label>
            <asp:TextBox ID="txtTelefonoModal" runat="server" CssClass="form-control" Placeholder="Teléfono"></asp:TextBox>
          </div>

          <!-- Seguro Social -->
          <div class="form-group col-md-6">
            <label for="txtSeguroSocialModal">Seguro Social</label>
            <asp:TextBox ID="txtSeguroSocialModal" runat="server" CssClass="form-control" Placeholder="Seguro Social"></asp:TextBox>
          </div>

          <!-- Número Empleado -->
          <div class="form-group col-md-6">
            <label for="txtNumeroEmpleadoModal">Número Empleado</label>
            <asp:TextBox ID="txtNumeroEmpleadoModal" runat="server" CssClass="form-control" Placeholder="Número Empleado"></asp:TextBox>
          </div>

          <!-- Email -->
          <div class="form-group col-md-6">
            <label for="txtEmailModal">Email</label>
            <asp:TextBox ID="txtEmailModal" runat="server" CssClass="form-control" Placeholder="Email"></asp:TextBox>
          </div>

          <!-- Dirección -->
          <div class="form-group col-md-6">
            <label for="txtDireccionModal">Dirección</label>
            <asp:TextBox ID="txtDireccionModal" runat="server" CssClass="form-control" Placeholder="Dirección"></asp:TextBox>
          </div>

          <!-- Nombre Familia -->
          <div class="form-group col-md-6">
            <label for="txtNombreFamiliaModal">Nombre Familia</label>
            <asp:TextBox ID="txtNombreFamiliaModal" runat="server" CssClass="form-control" Placeholder="Nombre Familia"></asp:TextBox>
          </div>

          <!-- Teléfono Familiar -->
          <div class="form-group col-md-6">
            <label for="txtTelefonoFamiliarModal">Teléfono Familiar</label>
            <asp:TextBox ID="txtTelefonoFamiliarModal" runat="server" CssClass="form-control" Placeholder="Teléfono Familiar"></asp:TextBox>
          </div>

          <!-- Usuario -->
          <div class="form-group col-md-6">
            <label for="txtUsuarioModal">Usuario</label>
            <asp:TextBox ID="txtUsuarioModal" runat="server" CssClass="form-control" Placeholder="Usuario"></asp:TextBox>
          </div>

          <!-- Clave -->
          <div class="form-group col-md-6">
            <label for="txtClaveModal">Clave</label>
            <asp:TextBox ID="txtClaveModal" runat="server" CssClass="form-control" Placeholder="Clave"></asp:TextBox>
          </div>

          <!-- Edad -->
          <div class="form-group col-md-6">
            <label for="txtEdadModal">Edad</label>
            <asp:TextBox ID="txtEdadModal" runat="server" CssClass="form-control" Placeholder="Edad"></asp:TextBox>
          </div>

          <!-- Dispositivo 1 -->
          <div class="form-group col-md-6">
            <label for="txtDispositivo1Modal">Dispositivo 1</label>
            <asp:TextBox ID="txtDispositivo1Modal" runat="server" CssClass="form-control" Placeholder="Dispositivo 1"></asp:TextBox>
          </div>

          <!-- MAC 1 -->
          <div class="form-group col-md-6">
            <label for="txtMac1Modal">MAC Dispositivo 1</label>
            <asp:TextBox ID="txtMac1Modal" runat="server" CssClass="form-control" Placeholder="MAC 1"></asp:TextBox>
          </div>

          <!-- Dispositivo 2 -->
          <div class="form-group col-md-6">
            <label for="txtDispositivo2Modal">Dispositivo 2</label>
            <asp:TextBox ID="txtDispositivo2Modal" runat="server" CssClass="form-control" Placeholder="Dispositivo 2"></asp:TextBox>
          </div>

          <!-- MAC 2 -->
          <div class="form-group col-md-6">
            <label for="txtMac2Modal">MAC 2</label>
            <asp:TextBox ID="txtMac2Modal" runat="server" CssClass="form-control" Placeholder="MAC 2"></asp:TextBox>
          </div>
        </div>
      </div>

      <!-- Footer -->
      <div class="modal-footer">
        <asp:Button ID="btnGuardarModal" runat="server" Text="Guardar" CssClass="btn btn-success" OnClick="btnGuardarModal_Click" />
        <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancelar</button>
      </div>

    </div>
  </div>
</div>

<!-- Modal para mostrar credencial con QR -->
<div class="modal fade" id="modalQr" tabindex="-1" role="dialog" aria-labelledby="modalQrLabel" aria-hidden="true">
  <div class="modal-dialog" role="document">
    <div class="modal-content">
      <div class="modal-header text-white" style="background-color: #003366;">
        <h5 class="modal-title" id="modalQrLabel">Credencial de empleado</h5>
        <button type="button" class="close text-white" data-dismiss="modal" aria-label="Cerrar">
          <span aria-hidden="true">&times;</span>
        </button>
      </div>
      <div class="modal-body d-flex justify-content-center">
        <div class="credencial-card" id="credencialEmpleado">
          <div class="credencial-header">
            <div class="credencial-header-bar"></div>
            <div class="credencial-logo-text">
                <div class="empresa"><img src="img/ankhal.png" width="30px" /> GRUPO ANKHAL</div>
              <div class="subtitulo">Credencial de empleado</div>
            </div>
          </div>
          <div class="credencial-body">
            <h5 id="lblNombreCompleto"></h5>
            <div class="credencial-datos">
              <div>
                <p><strong>N&uacute;mero de empleado:</strong><br /><span id="lblNumeroEmpleado"></span></p>
                <p><strong>&aacute;rea:</strong><br /><span id="lblArea"></span></p>
              </div>
              <div id="qrContainer"></div>
            </div>
          </div>
          <div class="credencial-footer">
            Uso interno · Grupo Ankhal
          </div>
        </div>
      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-secondary" data-dismiss="modal">Cerrar</button>
        <button type="button" class="btn btn-primary" onclick="imprimirCredencial()">Imprimir</button>
      </div>
    </div>
  </div>
</div>

<script type="text/javascript">
    function abrirModal() {
        $('#modalAgregar').modal('show');
        abrirCamara();
    }

    function abrirModalEditar(
        idUsuario, idRol, idArea, idPuesto, idPlanta, nombre, apellidoPaterno, apellidoMaterno,
        curp, rfc, fechaNacimiento, fechaIngreso, genero, estadoSocial, telefono,
        seguroSocial, numeroEmpleado, email, direccion, nombreFamilia, telefonoFamiliar,
        usuario, clave, edad, dispositivo1, mac1, dispositivo2, mac2) {

        // IDs ocultos y combos
        document.getElementById('<%= hfIdUsuario.ClientID %>').value = idUsuario;
        document.getElementById('<%= ddlRolModal.ClientID %>').value = idRol;
        document.getElementById('<%= ddlAreaModal.ClientID %>').value = idArea;
        document.getElementById('<%= ddlPlantaModal.ClientID %>').value = idPlanta || '0';
        document.getElementById('<%= ddlPuestoModal.ClientID %>').value = idPuesto;

        // Datos personales
        document.getElementById('<%= txtNombreModal.ClientID %>').value = nombre;
        document.getElementById('<%= txtApellidoPaternoModal.ClientID %>').value = apellidoPaterno;
        document.getElementById('<%= txtApellidoMaternoModal.ClientID %>').value = apellidoMaterno;
        document.getElementById('<%= txtCurpModal.ClientID %>').value = curp; 
        document.getElementById('<%= txtRfcModal.ClientID %>').value = rfc;
        document.getElementById('<%= FechaNacimientoModal.ClientID %>').value = fechaNacimiento;
        document.getElementById('<%= FechaIngreModal.ClientID %>').value = fechaIngreso;
        document.getElementById('<%= ddlGeneroModal.ClientID %>').value = genero;
        document.getElementById('<%= ddlEstadoSocialModal.ClientID %>').value = estadoSocial;

        // Contacto
        document.getElementById('<%= txtTelefonoModal.ClientID %>').value = telefono;
        document.getElementById('<%= txtEmailModal.ClientID %>').value = email;
        document.getElementById('<%= txtDireccionModal.ClientID %>').value = direccion;

        // Familia
        document.getElementById('<%= txtNombreFamiliaModal.ClientID %>').value = nombreFamilia;
        document.getElementById('<%= txtTelefonoFamiliarModal.ClientID %>').value = telefonoFamiliar;

        // Laboral
        document.getElementById('<%= txtSeguroSocialModal.ClientID %>').value = seguroSocial;
        document.getElementById('<%= txtNumeroEmpleadoModal.ClientID %>').value = numeroEmpleado;
        document.getElementById('<%= txtUsuarioModal.ClientID %>').value = usuario;
        document.getElementById('<%= txtClaveModal.ClientID %>').value = clave;
        document.getElementById('<%= txtEdadModal.ClientID %>').value = edad;

        // Dispositivos
        document.getElementById('<%= txtDispositivo1Modal.ClientID %>').value = dispositivo1;
        document.getElementById('<%= txtMac1Modal.ClientID %>').value = mac1;
        document.getElementById('<%= txtDispositivo2Modal.ClientID %>').value = dispositivo2;
        document.getElementById('<%= txtMac2Modal.ClientID %>').value = mac2;

        // Mostrar modal de edición
        $('#modalEditar').modal('show');
    }

    //este es para enviar la foto al back
    // Prender cámara
    function abrirCamara() {
        navigator.mediaDevices.getUserMedia({ video: true })
            .then(stream => {
                document.getElementById("videoCamara").srcObject = stream;
            })
            .catch(err => {
                alert("Error al acceder a la cámara: " + err);
            });
    }

    // Tomar la foto y convertirla a Base64
    function tomarFoto() {
        let video = document.getElementById("videoCamara");
        let canvas = document.getElementById("canvasFoto");
        let ctx = canvas.getContext("2d");

        ctx.drawImage(video, 0, 0, canvas.width, canvas.height);

        // Obtener Base64
        let dataUrl = canvas.toDataURL("image/jpeg");

        // Guardar el Base64 en el HiddenField ASP.NET
        document.getElementById("<%= hdFoto.ClientID %>").value = dataUrl;

        Swal.fire("Foto guardada", "", "success");
    }

    // Abre el modal de credencial y genera el QR
    function abrirModalQr(nombre, apellidoPaterno, apellidoMaterno, numeroEmpleado, area) {
        var nombreCompleto = (nombre + " " + apellidoPaterno + " " + apellidoMaterno).trim();

        document.getElementById("lblNombreCompleto").innerText = nombreCompleto;
        document.getElementById("lblNumeroEmpleado").innerText = numeroEmpleado;
        document.getElementById("lblArea").innerText = area;

        // Limpiar QR anterior
        var contenedor = document.getElementById("qrContainer");
        contenedor.innerHTML = "";

        if (numeroEmpleado && numeroEmpleado !== "") {
            new QRCode(contenedor, {
                text: numeroEmpleado,
                width: 110,
                height: 110
            });
        }

        $('#modalQr').modal('show');
    }

    // Imprimir solo la credencial
    function imprimirCredencial() {
        var contenido = document.getElementById("credencialEmpleado").innerHTML;
        var estilo = `
            <style>
                body { margin: 0; font-family: 'Segoe UI', Arial, sans-serif; background: #ffffff; }
                .credencial-card {
                    width: 340px;
                    height: 210px;
                    border-radius: 14px;
                    border: 3px solid #ff6a00;
                    background: #ffffff;
                    overflow: hidden;
                    box-shadow: 0 8px 18px rgba(0, 0, 0, 0.25);
                    font-family: 'Segoe UI', Arial, sans-serif;
                    margin: 20px auto;
                }
                .credencial-header {
                    background: #111111;
                    color: #ffffff;
                    padding: 6px 10px;
                    display: flex;
                    align-items: center;
                }
                .credencial-header-bar {
                    width: 8px;
                    height: 32px;
                    border-radius: 4px;
                    background: #ff6a00;
                }
                .credencial-logo-text { margin-left: 10px; }
                .credencial-logo-text .empresa {
                    font-weight: 700;
                    font-size: 15px;
                    letter-spacing: 0.5px;
                    color: #ff6a00;
                }
                .credencial-logo-text .subtitulo {
                    font-size: 11px;
                    color: #ffffff;
                    opacity: 0.9;
                }
                .credencial-body {
                    padding: 10px 12px;
                    font-size: 12px;
                    color: #111111;
                }
                .credencial-body h5 {
                    font-size: 15px;
                    font-weight: 600;
                    margin-bottom: 4px;
                }
                .credencial-datos {
                    display: flex;
                    justify-content: space-between;
                    align-items: center;
                    margin-top: 6px;
                }
                .credencial-datos p { margin-bottom: 2px; }
                .credencial-footer {
                    background: #f5f5f5;
                    border-top: 1px solid #e0e0e0;
                    padding: 6px 10px;
                    font-size: 11px;
                    text-align: center;
                    color: #555555;
                }
            </style>
        `;

        var ventana = window.open("", "_blank", "width=400,height=600");
        ventana.document.write("<html><head><title>Imprimir credencial</title>");
        ventana.document.write(estilo);
        ventana.document.write("</head><body>");
        ventana.document.write('<div class="credencial-wrapper">' + contenido + "</div>");
        ventana.document.write("</body></html>");
        ventana.document.close();
        ventana.focus();
        ventana.print(); // aquí el navegador muestra el cuadro de impresión con todas las impresoras disponibles
        // NO cerramos la ventana para que el usuario pueda reintentar o cambiar impresora si lo requiere
    }


</script>

</asp:Content>
