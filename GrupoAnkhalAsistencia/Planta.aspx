<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Planta.aspx.cs" Inherits="GrupoAnkhalAsistencia.Planta" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
              <link href="css/gridviewPantalla.css" rel="stylesheet" />
<script src="scriptspropios/sweetalert2@11.js"></script>
<script src="scriptspropios/propios.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
                            <h2>Gestión de Plantas</h2>

    <!-- Botón Agregar -->
    <asp:Button ID="btnAgregar" runat="server" Text="Agregar Nuevo" CssClass="btn btn-primary mb-3"  OnClientClick="abrirModal(); return false;" />

    <!-- Grid de datos -->
  <br />

<div class="table-responsive">
    <div class="col-md-6"> 
        <asp:TextBox ID="txtBuscar" runat="server" 
   CssClass="form-control" 
   Placeholder="Buscar planta..."
   AutoPostBack="true"
   OnTextChanged="txtBuscar_TextChanged" />
    </div>
    <br />
<asp:GridView ID="dvgPlanta" runat="server" AutoGenerateColumns="False"
    CssClass="table table-bordered table-striped custom-grid"
    AllowPaging="True" PageSize="5"
    OnPageIndexChanging="dvgPlanta_PageIndexChanging">
    <Columns>
        <asp:BoundField DataField="Planta" HeaderText="Planta" />
        <asp:BoundField DataField="longitud" HeaderText="longitud" />
        <asp:BoundField DataField="latitud" HeaderText="latitud" /> 
        <asp:TemplateField HeaderText="Acciones">
            <ItemTemplate>
                <button type="button" class="btn btn-warning btn-sm"
                        onclick="abrirModalEditar('<%# Eval("IdPlanta") %>','<%# Eval("Planta") %>','<%# Eval("longitud") %>','<%# Eval("latitud") %>')">
                    Editar
                </button>
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
        <h5 class="modal-title" id="modalAgregarLabel">Agregar Planta</h5>
        <button type="button" class="close" data-dismiss="modal" aria-label="Cerrar">
          <span aria-hidden="true">&times;</span>
        </button>
      </div>

      <div class="modal-body">
        <div class="row">
          <div class="form-group col-md-6">
            <label>Planta:</label>
            <asp:TextBox ID="txtPlanta" runat="server" CssClass="form-control" Placeholder="Planta"></asp:TextBox>
          </div>

          <div class="form-group col-md-6">
            <label>Latitud:</label>
            <asp:TextBox ID="txtlatitud" runat="server" CssClass="form-control" Placeholder="Latitud"></asp:TextBox>
          </div>

          <div class="form-group col-md-6">
            <label>Longitud:</label>
            <asp:TextBox ID="txtlongitud" runat="server" CssClass="form-control" Placeholder="Longitud"></asp:TextBox>
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

      <!-- HEADER -->
      <div class="modal-header text-white" style="background-color: #003366;">
        <h5 class="modal-title" id="modalEditarLabel">Editar Planta</h5>
        <button type="button" class="close text-white" data-dismiss="modal" aria-label="Cerrar">
          <span aria-hidden="true">&times;</span>
        </button>
      </div>

      <!-- BODY -->
      <div class="modal-body">
        <asp:HiddenField ID="hfIdPlanta" runat="server" />

        <div class="row">
          <div class="form-group col-md-6">
            <label for="HorarioInicioModal">Planta:</label>
              <asp:TextBox ID="PlantaModal" runat="server" CssClass="form-control"></asp:TextBox>
          </div>

          <div class="form-group col-md-6">
            <label for="HorarioFinModal">Longitud:</label>
           <asp:TextBox ID="LongitudModal" runat="server" CssClass="form-control"></asp:TextBox>
          </div>

          <div class="form-group col-md-12">
            <label for="DescripcionModal">Latitud:</label>
            <asp:TextBox ID="LatitudModal" runat="server" CssClass="form-control"></asp:TextBox>
          </div>
        </div>
      </div>

      <!-- FOOTER -->
      <div class="modal-footer d-flex justify-content-end">
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
        function abrirModalEditar(idplanta, planta,longitud, latitud) {
            // Llenar los campos del modal
            document.getElementById('<%= hfIdPlanta.ClientID %>').value = idplanta;
            document.getElementById('<%= PlantaModal.ClientID %>').value = planta;
            document.getElementById('<%= LongitudModal.ClientID %>').value = longitud;
            document.getElementById('<%= LatitudModal.ClientID %>').value = latitud;
            // Abrir modal
            $('#modalEditar').modal('show');
        }
    </script>
</asp:Content>
