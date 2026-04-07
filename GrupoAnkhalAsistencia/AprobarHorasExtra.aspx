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
    <p class="text-muted"><i class="fas fa-info-circle"></i> Capture las decisiones de esta p&aacute;gina antes de cambiar de p&aacute;gina para no perder los cambios.</p>

    <div class="table-responsive">
        <asp:GridView ID="gvHorasExtra" runat="server"
            AutoGenerateColumns="False"
            CssClass="table table-bordered table-striped custom-grid"
            AllowPaging="True" PageSize="20"
            DataKeyNames="IdAsistencia"
            OnPageIndexChanging="gvHorasExtra_PageIndexChanging"
            OnRowDataBound="gvHorasExtra_RowDataBound">
            <Columns>
                <asp:BoundField DataField="Empleado" HeaderText="Empleado" />
                <asp:BoundField DataField="Planta" HeaderText="Planta" />
                <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:BoundField DataField="HorasExtraFormato" HeaderText="Horas Extra" />
                <asp:BoundField DataField="TipoHorasExtra" HeaderText="Tipo" />
                <asp:BoundField DataField="EstatusTexto" HeaderText="Estatus Actual" />
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

    <script>
        function imprimirActa() {
            var fi = document.getElementById('<%= txtFechaInicio.ClientID %>').value;
            var ff = document.getElementById('<%= txtFechaFin.ClientID %>').value;
            var idAprobador = '<%= IdAprobadorActual %>';
            var ddlPlanta = document.getElementById('<%= ddlFiltroPlanta.ClientID %>');
            var idPlanta = ddlPlanta ? ddlPlanta.value : '0';
            window.open('ImprimirActaHorasExtra.aspx?idAprobador=' + idAprobador + '&fi=' + fi + '&ff=' + ff + '&idPlanta=' + idPlanta, '_blank');
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
