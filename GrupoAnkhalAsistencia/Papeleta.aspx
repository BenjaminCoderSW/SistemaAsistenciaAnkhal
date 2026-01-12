<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Papeleta.aspx.cs" Inherits="GrupoAnkhalAsistencia.Papeleta" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="css/gridviewPantalla.css" rel="stylesheet" />
    <style>
        .custom-grid {
            width: 100%;
            border-collapse: collapse;
            background-color: white;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }

        .custom-grid th {
            background-color: #003366;
            color: white;
            padding: 12px;
            text-align: left;
            font-weight: bold;
        }

        .custom-grid td {
            padding: 10px;
            border-bottom: 1px solid #ddd;
        }

        .custom-grid tr:hover {
            background-color: #f5f5f5;
        }

        .custom-grid tr:nth-child(even) {
            background-color: #f9f9f9;
        }

        .btn-action {
            padding: 5px 10px;
            margin: 2px;
            border: none;
            border-radius: 3px;
            cursor: pointer;
            font-size: 12px;
        }

        .btn-edit {
            background-color: #ffc107;
            color: white;
        }

        .btn-delete {
            background-color: #dc3545;
            color: white;
        }

        .btn-print {
            background-color: #17a2b8;
            color: white;
        }

        .modal-header {
            background-color: #003366;
            color: white;
        }

        .form-control, .form-select {
            border: 1px solid #ced4da;
            border-radius: 4px;
            padding: 8px;
        }

        .truncate-text {
            max-width: 180px;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <section class="content-header">
        <div class="container-fluid">
            <div class="row mb-2">
                <div class="col-sm-6">
                    <h1>Gestión de Papeletas</h1>
                </div>
            </div>
        </div>
    </section>

    <section class="content">
        <div class="container-fluid">
            <div class="card">
                <div class="card-header">
                    <div class="row">
                        <div class="col-md-6">
                            <button type="button" class="btn btn-primary" onclick="abrirModal()">
                                <i class="fas fa-plus"></i> Agregar Nueva Papeleta
                            </button>
                        </div>
                        <div class="col-md-6">
                            <div class="input-group">
                                <asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control" 
                                    placeholder="Buscar por empleado..." AutoPostBack="true" 
                                    OnTextChanged="txtBuscar_TextChanged"></asp:TextBox>
                                <span class="input-group-text">
                                    <i class="fas fa-search"></i>
                                </span>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="card-body">
                    <asp:GridView ID="dvgPapeleta" runat="server" CssClass="custom-grid" 
                        AutoGenerateColumns="False" AllowPaging="True" PageSize="10"
                        OnPageIndexChanging="dvgPapeleta_PageIndexChanging">
                        <Columns>
                            <asp:BoundField DataField="IdPapeleta" HeaderText="ID" Visible="false" />
                            <asp:BoundField DataField="IdUsuario" HeaderText="IdUsuario" Visible="false" />
                            <asp:BoundField DataField="NombreCompleto" HeaderText="Empleado" />
                            <asp:BoundField DataField="RFC" HeaderText="RFC" />
                            <asp:BoundField DataField="NTrabajador" HeaderText="N° Trabajador" />
                            <asp:BoundField DataField="PeriodoPago" HeaderText="Periodo de Pago" />
                            <asp:BoundField DataField="FechaPago" HeaderText="Fecha Pago" DataFormatString="{0:dd/MM/yyyy}" />
                            <asp:BoundField DataField="NetoPagar" HeaderText="Neto a Pagar" DataFormatString="${0:N2}" />
                            <asp:TemplateField HeaderText="Acciones">
                                <ItemTemplate>
                                    <asp:Button ID="btnEditar" runat="server" Text="Editar" 
                                        CssClass="btn-action btn-edit"
                                        OnClientClick='<%# "abrirModalEditar(" + 
                                            Eval("IdPapeleta") + ",\"" + 
                                            Eval("IdUsuario") + "\",\"" + 
                                            Eval("PeriodoPago") + "\"," + 
                                            Eval("DiasPagados") + ",\"" + 
                                            Eval("FechaPago", "{0:yyyy-MM-dd}") + "\"," + 
                                            Eval("SueldoPeriodo") + "," + 
                                            Eval("HorasExtras") + "," + 
                                            Eval("Bonos") + "," + 
                                            Eval("DiasPendientesPago") + "," + 
                                            Eval("Veladas") + "," + 
                                            Eval("OtrosIngresos") + "," + 
                                            Eval("DiasNoLaborados") + "," + 
                                            Eval("DescuentoHoras") + "," + 
                                            Eval("OtrosDescuentos") + 
                                            "); return false;" %>' />
                                    
                                    <asp:Button ID="btnImprimir" runat="server" Text="Imprimir" 
                                        CssClass="btn-action btn-print"
                                        OnClientClick='<%# "window.open(\"ImprimirPapeleta.aspx?id=" + 
                                            Eval("IdPapeleta") + "\", \"_blank\"); return false;" %>' />
                                    
                                    <asp:Button ID="btnEliminar" runat="server" Text="Eliminar" 
                                        CssClass="btn-action btn-delete"
                                        CommandArgument='<%# Eval("IdPapeleta") %>'
                                        OnClick="btnEliminar_Click"
                                        OnClientClick="return confirm('¿Está seguro de eliminar esta papeleta?');" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <PagerStyle CssClass="pagination" HorizontalAlign="Center" />
                    </asp:GridView>
                </div>
            </div>
        </div>
    </section>

    <!-- Modal para Agregar/Editar Papeleta -->
    <div class="modal fade" id="modalPapeleta" tabindex="-1" role="dialog">
        <div class="modal-dialog modal-xl" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Registro de Papeleta</h5>
                    <button type="button" class="close" data-dismiss="modal">
                        <span>&times;</span>
                    </button>
                </div>
                <div class="modal-body">
                    <asp:HiddenField ID="hdIdPapeleta" runat="server" Value="0" />
                    
                    <div class="row">
                        <div class="col-md-6">
                            <div class="form-group">
                                <label>Empleado *</label>
                                <asp:DropDownList ID="ddlUsuario" runat="server" CssClass="form-control">
                                </asp:DropDownList>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="form-group">
                                <label>Periodo de Pago *</label>
                                <asp:TextBox ID="txtPeriodoPago" runat="server" CssClass="form-control" 
                                    placeholder="Ej: del 05 al 11 de enero del 2026"></asp:TextBox>
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-4">
                            <div class="form-group">
                                <label>Días Pagados *</label>
                                <asp:TextBox ID="txtDiasPagados" runat="server" CssClass="form-control" 
                                    TextMode="Number" placeholder="7"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-md-4">
                            <div class="form-group">
                                <label>Fecha de Pago *</label>
                                <asp:TextBox ID="txtFechaPago" runat="server" CssClass="form-control" 
                                    TextMode="Date"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-md-4">
                            <div class="form-group">
                                <label>Sueldo del Período</label>
                                <asp:TextBox ID="txtSueldoPeriodo" runat="server" CssClass="form-control" 
                                    TextMode="Number" step="0.01" placeholder="1000.00"></asp:TextBox>
                            </div>
                        </div>
                    </div>

                    <h5 class="mt-3 mb-3" style="color: #003366;">PERCEPCIONES</h5>
                    <div class="row">
                        <div class="col-md-4">
                            <div class="form-group">
                                <label>Horas Extras</label>
                                <asp:TextBox ID="txtHorasExtras" runat="server" CssClass="form-control" 
                                    TextMode="Number" step="0.01" placeholder="0.00"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-md-4">
                            <div class="form-group">
                                <label>Bonos</label>
                                <asp:TextBox ID="txtBonos" runat="server" CssClass="form-control" 
                                    TextMode="Number" step="0.01" placeholder="0.00"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-md-4">
                            <div class="form-group">
                                <label>Días Pendientes de Pago</label>
                                <asp:TextBox ID="txtDiasPendientesPago" runat="server" CssClass="form-control" 
                                    TextMode="Number" step="0.01" placeholder="0.00"></asp:TextBox>
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <div class="form-group">
                                <label>Veladas</label>
                                <asp:TextBox ID="txtVeladas" runat="server" CssClass="form-control" 
                                    TextMode="Number" step="0.01" placeholder="0.00"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="form-group">
                                <label>Otros Ingresos</label>
                                <asp:TextBox ID="txtOtrosIngresos" runat="server" CssClass="form-control" 
                                    TextMode="Number" step="0.01" placeholder="0.00"></asp:TextBox>
                            </div>
                        </div>
                    </div>

                    <h5 class="mt-3 mb-3" style="color: #003366;">DEDUCCIONES</h5>
                    <div class="row">
                        <div class="col-md-4">
                            <div class="form-group">
                                <label>Días No Laborados</label>
                                <asp:TextBox ID="txtDiasNoLaborados" runat="server" CssClass="form-control" 
                                    TextMode="Number" step="0.01" placeholder="0.00"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-md-4">
                            <div class="form-group">
                                <label>Descuento de Horas</label>
                                <asp:TextBox ID="txtDescuentoHoras" runat="server" CssClass="form-control" 
                                    TextMode="Number" step="0.01" placeholder="0.00"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-md-4">
                            <div class="form-group">
                                <label>Otros Descuentos</label>
                                <asp:TextBox ID="txtOtrosDescuentos" runat="server" CssClass="form-control" 
                                    TextMode="Number" step="0.01" placeholder="0.00"></asp:TextBox>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <asp:Button ID="btnGuardar" runat="server" Text="Guardar" 
                        CssClass="btn btn-primary" OnClick="btnGuardar_Click" />
                    <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancelar</button>
                </div>
            </div>
        </div>
    </div>

    <script src="scriptsPropios/sweetalert2@11.js"></script>
    <script>
        function abrirModal() {
            document.getElementById('<%= hdIdPapeleta.ClientID %>').value = '0';
            document.getElementById('<%= ddlUsuario.ClientID %>').value = '';
            document.getElementById('<%= txtPeriodoPago.ClientID %>').value = '';
            document.getElementById('<%= txtDiasPagados.ClientID %>').value = '';
            document.getElementById('<%= txtFechaPago.ClientID %>').value = '';
            document.getElementById('<%= txtSueldoPeriodo.ClientID %>').value = '';
            document.getElementById('<%= txtHorasExtras.ClientID %>').value = '';
            document.getElementById('<%= txtBonos.ClientID %>').value = '';
            document.getElementById('<%= txtDiasPendientesPago.ClientID %>').value = '';
            document.getElementById('<%= txtVeladas.ClientID %>').value = '';
            document.getElementById('<%= txtOtrosIngresos.ClientID %>').value = '';
            document.getElementById('<%= txtDiasNoLaborados.ClientID %>').value = '';
            document.getElementById('<%= txtDescuentoHoras.ClientID %>').value = '';
            document.getElementById('<%= txtOtrosDescuentos.ClientID %>').value = '';
            
            $('#modalPapeleta').modal('show');
        }

        function abrirModalEditar(id, idUsuario, periodo, dias, fecha, sueldo, horas, bonos, 
                                   diasPend, veladas, otros, diasNoLab, descHoras, otrosDesc) {
            document.getElementById('<%= hdIdPapeleta.ClientID %>').value = id;
            document.getElementById('<%= ddlUsuario.ClientID %>').value = idUsuario;
            document.getElementById('<%= txtPeriodoPago.ClientID %>').value = periodo;
            document.getElementById('<%= txtDiasPagados.ClientID %>').value = dias;
            document.getElementById('<%= txtFechaPago.ClientID %>').value = fecha;
            document.getElementById('<%= txtSueldoPeriodo.ClientID %>').value = sueldo;
            document.getElementById('<%= txtHorasExtras.ClientID %>').value = horas;
            document.getElementById('<%= txtBonos.ClientID %>').value = bonos;
            document.getElementById('<%= txtDiasPendientesPago.ClientID %>').value = diasPend;
            document.getElementById('<%= txtVeladas.ClientID %>').value = veladas;
            document.getElementById('<%= txtOtrosIngresos.ClientID %>').value = otros;
            document.getElementById('<%= txtDiasNoLaborados.ClientID %>').value = diasNoLab;
            document.getElementById('<%= txtDescuentoHoras.ClientID %>').value = descHoras;
            document.getElementById('<%= txtOtrosDescuentos.ClientID %>').value = otrosDesc;
            
            $('#modalPapeleta').modal('show');
        }
    </script>
</asp:Content>
