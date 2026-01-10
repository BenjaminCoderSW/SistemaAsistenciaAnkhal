<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Horarios.aspx.cs" Inherits="GrupoAnkhalAsistencia.Horarios" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
          <link href="css/gridviewPantalla.css" rel="stylesheet" />
<script src="scriptspropios/sweetalert2@11.js"></script>
<script src="scriptspropios/propios.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
                        <h2>Gestión de Horarios</h2>

    <!-- Botón Agregar -->
    <asp:Button ID="btnAgregar" runat="server" Text="Agregar Nuevo" CssClass="btn btn-primary mb-3"  OnClientClick="abrirModal(); return false;" />

    <!-- Grid de datos -->
  <br />

<div class="table-responsive">
    <div class="col-md-6"> 
        <asp:TextBox ID="txtBuscar" runat="server" 
   CssClass="form-control" 
   Placeholder="Buscar horario..."
   AutoPostBack="true"
   OnTextChanged="txtBuscar_TextChanged" />
    </div>
    <br />
<asp:GridView ID="dvgHorario" runat="server" AutoGenerateColumns="False"
    CssClass="table table-bordered table-striped custom-grid"
    AllowPaging="True" PageSize="5"
    OnPageIndexChanging="dvgHorario_PageIndexChanging">
    <Columns>
        <asp:BoundField DataField="HoraInicio" HeaderText="Hora Inicio" />
        <asp:BoundField DataField="HoraFin" HeaderText="Hora Fin" />
        <asp:BoundField DataField="Descripcion" HeaderText="Descripcion" /> 
        <asp:TemplateField HeaderText="Acciones">
            <ItemTemplate>
                <button type="button" class="btn btn-warning btn-sm"
                        onclick="abrirModalEditar('<%# Eval("IdHorario") %>','<%# Eval("HoraInicio") %>','<%# Eval("HoraFin") %>','<%# Eval("Descripcion") %>')">
                    Editar
                </button>
                <asp:Button ID="btnEliminar" runat="server" Text="Eliminar" CssClass="btn btn-danger btn-sm"
                            CommandArgument='<%# Eval("IdHorario") %>' OnClick="btnEliminar_Click" />
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
            <h5 class="modal-title" id="modalAgregarLabel">Agregar Horario</h5>
            <button type="button" class="close" data-dismiss="modal" aria-label="Cerrar">
              <span aria-hidden="true">&times;</span>
            </button>
          </div>
       <div class="modal-body">
           <div class="row">
              <div class="form-group col-md-6">
                    <label>Hora de Inicio:</label>
                    <input type="time" id="txtHoraInicio" runat="server" class="form-control" />
                </div>
                <div class="form-group col-md-6">
                    <label>Hora de Fin:</label>
                    <input type="time" id="txtHoraFin" runat="server" class="form-control" />
                </div>
               <div class="form-group col-md-6"> 
                   <label>Descripcion:</label> 
                   <asp:TextBox ID="txtDescripcion" runat="server" CssClass="form-control " Placeholder="Descripcion"></asp:TextBox> </div>
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
        <h5 class="modal-title" id="modalEditarLabel">Editar Horario</h5>
        <button type="button" class="close text-white" data-dismiss="modal" aria-label="Cerrar">
          <span aria-hidden="true">&times;</span>
        </button>
      </div>

      <!-- BODY -->
      <div class="modal-body">
        <asp:HiddenField ID="hfIdHorario" runat="server" />

        <div class="row">
          <div class="form-group col-md-6">
            <label for="HorarioInicioModal">Hora de Inicio:</label>
            <input type="time" id="HorarioInicioModal" runat="server" class="form-control" />
          </div>

          <div class="form-group col-md-6">
            <label for="HorarioFinModal">Hora de Fin:</label>
            <input type="time" id="HorarioFinModal" runat="server" class="form-control" />
          </div>

          <div class="form-group col-md-12">
            <label for="DescripcionModal">Descripción:</label>
            <asp:TextBox ID="DescripcionModal" runat="server" CssClass="form-control" Placeholder="Descripción"></asp:TextBox>
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
        function abrirModalEditar(idhorario, inicio, fin ,descripcion) {
            // Llenar los campos del modal
            document.getElementById('<%= hfIdHorario.ClientID %>').value = idhorario;
            document.getElementById('<%= HorarioInicioModal.ClientID %>').value = inicio;
            document.getElementById('<%= HorarioFinModal.ClientID %>').value = fin;
            document.getElementById('<%= DescripcionModal.ClientID %>').value = descripcion;
            // Abrir modal
            $('#modalEditar').modal('show');
        }

    </script>
</asp:Content>
