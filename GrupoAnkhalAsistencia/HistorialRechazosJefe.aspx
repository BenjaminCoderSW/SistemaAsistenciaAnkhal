<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="HistorialRechazosJefe.aspx.cs" Inherits="GrupoAnkhalAsistencia.HistorialRechazosJefe" ResponseEncoding="utf-8" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta charset="utf-8" />
    <link href="css/gridviewPantalla.css" rel="stylesheet" />
    <script src="scriptspropios/sweetalert2@11.js"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h2>Rechazos de Vacaciones por Jefe de Planta</h2>
    <p class="text-muted">
        <i class="fas fa-info-circle"></i>
        Historial de solicitudes que fueron rechazadas por un Jefe de Planta, junto con la decisi&oacute;n final de RH.
    </p>
    <br />

    <div class="row mb-3">
        <div class="col-md-4">
            <asp:TextBox ID="txtBuscar" runat="server"
                CssClass="form-control"
                Placeholder="Buscar empleado..."
                AutoPostBack="true"
                OnTextChanged="txtBuscar_TextChanged" />
        </div>
    </div>

    <div class="table-responsive">
        <asp:GridView ID="gvHistorial" runat="server"
            AutoGenerateColumns="False"
            CssClass="table table-bordered table-striped custom-grid"
            AllowPaging="True" PageSize="15"
            OnPageIndexChanging="gvHistorial_PageIndexChanging">
            <Columns>
                <asp:BoundField DataField="Empleado" HeaderText="Empleado" />
                <asp:BoundField DataField="Planta" HeaderText="Planta" />
                <asp:BoundField DataField="FechaInicio" HeaderText="Fecha Inicio" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:BoundField DataField="FechaFin" HeaderText="Fecha Fin" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:BoundField DataField="Dias" HeaderText="D&iacute;as" />
                <asp:BoundField DataField="FechaSolicitud" HeaderText="Solicitado el" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                <asp:BoundField DataField="JefePlanta" HeaderText="Jefe de Planta" />
                <asp:BoundField DataField="FechaRechazoJefe" HeaderText="Rechazado el" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                <asp:BoundField DataField="MotivoJefe" HeaderText="Motivo del Jefe" />
                <asp:BoundField DataField="DecisionFinalRH" HeaderText="Decisi&oacute;n Final RH" />
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>
