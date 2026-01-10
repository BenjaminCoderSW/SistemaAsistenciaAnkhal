<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AsignarHorario.aspx.cs" Inherits="GrupoAnkhalAsistencia.AsignarHorario" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <!-- Tus estilos propios -->
    <link href="css/gridviewPantalla.css" rel="stylesheet" />

    <!-- ✅ jQuery debe ir ANTES de Select2 -->
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>

    <!-- SweetAlert y scripts propios -->
    <script src="scriptspropios/sweetalert2@11.js"></script>
    <script src="scriptspropios/propios.js"></script>

    <!-- ✅ Select2 (después de jQuery) -->
  <link href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css" rel="stylesheet" />
<script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>


    <!-- ✅ Activar Select2 para tus combos -->
    <script type="text/javascript">
        $(document).ready(function () {
            // Combo del modal de agregar
            $('#<%= ddlHorario.ClientID %>').select2({
                placeholder: 'Selecciona un horario',
                allowClear: true,
                width: '100%'
            });
            $('#<%= ddlUsuario.ClientID %>').select2({
                placeholder: 'Selecciona un empleado',
                allowClear: true,
                width: '100%'
            });

            // Combo del modal de editar
            $('#<%= ddlHorarioModal.ClientID %>').select2({
                placeholder: 'Selecciona un horario',
                allowClear: true,
                width: '100%'
            });
            $('#<%= ddlUsuarioModal.ClientID %>').select2({
                placeholder: 'Selecciona un empleado',
                allowClear: true,
                width: '100%'
            });
        });
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
      <h2>Asignacion de Horarios</h2>
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
<asp:GridView ID="dvgAsignacionHorario" runat="server" AutoGenerateColumns="False"
    CssClass="table table-bordered table-striped custom-grid"
    AllowPaging="True" PageSize="5"
    OnPageIndexChanging="dvgAsignacionHorario_PageIndexChanging">
    <Columns>
        <asp:BoundField DataField="Horario" HeaderText="Horario" />
                <asp:TemplateField HeaderText="Usuario">
    <ItemTemplate>
        <span class="truncate-text" title='<%# Eval("Usuario") %>'>
            <%# Eval("Usuario") %>
        </span>
    </ItemTemplate>
</asp:TemplateField><asp:BoundField DataField="Dia" HeaderText="Dia" />
     <asp:TemplateField HeaderText="Acciones">
      <ItemTemplate>
           <button type="button" class="btn btn-warning btn-sm"
         onclick="abrirModalEditar('<%# Eval("IdAsignarHorario") %>',
 '<%# Eval("IdHorario") %>',
 '<%# Eval("IdUsuario") %>',
 '<%# Eval("IdDia") %>')">
     Editar
 </button>
      
         <asp:Button ID="btnEliminar" runat="server" Text="Eliminar" CssClass="btn btn-danger btn-sm"
                      CommandArgument='<%# Eval("IdAsignarHorario") %>' OnClick="btnEliminar_Click" />
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
        <h5 class="modal-title" id="modalAgregarLabel">Agregar Horario</h5>
        <button type="button" class="close text-white" data-dismiss="modal" aria-label="Cerrar">
          <span aria-hidden="true">&times;</span>
        </button>
      </div>

      <div class="modal-body">
        <div class="container-fluid">
          <div class="row">
            
            <!-- Primera sección -->
            <div class="col-md-6 mb-3">
              <label for="ddlRol">Horario</label>
              <asp:DropDownList ID="ddlHorario" runat="server" CssClass="form-control"></asp:DropDownList>
            </div>
            <div class="col-md-6 mb-3">
              <label for="ddlArea">Empleado</label>
              <asp:DropDownList ID="ddlUsuario" runat="server" CssClass="form-control"></asp:DropDownList>
            </div>
             <div class="col-md-6 mb-3">
              <label>Día(s)</label>
              <asp:CheckBoxList ID="chkDias" runat="server" RepeatDirection="Vertical" CssClass="form-check"></asp:CheckBoxList>
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
        <h5 class="modal-title" id="modalEditarLabel">Editar Horario</h5>
        <button type="button" class="close text-white" data-dismiss="modal" aria-label="Cerrar">
          <span aria-hidden="true">&times;</span>
        </button>
      </div>

      <!-- Body -->
      <div class="modal-body">
        <asp:HiddenField ID="hfIdAsignarHorario" runat="server" />

        <div class="form-row">
            <div class="col-md-6 mb-3">
  <label for="ddlRol">Horario</label>
  <asp:DropDownList ID="ddlHorarioModal" runat="server" CssClass="form-control"></asp:DropDownList>
</div>
<div class="col-md-6 mb-3">
  <label for="ddlArea">Empleado</label>
  <asp:DropDownList ID="ddlUsuarioModal" runat="server" CssClass="form-control"></asp:DropDownList>
</div>
<div class="col-md-6 mb-3">
  <label for="ddlPuesto">Dia</label>
  <asp:DropDownList ID="ddlDiaModal" runat="server" CssClass="form-control"></asp:DropDownList>
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

<script type="text/javascript">
    function abrirModal() {
        $('#modalAgregar').modal('show');
    }

    function abrirModalEditar(
        idAsignarHorario, idHorario, idUsuario, idDia) {

        // IDs ocultos y combos
        document.getElementById('<%= hfIdAsignarHorario.ClientID %>').value = idAsignarHorario;
        document.getElementById('<%= ddlHorarioModal.ClientID %>').value = idHorario;
        document.getElementById('<%= ddlUsuarioModal.ClientID %>').value = idUsuario;
        document.getElementById('<%= ddlDiaModal.ClientID %>').value = idDia;
        // Mostrar modal de edición
        $('#modalEditar').modal('show');
    }

        
    </script>

</asp:Content>
