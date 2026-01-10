<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Area.aspx.cs" Inherits="GrupoAnkhalAsistencia.Area" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
      <link href="css/gridviewPantalla.css" rel="stylesheet" />
<script src="scriptspropios/sweetalert2@11.js"></script>
<script src="scriptspropios/propios.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
                    <h2>Gestión de Area</h2>

    <!-- Botón Agregar -->
    <asp:Button ID="btnAgregar" runat="server" Text="Agregar Nuevo" CssClass="btn btn-primary mb-3"  OnClientClick="abrirModal(); return false;" />

    <!-- Grid de datos -->
  <br />

<div class="table-responsive">
    <div class="col-md-6"> 
        <asp:TextBox ID="txtBuscar" runat="server" 
   CssClass="form-control" 
   Placeholder="Buscar área..."
   AutoPostBack="true"
   OnTextChanged="txtBuscar_TextChanged" />
    </div>
    <br />
<asp:GridView ID="dvgArea" runat="server" AutoGenerateColumns="False"
    CssClass="table table-bordered table-striped custom-grid"
    AllowPaging="True" PageSize="5"
    OnPageIndexChanging="dvgArea_PageIndexChanging">
    <Columns>
        <asp:BoundField DataField="Area" HeaderText="Area" />
       
        <asp:TemplateField HeaderText="Acciones">
            <ItemTemplate>
                <button type="button" class="btn btn-warning btn-sm"
                        onclick="abrirModalEditar('<%# Eval("IdArea") %>','<%# Eval("Area") %>')">
                    Editar
                </button>
                <asp:Button ID="btnEliminar" runat="server" Text="Eliminar" CssClass="btn btn-danger btn-sm"
                            CommandArgument='<%# Eval("IdArea") %>' OnClick="btnEliminar_Click" />
            </ItemTemplate>
        </asp:TemplateField>
    </Columns>
</asp:GridView>
    </div>


    <!-- Formulario para agregar/editar -->
     <!-- Modal Bootstrap -->
    <div class="modal fade" id="modalAgregar" tabindex="-1" role="dialog" aria-labelledby="modalAgregarLabel" aria-hidden="true">
     <div class="modal-dialog modal-xl" role="document">
        <div class="modal-content">
          <div class="modal-header text-white" style="background-color: #003366;">
            <h5 class="modal-title" id="modalAgregarLabel">Agregar Ubicación</h5>
            <button type="button" class="close" data-dismiss="modal" aria-label="Cerrar">
              <span aria-hidden="true">&times;</span>
            </button>
          </div>
       <div class="modal-body">
           <div class="row">
               <div class="form-group col-md-6">
                    <asp:TextBox ID="txtArea" runat="server" CssClass="form-control " Placeholder="Area"></asp:TextBox>
                  
               </div>
           </div>
         </div>
          <div class="modal-footer">
            <asp:Button ID="btnGuardar" runat="server" Text="Guardar" CssClass="btn btn-success" OnClick="btnGuardar_Click" />
            <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancelar</button>
          </div>
        </div>
      </div>
    </div>


    <div class="modal fade" id="modalEditar" tabindex="-1" role="dialog" aria-labelledby="modalEditarLabel" aria-hidden="true">
  <div class="modal-dialog modal-xl" role="document">
    <div class="modal-content">
      <div class="modal-header text-white" style="background-color: #003366;">
        <h5 class="modal-title" id="modalEditarLabel">Editar Area</h5>
        <button type="button" class="close" data-dismiss="modal" aria-label="Cerrar">
          <span aria-hidden="true">&times;</span>
        </button>
      </div>
      <div class="modal-body">
            <div class="row">
                <asp:HiddenField ID="hfIdArea" runat="server" />
                   <div class="form-group col-md-6">
                <asp:TextBox ID="txtAreaModal" runat="server" CssClass="form-control mb-2" Placeholder="Area"></asp:TextBox>
                   </div>
             </div>
      </div>
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

        //editar modal
        function abrirModalEditar(idarea, area) {
            // Llenar los campos del modal
            document.getElementById('<%= hfIdArea.ClientID %>').value = idarea;
            document.getElementById('<%= txtAreaModal.ClientID %>').value = area;
            // Abrir modal
            $('#modalEditar').modal('show');
        }

    </script>
</asp:Content>
