<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="HistorialVacacionesRH.aspx.cs" Inherits="GrupoAnkhalAsistencia.HistorialVacacionesRH"
    ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta charset="utf-8" />
    <link href="css/gridviewPantalla.css" rel="stylesheet" />
    <script src="scriptspropios/sweetalert2@11.js"></script>
    <style>
        /* Paginado Bootstrap */
        .pager-row td {
            background-color: #fff !important;
            border: none !important;
            padding: 8px 0 !important;
        }
        .pager-row td table {
            margin: 0 auto;
        }
        .pager-row td table td {
            padding: 0 2px !important;
        }
        .pager-row td table td a,
        .pager-row td table td span {
            display: inline-block;
            padding: 5px 12px;
            border: 1px solid #dee2e6;
            border-radius: 4px;
            color: #003366;
            text-decoration: none;
            font-size: 14px;
            background-color: #fff;
        }
        .pager-row td table td span {
            background-color: #003366;
            color: #fff;
            border-color: #003366;
            font-weight: bold;
        }
        .pager-row td table td a:hover {
            background-color: #e9f0fb;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <h2>Historial de Decisiones RH / Administrador</h2>
    <p class="text-muted">
        <i class="fas fa-info-circle"></i>
        Registro de todas las solicitudes de vacaciones en las que RH o un Administrador tom&oacute; una decisi&oacute;n final.
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
            OnPageIndexChanging="gvHistorial_PageIndexChanging"
            EmptyDataText="No hay registros de decisiones RH.">
            <PagerStyle CssClass="pager-row" HorizontalAlign="Center" />
            <Columns>
                <asp:BoundField DataField="Empleado" HeaderText="Empleado" />
                <asp:BoundField DataField="Planta" HeaderText="Planta" />
                <asp:BoundField DataField="FechaInicio" HeaderText="Fecha Inicio" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:BoundField DataField="FechaFin" HeaderText="Fecha Fin" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:BoundField DataField="Dias" HeaderText="D&iacute;as" />
                <asp:BoundField DataField="FechaSolicitud" HeaderText="Solicitado el" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                <asp:BoundField DataField="Decision" HeaderText="Decisi&oacute;n RH" />
                <asp:BoundField DataField="DecididoPor" HeaderText="Decidido por" />
                <asp:BoundField DataField="FechaDecision" HeaderText="Fecha de decisi&oacute;n" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                <asp:BoundField DataField="DecisionJefe" HeaderText="Decisi&oacute;n del Jefe" />
                <asp:BoundField DataField="MotivoJefe" HeaderText="Motivo del Jefe" />
            </Columns>
        </asp:GridView>
    </div>

</asp:Content>
