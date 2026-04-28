<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="PrincipalAdmin.aspx.cs"
    Inherits="GrupoAnkhalAsistencia.PrincipalAdmin" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .card-info {
            padding: 25px;
            border-radius: 10px;
            color: #fff;
            margin-bottom: 20px;
            text-align: center;
            font-size: 22px;
            font-weight: bold;
            cursor: pointer;
            transition: all 0.3s ease;
        }

        .card-info:hover {
            transform: translateY(-5px);
            box-shadow: 0 8px 20px rgba(0,0,0,0.2);
        }

        .card-info.active {
            border: 4px solid #fff;
            box-shadow: 0 0 20px rgba(255,255,255,0.5);
        }

        .bg-total      { background: #0d6efd; }
        .bg-tiempo     { background: #198754; }
        .bg-tarde      { background: #ffc107; color: black; }
        .bg-vacaciones { background: #0dcaf0; color: #000; }
        .bg-faltaron   { background: #dc3545; }

        .table-container { margin-top: 30px; }
        .search-box      { margin-bottom: 20px; }

        .filter-info {
            background: #f8f9fa;
            padding: 15px;
            border-radius: 8px;
            margin-bottom: 20px;
            border-left: 4px solid #0d6efd;
        }

        .filter-info h5 {
            margin: 0;
            color: #0d6efd;
            font-weight: 600;
        }

        .btn-clear-filter { margin-left: 10px; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="container-fluid mt-4">

        <h2 class="mb-4" id="hTituloAsistencia" runat="server">Resumen de Asistencia (Hoy)</h2>

        <!-- CARDS -->
        <div class="row">

            <div class="col-xl-2 col-lg-4 col-md-6 mb-3">
                <asp:LinkButton ID="lnkTotalEmpleados" runat="server"
                    OnClick="lnkTotalEmpleados_Click" CssClass="text-decoration-none">
                    <div class="card-info bg-total shadow" id="cardTotal">
                        Total empleados<br />
                        <asp:Label ID="lblTotalEmpleados" runat="server" Text="0"></asp:Label>
                    </div>
                </asp:LinkButton>
            </div>

            <div class="col-xl-2 col-lg-4 col-md-6 mb-3">
                <asp:LinkButton ID="lnkLlegaronTiempo" runat="server"
                    OnClick="lnkLlegaronTiempo_Click" CssClass="text-decoration-none">
                    <div class="card-info bg-tiempo shadow" id="cardTiempo">
                        Llegaron a tiempo<br />
                        <asp:Label ID="lblLlegaronTiempo" runat="server" Text="0"></asp:Label>
                    </div>
                </asp:LinkButton>
            </div>

            <div class="col-xl-2 col-lg-4 col-md-6 mb-3">
                <asp:LinkButton ID="lnkLlegaronTarde" runat="server"
                    OnClick="lnkLlegaronTarde_Click" CssClass="text-decoration-none">
                    <div class="card-info bg-tarde shadow" id="cardTarde">
                        Llegaron tarde<br />
                        <asp:Label ID="lblLlegaronTarde" runat="server" Text="0"></asp:Label>
                    </div>
                </asp:LinkButton>
            </div>

            <div class="col-xl-2 col-lg-4 col-md-6 mb-3">
                <asp:LinkButton ID="lnkVacaciones" runat="server"
                    OnClick="lnkVacaciones_Click" CssClass="text-decoration-none">
                    <div class="card-info bg-vacaciones shadow" id="cardVacaciones">
                        Total Vacaciones<br />
                        <asp:Label ID="lblVacaciones" runat="server" Text="0"></asp:Label>
                    </div>
                </asp:LinkButton>
            </div>

            <div class="col-xl-2 col-lg-4 col-md-6 mb-3">
                <asp:LinkButton ID="lnkFaltaron" runat="server"
                    OnClick="lnkFaltaron_Click" CssClass="text-decoration-none">
                    <div class="card-info bg-faltaron shadow" id="cardFaltaron">
                        Sin registro<br />
                        <asp:Label ID="lblFaltaron" runat="server" Text="0"></asp:Label>
                    </div>
                </asp:LinkButton>
            </div>

        </div>

        <!-- SELECTOR DE FECHA -->
        <div class="row mb-3 align-items-center">
            <div class="col-auto">
                <label class="mb-0 font-weight-bold">Fecha:</label>
            </div>
            <div class="col-auto">
                <asp:TextBox ID="txtFecha" runat="server" TextMode="Date"
                    CssClass="form-control form-control-sm"
                    style="max-width:160px;" />
            </div>
            <div class="col-auto">
                <asp:Button ID="btnVerFecha" runat="server" Text="Ver asistencia"
                    CssClass="btn btn-primary btn-sm"
                    OnClick="btnVerFecha_Click" />
            </div>
        </div>

        <!-- INFORMACIÓN DEL FILTRO ACTIVO -->
        <asp:Panel ID="pnlFiltroActivo" runat="server" Visible="false" CssClass="filter-info">
            <div class="d-flex justify-content-between align-items-center">
                <div>
                    <h5>
                        <i class="fas fa-filter"></i>
                        Filtro activo: <asp:Label ID="lblFiltroActivo" runat="server" Text=""></asp:Label>
                    </h5>
                </div>
                <div>
                    <asp:Button ID="btnLimpiarFiltro" runat="server"
                        Text="Mostrar todos"
                        CssClass="btn btn-sm btn-outline-primary btn-clear-filter"
                        OnClick="btnLimpiarFiltro_Click" />
                </div>
            </div>
        </asp:Panel>

        <!-- TABLA -->
        <div class="table-container card shadow p-3">

            <div class="search-box row">
                <div class="col-md-4">
                    <asp:TextBox ID="txtBuscar" runat="server"
                        CssClass="form-control"
                        Placeholder="Buscar empleado..."
                        AutoPostBack="true"
                        OnTextChanged="txtBuscar_TextChanged" />
                </div>
                <div class="col-md-8 text-right">
                    <small class="text-muted">
                        Mostrando <asp:Label ID="lblTotalRegistros" runat="server" Text="0"></asp:Label> registros
                    </small>
                </div>
            </div>

            <div class="table-responsive">
                <asp:GridView ID="gvAsistenciaHoy" runat="server"
                    CssClass="table table-bordered table-striped"
                    AutoGenerateColumns="False"
                    AllowPaging="True"
                    PageSize="10"
                    OnPageIndexChanging="gvAsistenciaHoy_PageIndexChanging"
                    EmptyDataText="No hay registros para mostrar con el filtro seleccionado.">

                    <Columns>
                        <asp:BoundField DataField="Empleado"        HeaderText="Empleado" />
                        <asp:BoundField DataField="Planta"          HeaderText="Planta" />
                        <asp:BoundField DataField="Fecha"           HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                        <asp:BoundField DataField="HoraEntrada"     HeaderText="HoraEntrada" />
                        <asp:BoundField DataField="HoraSalidaComer" HeaderText="HoraSalidaComer" />
                        <asp:BoundField DataField="HoraEntradaComer" HeaderText="HoraEntradaComer" />
                        <asp:BoundField DataField="HoraSalida"      HeaderText="HoraSalida" />
                        <asp:BoundField DataField="EstatusEntrada"  HeaderText="EstatusEntrada" />
                        <asp:BoundField DataField="EstatusComida"   HeaderText="EstatusComida" />
                        <asp:BoundField DataField="EstatusSalida"   HeaderText="EstatusSalida" />
                        <asp:TemplateField HeaderText="Ubicacion Entrada">
                            <ItemTemplate>
                                <%# GetMapaLink(Eval("UbicacionEntrada")?.ToString()) %>
                                <%# GetPlantaHtml(Eval("UbicacionEntrada")?.ToString()) %>
                                <%# GetSinGpsHtml(Eval("UbicacionEntrada")?.ToString(), Eval("EstatusEntrada")?.ToString()) %>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Ubicacion Salida">
                            <ItemTemplate>
                                <%# GetMapaLink(Eval("UbicacionSalida")?.ToString()) %>
                                <%# GetPlantaHtml(Eval("UbicacionSalida")?.ToString()) %>
                                <%# GetSinGpsHtml(Eval("UbicacionSalida")?.ToString(), Eval("EstatusSalida")?.ToString()) %>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>

                    <PagerSettings Mode="NumericFirstLast" PageButtonCount="5"
                                   FirstPageText="Primera" LastPageText="Ultima" />
                    <PagerStyle CssClass="pagination-ys" />

                </asp:GridView>
            </div>
        </div>

    </div>

    <asp:Literal ID="ltScriptCard" runat="server"></asp:Literal>

</asp:Content>
