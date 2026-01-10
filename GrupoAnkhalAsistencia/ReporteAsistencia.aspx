<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="ReporteAsistencia.aspx.cs" Inherits="GrupoAnkhalAsistencia.ReporteAsistencia" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="css/gridviewPantalla.css" rel="stylesheet" />
     <script src="scriptspropios/sweetalert2@11.js"></script>
  <script src="scriptspropios/propios.js"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <h2>Reporte Asistencia</h2>

    <br />

    <!-- FILTROS -->
    <div class="row">

        <div class="col-md-3">
            <asp:TextBox ID="txtFechaInicio" runat="server" CssClass="form-control"
                TextMode="Date" />
        </div>

        <div class="col-md-3">
            <asp:TextBox ID="txtFechaFin" runat="server" CssClass="form-control"
                TextMode="Date" />
        </div>

        <div class="col-md-3">
            <asp:TextBox ID="txtEmpleado" runat="server" CssClass="form-control"
                Placeholder="Empleado..." />
        </div>

        <div class="col-md-3">
            <asp:Button ID="btnFiltrar" runat="server" CssClass="btn btn-primary"
                Text="Filtrar" OnClick="btnFiltrar_Click" />
        </div>

    </div>

    <br />

    <!-- BUSCADOR ORIGINAL -->
    <div class="col-md-6">

       <%-- <asp:TextBox ID="txtBuscar" runat="server"
            CssClass="form-control"
            Placeholder="Buscar Empleado..."
            AutoPostBack="true"
            OnTextChanged="txtBuscar_TextChanged" />--%>

        <br /><br />

        <asp:Button ID="btnExportExcel" runat="server" Text="Exportar a Excel"
            CssClass="btn btn-success" OnClick="btnExportExcel_Click" />

        <asp:Button ID="btnExportPDF" runat="server" Text="Exportar a PDF"
            CssClass="btn btn-danger" OnClick="btnExportPDF_Click" />

        <br /><br />

    </div>

    <br />

    <!-- GRID -->
    <div class="table-responsive">
        <asp:GridView ID="dvgHistorialEmpleado" runat="server" AutoGenerateColumns="False"
            CssClass="table table-bordered table-striped custom-grid"
            AllowPaging="True" PageSize="5"
            OnPageIndexChanging="dvgHistorialEmpleado_PageIndexChanging">

            <Columns>

                <asp:BoundField DataField="EMPLEADO" HeaderText="EMPLEADO" />
                <asp:BoundField DataField="Planta" HeaderText="Planta" />
                <asp:BoundField DataField="Fecha" HeaderText="Fecha" />
                <asp:BoundField DataField="HoraEntrada" HeaderText="HoraEntrada" />
                <asp:BoundField DataField="HoraSalida" HeaderText="HoraSalida" />
                <asp:BoundField DataField="HoraSalidaComer" HeaderText="HoraSalidaComer" />
                <asp:BoundField DataField="HoraEntradaComer" HeaderText="HoraEntradaComer" />
                <asp:BoundField DataField="HorasTrabajadas" HeaderText="HorasTrabajadas" />
                <asp:BoundField DataField="tiempoComida" HeaderText="Tiempo Comida" />

                <asp:BoundField DataField="EstatusEntrada" HeaderText="EstatusEntrada" />
                <asp:BoundField DataField="EstatusSalida" HeaderText="EstatusSalida" />
                <asp:BoundField DataField="EstatusComida" HeaderText="EstatusComida" />

                <asp:BoundField DataField="TipoPermiso" HeaderText="TipoPermiso" />
                <asp:BoundField DataField="HoraSalidaPermiso" HeaderText="HoraSalidaPermiso" />
                <asp:BoundField DataField="HoraEntradaPermiso" HeaderText="HoraEntradaPermiso" />
                <asp:BoundField DataField="HorasPermiso" HeaderText="HorasPermiso" />

                <asp:BoundField DataField="horaSalidaComision" HeaderText="horaSalidaComision" />
                <asp:BoundField DataField="horaEntradaComision" HeaderText="horaEntradaComision" />
                <asp:BoundField DataField="horasComision" HeaderText="horasComision" />

              
                <asp:TemplateField HeaderText="Ubicación Entrada">
                    <ItemTemplate>
                        <%# GetMapaLink(Eval("UbicacionEntrada").ToString()) %>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Ubicación Salida">
                    <ItemTemplate>
                        <%# GetMapaLink(Eval("UbicacionSalida").ToString()) %>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:BoundField DataField="MacEntrada" HeaderText="MacEntrada" />
                <asp:BoundField DataField="MacSalida" HeaderText="MacSalida" />
                <asp:BoundField DataField="IP" HeaderText="IP" />

            </Columns>

        </asp:GridView>
    </div>

</asp:Content>
