<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ReporteHorasExtraRH.aspx.cs" Inherits="GrupoAnkhalAsistencia.ReporteHorasExtraRH" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta charset="utf-8" />
    <link href="css/gridviewPantalla.css" rel="stylesheet" />
    <script src="scriptspropios/sweetalert2@11.js"></script>
    <script src="scriptspropios/propios.js"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h2>Reporte de Horas Extra &mdash; Recursos Humanos</h2>
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
            <asp:TextBox ID="txtEmpleado" runat="server" CssClass="form-control" Placeholder="Nombre..." />
        </div>
        <div class="col-md-2">
            <label>Estatus:</label>
            <asp:DropDownList ID="ddlEstatus" runat="server" CssClass="form-control">
                <asp:ListItem Value="0" Text="-- Todos --" />
                <asp:ListItem Value="1" Text="Pendiente" />
                <asp:ListItem Value="2" Text="Aprobado" />
                <asp:ListItem Value="3" Text="Rechazado" />
            </asp:DropDownList>
        </div>
        <div class="col-md-2">
            <label>Planta:</label>
            <asp:DropDownList ID="ddlPlanta" runat="server" CssClass="form-control" />
        </div>
        <div class="col-md-2 d-flex align-items-end" style="gap:6px;">
            <asp:Button ID="btnFiltrar" runat="server" Text="Filtrar" CssClass="btn btn-primary" OnClick="btnFiltrar_Click" />
            <asp:Button ID="btnLimpiarFiltros" runat="server" Text="Limpiar" CssClass="btn btn-secondary" OnClick="btnLimpiarFiltros_Click" />
        </div>
    </div>

    <div class="row mt-2">
        <div class="col-md-12" style="display:flex; gap:8px;">
            <asp:Button ID="btnExportExcel" runat="server" Text="Exportar a Excel (Detalle)"
                CssClass="btn btn-success" OnClick="btnExportExcel_Click" />
            <asp:Button ID="btnExportExcelResumen" runat="server" Text="Exportar a Excel (Resumen)"
                CssClass="btn btn-info" OnClick="btnExportExcelResumen_Click" />
        </div>
    </div>

    <br />

    <div class="table-responsive">
        <asp:GridView ID="gvReporteRH" runat="server"
            AutoGenerateColumns="False"
            CssClass="table table-bordered table-striped custom-grid"
            AllowPaging="True" PageSize="15"
            OnPageIndexChanging="gvReporteRH_PageIndexChanging">
            <Columns>
                <asp:BoundField DataField="Empleado" HeaderText="Empleado" />
                <asp:BoundField DataField="Planta" HeaderText="Planta" />
                <asp:BoundField DataField="Fecha" HeaderText="Fecha" />
                <asp:BoundField DataField="HorasExtraFormato" HeaderText="Horas Extra" />
                <asp:BoundField DataField="TipoHorasExtra" HeaderText="Tipo" />
                <asp:BoundField DataField="Descripcion" HeaderText="Descripci&oacute;n" />
                <asp:BoundField DataField="Motivo" HeaderText="Motivo Aprobaci&oacute;n" />
                <asp:BoundField DataField="EstatusTexto" HeaderText="Estatus" />
                <asp:BoundField DataField="Origen" HeaderText="Origen" />
                <asp:BoundField DataField="Aprobador" HeaderText="Jefe Aprobador" />
                <asp:BoundField DataField="FechaAprobacion" HeaderText="Fecha Aprobaci&oacute;n" />
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>
