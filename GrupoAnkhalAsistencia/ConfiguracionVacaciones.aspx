<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ConfiguracionVacaciones.aspx.cs" Inherits="GrupoAnkhalAsistencia.ConfiguracionVacaciones" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="css/gridviewPantalla.css" rel="stylesheet" />
    <script src="scriptspropios/sweetalert2@11.js"></script>
    <script src="scriptspropios/propios.js"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container-fluid">
        <div class="row">
            <div class="col-12">
                <div class="card">
                    <div class="card-header" style="background-color: #003366; color: white;">
                        <h3 class="card-title"><i class="fas fa-calendar-alt"></i> Configuración de Días de Vacaciones</h3>
                    </div>
                    <div class="card-body">
                        
                        <!-- Botón para agregar nueva configuración -->
                        <div class="mb-3">
                            <asp:Button ID="btnAgregar" runat="server" Text="Agregar Configuración" 
                                CssClass="btn btn-primary" OnClientClick="abrirModal(); return false;" />
                            
                            <asp:Button ID="btnActualizarTodos" runat="server" Text="Actualizar Días de Todos los Usuarios" 
                                CssClass="btn btn-success ml-2" OnClick="btnActualizarTodos_Click" 
                                OnClientClick="return confirm('¿Está seguro de actualizar los días de vacaciones de todos los usuarios según su antigüedad?');" />
                        </div>

                        <!-- Grid de configuración -->
                        <div class="table-responsive">
                            <asp:GridView ID="dvgConfiguracion" runat="server" AutoGenerateColumns="False"
                                CssClass="table table-bordered table-striped custom-grid"
                                AllowPaging="False">
                                <Columns>
                                    <asp:BoundField DataField="IdConfigVacaciones" HeaderText="ID" Visible="false" />
                                    <asp:BoundField DataField="AñosAntiguedad" HeaderText="Años de Antigüedad" />
                                    <asp:BoundField DataField="DiasCorresponden" HeaderText="Días que Corresponden" />
                                    <asp:TemplateField HeaderText="Estatus">
                                        <ItemTemplate>
                                            <span class="badge badge-<%# Convert.ToInt32(Eval("Estatus")) == 1 ? "success" : "secondary" %>">
                                                <%# Convert.ToInt32(Eval("Estatus")) == 1 ? "Activo" : "Inactivo" %>
                                            </span>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Acciones">
                                        <ItemTemplate>
                                            <button type="button" class="btn btn-warning btn-sm"
                                                onclick="abrirModalEditar(
                                                    '<%# Eval("IdConfigVacaciones") %>',
                                                    '<%# Eval("AñosAntiguedad") %>',
                                                    '<%# Eval("DiasCorresponden") %>'
                                                )">
                                                <i class="fas fa-edit"></i> Editar
                                            </button>
                                            <asp:Button ID="btnEliminar" runat="server" Text="Eliminar" 
                                                CssClass="btn btn-danger btn-sm"
                                                CommandArgument='<%# Eval("IdConfigVacaciones") %>' 
                                                OnClick="btnEliminar_Click"
                                                OnClientClick="return confirm('¿Está seguro de eliminar esta configuración?');" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>

                        <!-- Información adicional -->
                        <div class="alert alert-info mt-3">
                            <h5><i class="fas fa-info-circle"></i> Información</h5>
                            <ul>
                                <li>Los días de vacaciones se asignan automáticamente con el boton de actualizar según la fecha de ingreso del empleado.</li>
                                <li>Los días NO son acumulables. Cada aniversario se restablecen según la antigüedad actual.</li>
                                <li>Esta configuración se basa en la Ley Federal del Trabajo.</li>
                            </ul>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Modal Agregar -->
    <div class="modal fade" id="modalAgregar" tabindex="-1" role="dialog">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header" style="background-color: #003366; color: white;">
                    <h5 class="modal-title">Agregar Configuración de Vacaciones</h5>
                    <button type="button" class="close text-white" data-dismiss="modal">
                        <span>&times;</span>
                    </button>
                </div>
                <div class="modal-body">
                    <div class="form-group">
                        <label>Años de Antigüedad (*)</label>
                        <asp:TextBox ID="txtAnios" runat="server" CssClass="form-control" 
                            TextMode="Number" Placeholder="Ej: 1, 2, 5, 10..." />
                    </div>
                    <div class="form-group">
                        <label>Días que Corresponden (*)</label>
                        <asp:TextBox ID="txtDias" runat="server" CssClass="form-control" 
                            TextMode="Number" Placeholder="Ej: 12, 14, 20..." />
                    </div>
                </div>
                <div class="modal-footer">
                    <asp:Button ID="btnGuardar" runat="server" Text="Guardar" 
                        CssClass="btn btn-success" OnClick="btnGuardar_Click" />
                    <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancelar</button>
                </div>
            </div>
        </div>
    </div>

    <!-- Modal Editar -->
    <div class="modal fade" id="modalEditar" tabindex="-1" role="dialog">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header" style="background-color: #003366; color: white;">
                    <h5 class="modal-title">Editar Configuración de Vacaciones</h5>
                    <button type="button" class="close text-white" data-dismiss="modal">
                        <span>&times;</span>
                    </button>
                </div>
                <div class="modal-body">
                    <asp:HiddenField ID="hfIdConfig" runat="server" />
                    
                    <div class="form-group">
                        <label>Años de Antigüedad (*)</label>
                        <asp:TextBox ID="txtAniosModal" runat="server" CssClass="form-control" 
                            TextMode="Number" />
                    </div>
                    <div class="form-group">
                        <label>Días que Corresponden (*)</label>
                        <asp:TextBox ID="txtDiasModal" runat="server" CssClass="form-control" 
                            TextMode="Number" />
                    </div>
                </div>
                <div class="modal-footer">
                    <asp:Button ID="btnGuardarModal" runat="server" Text="Guardar Cambios" 
                        CssClass="btn btn-success" OnClick="btnGuardarModal_Click" />
                    <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancelar</button>
                </div>
            </div>
        </div>
    </div>

    <script>
        function abrirModal() {
            $('#modalAgregar').modal('show');
        }

        function abrirModalEditar(id, anios, dias) {
            document.getElementById('<%= hfIdConfig.ClientID %>').value = id;
            document.getElementById('<%= txtAniosModal.ClientID %>').value = anios;
            document.getElementById('<%= txtDiasModal.ClientID %>').value = dias;
            $('#modalEditar').modal('show');
        }
    </script>
</asp:Content>