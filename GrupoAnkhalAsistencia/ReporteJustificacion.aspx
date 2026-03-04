<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ReporteJustificacion.aspx.cs" Inherits="GrupoAnkhalAsistencia.ReporteJustificacion" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="css/gridviewPantalla.css" rel="stylesheet" />
    <script src="scriptspropios/sweetalert2@11.js"></script>
    <style>
        .badge-pendiente { background-color: #ffc107; color: #000; padding: 3px 8px; border-radius: 4px; font-size: 12px; }
        .badge-aceptada  { background-color: #28a745; color: #fff; padding: 3px 8px; border-radius: 4px; font-size: 12px; }
        .badge-rechazada { background-color: #dc3545; color: #fff; padding: 3px 8px; border-radius: 4px; font-size: 12px; }
        .badge-retardo   { background-color: #ffc107; color: #000; padding: 3px 8px; border-radius: 4px; font-size: 12px; }
        .badge-falta     { background-color: #343a40; color: #fff; padding: 3px 8px; border-radius: 4px; font-size: 12px; }
        .card-filtros    { background: #fff; border-radius: 10px; padding: 20px; box-shadow: 0 2px 8px rgba(0,0,0,0.08); margin-bottom: 20px; }
        .section-title   { color: #003366; font-weight: 700; border-left: 4px solid #003366; padding-left: 10px; margin-bottom: 16px; }
        .resumen-card    { border-radius: 10px; padding: 16px 20px; color: #fff; text-align: center; }
        .resumen-num     { font-size: 32px; font-weight: 700; line-height: 1; }
        .resumen-lbl     { font-size: 13px; opacity: .9; margin-top: 4px; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <h2 class="section-title">Reporte de Justificaciones</h2>

    <!-- Tarjetas de resumen -->
    <div class="row mb-4">
        <div class="col-md-3">
            <div class="resumen-card" style="background:#ffc107;">
                <div class="resumen-num"><asp:Label ID="lblTotalPendientes" runat="server" Text="0" /></div>
                <div class="resumen-lbl">Pendientes</div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="resumen-card" style="background:#28a745;">
                <div class="resumen-num"><asp:Label ID="lblTotalAceptadas" runat="server" Text="0" /></div>
                <div class="resumen-lbl">Aceptadas</div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="resumen-card" style="background:#dc3545;">
                <div class="resumen-num"><asp:Label ID="lblTotalRechazadas" runat="server" Text="0" /></div>
                <div class="resumen-lbl">Rechazadas</div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="resumen-card" style="background:#003366;">
                <div class="resumen-num"><asp:Label ID="lblTotal" runat="server" Text="0" /></div>
                <div class="resumen-lbl">Total</div>
            </div>
        </div>
    </div>

    <!-- Filtros -->
    <div class="card-filtros">
        <div class="row align-items-end">
            <div class="col-md-3">
                <label class="form-label fw-semibold">Buscar Empleado</label>
                <asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control"
                    Placeholder="Nombre del empleado..." />
            </div>
            <div class="col-md-2">
                <label class="form-label fw-semibold">Fecha Inicio</label>
                <asp:TextBox ID="txtFechaInicio" runat="server" CssClass="form-control" TextMode="Date" />
            </div>
            <div class="col-md-2">
                <label class="form-label fw-semibold">Fecha Fin</label>
                <asp:TextBox ID="txtFechaFin" runat="server" CssClass="form-control" TextMode="Date" />
            </div>
            <div class="col-md-2">
                <label class="form-label fw-semibold">Estatus</label>
                <asp:DropDownList ID="ddlEstatus" runat="server" CssClass="form-control">
                    <asp:ListItem Value="0">Todos</asp:ListItem>
                    <asp:ListItem Value="1">Pendientes</asp:ListItem>
                    <asp:ListItem Value="2">Aceptadas</asp:ListItem>
                    <asp:ListItem Value="3">Rechazadas</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="col-md-2">
                <label class="form-label fw-semibold">Tipo</label>
                <asp:DropDownList ID="ddlTipo" runat="server" CssClass="form-control">
                    <asp:ListItem Value="">Todos</asp:ListItem>
                    <asp:ListItem Value="Retardo">Retardos</asp:ListItem>
                    <asp:ListItem Value="Falta">Faltas</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="col-md-1 mt-3">
                <asp:Button ID="btnBuscar" runat="server" Text="Buscar"
                    CssClass="btn btn-success w-100"
                    OnClick="btnBuscar_Click" />
            </div>
        </div>
    </div>

    <!-- Tabla de resultados -->
    <div class="table-responsive">
        <asp:GridView ID="dvgReporte" runat="server" AutoGenerateColumns="False"
            CssClass="table table-bordered table-striped custom-grid"
            AllowPaging="True" PageSize="15"
            OnPageIndexChanging="dvgReporte_PageIndexChanging"
            EmptyDataText="No hay registros con los filtros seleccionados.">
            <Columns>
                <asp:BoundField DataField="NombreCompleto"     HeaderText="Empleado" />
                <asp:BoundField DataField="NumeroEmpleado"     HeaderText="N° Empleado" />
                <asp:BoundField DataField="Planta"             HeaderText="Planta" />
                <asp:BoundField DataField="FechaAsistencia"    HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:TemplateField HeaderText="Tipo">
                    <ItemTemplate>
                        <span class='<%# Eval("TipoRegistro").ToString() == "Retardo" ? "badge-retardo" : "badge-falta" %>'>
                            <%# Eval("TipoRegistro") %>
                        </span>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="HoraEntrada"        HeaderText="Hora Entrada" />
                <asp:BoundField DataField="HoraProgramada"     HeaderText="Hora Programada" />
                <asp:BoundField DataField="Motivo"             HeaderText="Motivo" />
                <asp:BoundField DataField="Observaciones"      HeaderText="Comentarios" />
                <asp:BoundField DataField="FechaSolicitud"     HeaderText="Fecha Solicitud" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:TemplateField HeaderText="Estatus">
                    <ItemTemplate>
                        <%# ObtenerBadge(Eval("Estatus").ToString()) %>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>

</asp:Content>
