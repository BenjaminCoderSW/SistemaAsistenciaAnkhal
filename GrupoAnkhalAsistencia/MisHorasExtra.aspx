<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MisHorasExtra.aspx.cs" Inherits="GrupoAnkhalAsistencia.MisHorasExtra" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="css/gridviewPantalla.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="card card-primary card-outline">
        <div class="card-header">
            <h3 class="card-title">
                <i class="fas fa-clock"></i> Mis Horas Extra Aprobadas
            </h3>
        </div>
        <div class="card-body">

            <!-- Filtros -->
            <div class="row mb-3">
                <div class="col-md-3">
                    <label>Fecha Inicio</label>
                    <asp:TextBox ID="txtFechaInicio" runat="server" CssClass="form-control" TextMode="Date" />
                </div>
                <div class="col-md-3">
                    <label>Fecha Fin</label>
                    <asp:TextBox ID="txtFechaFin" runat="server" CssClass="form-control" TextMode="Date" />
                </div>
                <div class="col-md-3 d-flex align-items-end">
                    <asp:Button ID="btnFiltrar" runat="server" Text="Filtrar"
                        CssClass="btn btn-primary mr-2" OnClick="btnFiltrar_Click" />
                    <asp:Button ID="btnLimpiarFiltros" runat="server" Text="Limpiar"
                        CssClass="btn btn-secondary" OnClick="btnLimpiarFiltros_Click" />
                </div>
            </div>

            <!-- Total del periodo -->
            <div class="alert alert-success">
                <i class="fas fa-check-circle"></i>
                <asp:Label ID="lblTotalAprobadas" runat="server" Text="Total aprobadas en el periodo: 00:00 hrs" />
            </div>

            <!-- Tabla -->
            <div class="table-responsive">
                <asp:GridView ID="gvMisHorasExtra" runat="server"
                    AutoGenerateColumns="False"
                    CssClass="table table-bordered table-striped custom-grid"
                    AllowPaging="True"
                    PageSize="10"
                    OnPageIndexChanging="gvMisHorasExtra_PageIndexChanging"
                    EmptyDataText="No tienes horas extra aprobadas en este periodo.">
                    <Columns>
                        <asp:BoundField DataField="Fecha" HeaderText="Fecha" />
                        <asp:BoundField DataField="HorasExtra" HeaderText="Horas Extra" />
                        <asp:BoundField DataField="Descripcion" HeaderText="Descripci&oacute;n" />
                        <asp:BoundField DataField="AprobadoPor" HeaderText="Aprobado por" />
                        <asp:BoundField DataField="FechaAprobacion" HeaderText="Fecha Aprobaci&oacute;n" />
                        <asp:TemplateField HeaderText="Origen">
                            <ItemTemplate>
                                <span class='<%# (string)Eval("Origen") == "Manual"
                                    ? "badge badge-info" : "badge badge-secondary" %>'>
                                    <%# Eval("Origen") %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Estatus">
                            <ItemTemplate>
                                <span class="badge badge-success">Aprobado</span>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <RowStyle CssClass="table-success" />
                </asp:GridView>
            </div>

        </div>
    </div>

</asp:Content>
