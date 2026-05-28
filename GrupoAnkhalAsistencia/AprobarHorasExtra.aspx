<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AprobarHorasExtra.aspx.cs" Inherits="GrupoAnkhalAsistencia.AprobarHorasExtra" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta charset="utf-8" />
    <link href="css/gridviewPantalla.css" rel="stylesheet" />
    <script src="scriptspropios/sweetalert2@11.js"></script>
    <script src="scriptspropios/propios.js"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h2>Aprobaci&oacute;n de Horas Extra</h2>
    <br />

    <div class="row">
        <div class="col-md-2">
            <label class="font-weight-bold">Fecha Inicio:</label>
            <asp:TextBox ID="txtFechaInicio" runat="server" CssClass="form-control" TextMode="Date" />
        </div>
        <div class="col-md-2">
            <label class="font-weight-bold">Fecha Fin:</label>
            <asp:TextBox ID="txtFechaFin" runat="server" CssClass="form-control" TextMode="Date" />
        </div>
        <div class="col-md-2">
            <label>Empleado:</label>
            <asp:TextBox ID="txtBuscarEmpleado" runat="server" CssClass="form-control" Placeholder="Buscar por nombre..." />
        </div>
        <div class="col-md-2">
            <label class="font-weight-bold">Planta:</label>
            <asp:DropDownList ID="ddlFiltroPlanta" runat="server" CssClass="form-control" />
        </div>
        <div class="col-md-2">
            <label class="font-weight-bold">Estatus:</label>
            <asp:DropDownList ID="ddlFiltroEstatus" runat="server" CssClass="form-control">
                <asp:ListItem Value="0" Text="Todos" />
                <asp:ListItem Value="1" Text="Pendiente" />
                <asp:ListItem Value="2" Text="Aprobado" />
                <asp:ListItem Value="3" Text="Rechazado" />
            </asp:DropDownList>
        </div>
        <div class="col-md-2 d-flex align-items-end" style="gap:8px;">
            <asp:Button ID="btnFiltrar" runat="server" Text="Filtrar" CssClass="btn btn-primary" OnClick="btnFiltrar_Click" />
            <asp:Button ID="btnLimpiarFiltros" runat="server" Text="Limpiar" CssClass="btn btn-secondary" OnClick="btnLimpiarFiltros_Click" />
        </div>
    </div>

    <br />
    <p class="text-muted"><i class="fas fa-info-circle"></i> Capture las decisiones antes de cambiar de p&aacute;gina para no perder los cambios. Las horas extra se muestran redondeadas al bloque de 30 min completado.</p>

    <asp:HiddenField ID="hfTabActivo" runat="server" Value="automaticas" />

    <%-- Tabs --%>
    <ul class="nav nav-tabs" id="tabsHorasExtra" role="tablist">
        <li class="nav-item">
            <a class="nav-link" id="tab-automaticas" data-toggle="tab" href="#panelAutomaticas" role="tab">
                <i class="fas fa-robot"></i> Autom&aacute;ticas
            </a>
        </li>
        <li class="nav-item">
            <a class="nav-link" id="tab-manuales" data-toggle="tab" href="#panelManuales" role="tab">
                <i class="fas fa-user-edit"></i> Manuales
                <asp:Label ID="lblBadgeManuales" runat="server" CssClass="badge badge-warning ml-1" Text="" />
            </a>
        </li>
    </ul>

    <div class="tab-content border border-top-0 p-3 mb-3">

        <%-- TAB AUTOMÁTICAS --%>
        <div class="tab-pane fade" id="panelAutomaticas" role="tabpanel">
            <br />
    <div class="table-responsive">
        <asp:GridView ID="gvHorasExtra" runat="server"
            AutoGenerateColumns="False"
            CssClass="table table-bordered table-striped custom-grid"
            AllowPaging="True" PageSize="20"
            DataKeyNames="IdUsuario"
            OnPageIndexChanging="gvHorasExtra_PageIndexChanging"
            OnRowDataBound="gvHorasExtra_RowDataBound"
            OnRowCommand="gvHorasExtra_RowCommand">
            <Columns>
                <asp:BoundField DataField="Empleado" HeaderText="Empleado" />
                <asp:BoundField DataField="Planta" HeaderText="Planta" />
                <asp:BoundField DataField="TotalHorasFormato" HeaderText="Total Horas Extra" />
                <asp:BoundField DataField="TipoHorasExtra" HeaderText="Tipo" />
                <asp:BoundField DataField="EstatusTexto" HeaderText="Estatus Actual" />
                <asp:TemplateField HeaderText="Detalle">
                    <ItemTemplate>
                        <asp:LinkButton ID="lbVerDetalle" runat="server"
                            CssClass="btn btn-sm btn-outline-info"
                            CommandName="VerDetalle"
                            CommandArgument='<%# Eval("IdUsuario") %>'>
                            <i class="fas fa-eye"></i> Ver Detalle
                        </asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Motivo">
                    <ItemTemplate>
                        <asp:TextBox ID="txtMotivo" runat="server" CssClass="form-control form-control-sm" Width="200px" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Decisi&oacute;n">
                    <ItemTemplate>
                        <asp:DropDownList ID="ddlDecision" runat="server" CssClass="form-control form-control-sm">
                            <asp:ListItem Value="0" Text="-- Sin cambio --" />
                            <asp:ListItem Value="2" Text="Aprobar" />
                            <asp:ListItem Value="3" Text="Rechazar" />
                        </asp:DropDownList>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>

            <br />

            <asp:Button ID="btnEnviarRH" runat="server" Text="Guardar y Enviar a RH"
                CssClass="btn btn-success btn-lg"
                OnClientClick="return confirmarEnvio(this);"
                OnClick="btnEnviarRH_Click" />

            <asp:Button ID="btnImprimirActa" runat="server" Text="Imprimir Acta"
                CssClass="btn btn-info btn-lg"
                OnClientClick="imprimirActa(); return false;" />

            <asp:Button ID="btnImprimirResumen" runat="server" Text="Imprimir Acta Resumen"
                CssClass="btn btn-warning btn-lg"
                OnClientClick="imprimirActaResumen(); return false;" />

        </div><%-- /panelAutomaticas --%>

        <%-- TAB MANUALES --%>
        <div class="tab-pane fade" id="panelManuales" role="tabpanel">
            <br />
            <p class="text-muted small">
                <i class="fas fa-info-circle"></i>
                Registros de horas extra cargados manualmente (viajes, guardias, trabajo en campo, etc.).
                Apruebe o rechace cada registro y haga clic en <strong>Guardar decisiones</strong>.
            </p>
            <div class="table-responsive">
                <asp:GridView ID="gvHorasExtraManual" runat="server"
                    AutoGenerateColumns="False"
                    CssClass="table table-bordered table-striped custom-grid"
                    AllowPaging="True" PageSize="20"
                    DataKeyNames="IdUsuario"
                    OnPageIndexChanging="gvHorasExtraManual_PageIndexChanging"
                    OnRowDataBound="gvHorasExtraManual_RowDataBound"
                    OnRowCommand="gvHorasExtraManual_RowCommand"
                    EmptyDataText="No hay horas extra manuales en este periodo y planta.">
                    <Columns>
                        <asp:BoundField DataField="Empleado" HeaderText="Empleado" />
                        <asp:BoundField DataField="Planta" HeaderText="Planta" />
                        <asp:BoundField DataField="TotalHorasFormato" HeaderText="Total Horas Extra" />
                        <asp:BoundField DataField="TipoHorasExtra" HeaderText="Tipo" />
                        <asp:BoundField DataField="EstatusTexto" HeaderText="Estatus Actual" />
                        <asp:TemplateField HeaderText="Detalle">
                            <ItemTemplate>
                                <asp:LinkButton ID="lbVerDetalleManual" runat="server"
                                    CssClass="btn btn-sm btn-outline-info"
                                    CommandName="VerDetalleManual"
                                    CommandArgument='<%# Eval("IdUsuario") %>'>
                                    <i class="fas fa-eye"></i> Ver Detalle
                                </asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Motivo">
                            <ItemTemplate>
                                <asp:TextBox ID="txtMotivoManual" runat="server" CssClass="form-control form-control-sm" Width="200px" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Decisi&oacute;n">
                            <ItemTemplate>
                                <asp:DropDownList ID="ddlDecisionManual" runat="server" CssClass="form-control form-control-sm">
                                    <asp:ListItem Value="0" Text="-- Sin cambio --" />
                                    <asp:ListItem Value="2" Text="Aprobar" />
                                    <asp:ListItem Value="3" Text="Rechazar" />
                                </asp:DropDownList>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>

            <asp:Button ID="btnGuardarManuales" runat="server" Text="Guardar decisiones"
                CssClass="btn btn-success btn-lg"
                OnClick="btnGuardarManuales_Click" />
        </div><%-- /panelManuales --%>

    </div><%-- /tab-content --%>

    <asp:HiddenField ID="hfMostrarModal" runat="server" Value="0" />

    <%-- Modal Ver Detalle --%>
    <div class="modal fade" id="modalDetalle" tabindex="-1" role="dialog" aria-labelledby="modalDetalleLabel" aria-hidden="true">
        <div class="modal-dialog modal-lg" role="document">
            <div class="modal-content">
                <div class="modal-header bg-info text-white">
                    <h5 class="modal-title" id="modalDetalleLabel">
                        Detalle de Horas Extra &mdash; <asp:Literal ID="litNombreEmpleadoDetalle" runat="server" />
                    </h5>
                    <button type="button" class="close text-white" data-dismiss="modal" aria-label="Cerrar">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
                <div class="modal-body">
                    <asp:GridView ID="gvDetalle" runat="server"
                        AutoGenerateColumns="False"
                        CssClass="table table-bordered table-sm table-hover"
                        OnRowDataBound="gvDetalle_RowDataBound">
                        <Columns>
                            <asp:BoundField DataField="FechaFormato" HeaderText="Fecha" />
                            <asp:BoundField DataField="HorasFormato" HeaderText="Horas Extra (redondeadas)" />
                            <asp:BoundField DataField="Descripcion" HeaderText="Descripci&oacute;n" />
                            <asp:BoundField DataField="EstatusTexto" HeaderText="Estatus" />
                        </Columns>
                    </asp:GridView>
                    <asp:Label ID="lblTotalDetalle" runat="server" CssClass="font-weight-bold d-block text-right" />
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal">Cerrar</button>
                </div>
            </div>
        </div>
    </div>

    <script>
        window.addEventListener('load', function () {
            // Restaurar tab activo tras postback
            var tabActivo = document.getElementById('<%= hfTabActivo.ClientID %>').value;
            if (tabActivo === 'manuales') {
                $('#tab-manuales').tab('show');
            } else {
                $('#tab-automaticas').tab('show');
            }

            // Guardar tab seleccionado en el HiddenField antes de postback
            $('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
                var id = e.target.getAttribute('href').replace('#panel', '').toLowerCase();
                document.getElementById('<%= hfTabActivo.ClientID %>').value = id;
            });

            var hf = document.getElementById('<%= hfMostrarModal.ClientID %>');
            if (hf && hf.value === '1') {
                $('#modalDetalle').modal('show');
                hf.value = '0';
            }
        });

        function imprimirActa() {
            var fi = document.getElementById('<%= txtFechaInicio.ClientID %>').value;
            var ff = document.getElementById('<%= txtFechaFin.ClientID %>').value;
            var idAprobador = '<%= IdAprobadorActual %>';
            var ddlPlanta = document.getElementById('<%= ddlFiltroPlanta.ClientID %>');
            var idPlanta = ddlPlanta ? ddlPlanta.value : '0';
            window.open('ImprimirActaHorasExtra.aspx?idAprobador=' + idAprobador + '&fi=' + fi + '&ff=' + ff + '&idPlanta=' + idPlanta, '_blank');
        }

        function imprimirActaResumen() {
            var fi = document.getElementById('<%= txtFechaInicio.ClientID %>').value;
            var ff = document.getElementById('<%= txtFechaFin.ClientID %>').value;
            var idAprobador = '<%= IdAprobadorActual %>';
            var ddlPlanta = document.getElementById('<%= ddlFiltroPlanta.ClientID %>');
            var idPlanta = ddlPlanta ? ddlPlanta.value : '0';
            window.open('ImprimirActaResumenHorasExtra.aspx?idAprobador=' + idAprobador + '&fi=' + fi + '&ff=' + ff + '&idPlanta=' + idPlanta, '_blank');
        }

        function confirmarEnvio(btn) {
            Swal.fire({
                title: '\u00bfEnviar reporte a RH?',
                text: 'Se guardar\u00e1n todas las decisiones y se enviar\u00e1 un correo a Recursos Humanos.',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'S\u00ed, enviar',
                cancelButtonText: 'Cancelar'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack(btn.name, '');
                }
            });
            return false;
        }
    </script>
</asp:Content>
