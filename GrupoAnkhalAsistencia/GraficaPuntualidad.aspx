<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="GraficaPuntualidad.aspx.cs" Inherits="GrupoAnkhalAsistencia.GraficaPuntualidad" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mt-4">
    <h3 class="text-center">Top 5 Empleados Más Puntuales</h3>

    <div class="row mb-3">
        <div class="col-md-4">
            <label>Fecha inicio</label>
            <asp:TextBox ID="txtFechaInicio" CssClass="form-control" TextMode="Date" runat="server"></asp:TextBox>
        </div>

        <div class="col-md-4">
            <label>Fecha fin</label>
            <asp:TextBox ID="txtFechaFin" CssClass="form-control" TextMode="Date" runat="server"></asp:TextBox>
        </div>

        <div class="col-md-4 d-flex align-items-end">
            <asp:Button ID="btnBuscar" CssClass="btn btn-primary w-100" Text="Generar" OnClick="btnBuscar_Click" runat="server" />
        </div>
    </div>

    <canvas id="graficaPuntualidad" style="max-height:400px;"></canvas>

    <asp:Literal ID="ltScript" runat="server"></asp:Literal>
</div>

</asp:Content>
