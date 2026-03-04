<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Justificacion.aspx.cs" Inherits="GrupoAnkhalAsistencia.Justificacion" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="css/gridviewPantalla.css" rel="stylesheet" />
    <script src="scriptspropios/sweetalert2@11.js"></script>
    <script src="scriptspropios/propios.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <style>
        .badge-retardo   { background-color: #ffc107; color: #000; padding: 4px 8px; border-radius: 4px; font-size: 12px; }
        .badge-falta     { background-color: #dc3545; color: #fff; padding: 4px 8px; border-radius: 4px; font-size: 12px; }
        .badge-pendiente { background-color: #6c757d; color: #fff; padding: 4px 8px; border-radius: 4px; font-size: 12px; }
        .badge-aceptada  { background-color: #28a745; color: #fff; padding: 4px 8px; border-radius: 4px; font-size: 12px; }
        .badge-rechazada { background-color: #dc3545; color: #fff; padding: 4px 8px; border-radius: 4px; font-size: 12px; }
        .card-filtros    { background: #fff; border-radius: 10px; padding: 20px; box-shadow: 0 2px 8px rgba(0,0,0,0.08); margin-bottom: 20px; }
        .section-title   { color: #003366; font-weight: 700; border-left: 4px solid #003366; padding-left: 10px; margin-bottom: 16px; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <h2 class="section-title">Mis Retardos y Faltas</h2>

    <!-- Filtros -->
    <div class="card-filtros">
        <div class="row align-items-end">
            <div class="col-md-3">
                <label class="form-label fw-semibold">Fecha Inicio</label>
                <asp:TextBox ID="txtFechaInicio" runat="server" CssClass="form-control" TextMode="Date" />
            </div>
            <div class="col-md-3">
                <label class="form-label fw-semibold">Fecha Fin</label>
                <asp:TextBox ID="txtFechaFin" runat="server" CssClass="form-control" TextMode="Date" />
            </div>
            <div class="col-md-3">
                <label class="form-label fw-semibold">Tipo</label>
                <asp:DropDownList ID="ddlTipoFiltro" runat="server" CssClass="form-control">
                    <asp:ListItem Value="">-- Todos --</asp:ListItem>
                    <asp:ListItem Value="Retardo">Retardos</asp:ListItem>
                    <asp:ListItem Value="Falta">Faltas</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="col-md-3">
                <asp:Button ID="btnBuscar" runat="server" Text="🔍 Buscar"
                    CssClass="btn btn-success btn-block w-100"
                    OnClick="btnBuscar_Click" />
            </div>
        </div>
    </div>

    <!-- Tabla de registros -->
    <div class="table-responsive">
        <asp:GridView ID="dvgJustificaionHoras" runat="server" AutoGenerateColumns="False"
            CssClass="table table-bordered table-striped custom-grid"
            AllowPaging="True" PageSize="10"
            OnPageIndexChanging="dvgJustificaion_PageIndexChanging"
            EmptyDataText="No se encontraron retardos o faltas en el período seleccionado.">
            <Columns>
                <asp:BoundField DataField="Fecha"        HeaderText="Fecha"       DataFormatString="{0:dd/MM/yyyy}" />
                <asp:BoundField DataField="Horario"      HeaderText="Horario" />
                <asp:BoundField DataField="HoraEntrada"  HeaderText="Hora Entrada" />
                <asp:BoundField DataField="HoraSalida"   HeaderText="Hora Salida" />
                <asp:TemplateField HeaderText="Tipo">
                    <ItemTemplate>
                        <span class='<%# Eval("EstatusEntrada").ToString() == "Retardo" ? "badge-retardo" : "badge-falta" %>'>
                            <%# Eval("EstatusEntrada") %>
                        </span>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Justificación">
                    <ItemTemplate>
                        <%# ObtenerBadgeJustificacion(Eval("Justificacion"), Eval("EstatusJustificacion")) %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="MotivoJustificacion" HeaderText="Motivo enviado" />
                <asp:TemplateField HeaderText="Acciones">
                    <ItemTemplate>
                        <asp:Button ID="btnJustificar"
                            runat="server"
                            Text="Solicitar Justificación"
                            CssClass="btn btn-primary btn-sm"
                            CommandArgument='<%# Eval("IdAsistencia") %>'
                            OnClick="btnJustificar_Click"
                            Visible='<%# PuedeJustificar(Eval("Justificacion"), Eval("EstatusJustificacion")) %>' />
                        <asp:Label runat="server"
                            Text="—"
                            Visible='<%# !PuedeJustificar(Eval("Justificacion"), Eval("EstatusJustificacion")) %>'
                            CssClass="text-muted" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>

    <!-- Modal para solicitar justificación -->
    <div class="modal fade" id="modalJustificacion" tabindex="-1">
        <div class="modal-dialog modal-dialog-centered modal-lg">
            <div class="modal-content shadow-lg border-0" style="border-radius: 18px;">

                <div class="modal-header" style="background: #1f2c3e; color:white; border-radius: 18px 18px 0 0;">
                    <h5 class="modal-title fw-bold">📋 Solicitar Justificación</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                </div>

                <div class="modal-body p-4">
                    <asp:HiddenField ID="hfIdAsistencia" runat="server" />

                    <!-- Info del registro seleccionado -->
                    <div class="alert alert-info mb-4">
                        <strong>Empleado:</strong>
                        <asp:Label ID="lblNombreEmpleado" runat="server" CssClass="fw-bold ms-1"></asp:Label><br />
                        <strong>Fecha:</strong>
                        <asp:Label ID="lblFechaRegistro" runat="server" CssClass="ms-1"></asp:Label>
                        &nbsp;&nbsp;<strong>Tipo:</strong>
                        <asp:Label ID="lblTipoRegistro" runat="server" CssClass="ms-1"></asp:Label>
                    </div>

                    <!-- Motivo -->
                    <div class="mb-4">
                        <label class="form-label fw-semibold">Motivo <span class="text-danger">*</span></label>
                        <asp:DropDownList ID="ddlMotivo" runat="server"
                            CssClass="form-control form-select form-select-lg"
                            Style="border-radius:10px;">
                            <asp:ListItem Value="">-- Selecciona un motivo --</asp:ListItem>
                            <asp:ListItem Value="Accidente">Accidente</asp:ListItem>
                            <asp:ListItem Value="Enfermedad">Enfermedad</asp:ListItem>
                            <asp:ListItem Value="Problema Personal">Problema Personal</asp:ListItem>
                            <asp:ListItem Value="Otro">Otro</asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <!-- Comentarios -->
                    <div class="mb-3">
                        <label class="form-label fw-semibold">Comentarios adicionales</label>
                        <asp:TextBox ID="txtComentarios" TextMode="MultiLine" runat="server"
                            CssClass="form-control"
                            Style="border-radius:10px; height:120px; resize:none;"
                            Placeholder="Describe brevemente lo ocurrido..." />
                    </div>
                </div>

                <div class="modal-footer d-flex justify-content-between px-4 pb-4">
                    <asp:Button ID="btnGuardarJustificacion"
                        runat="server"
                        Text="Enviar Solicitud"
                        CssClass="btn btn-primary btn-lg px-4 shadow-sm"
                        Style="border-radius:12px;"
                        OnClick="btnGuardarJustificacion_Click" />
                    <button type="button" class="btn btn-outline-secondary btn-lg px-4"
                        style="border-radius:12px;"
                        data-bs-dismiss="modal">
                        Cancelar
                    </button>
                </div>

            </div>
        </div>
    </div>

    <script>
        function abrirModalJustificar() {
            var myModal = new bootstrap.Modal(document.getElementById('modalJustificacion'));
            myModal.show();
        }
        function cerrarModalJustificar() {
            var inst = bootstrap.Modal.getInstance(document.getElementById('modalJustificacion'));
            if (inst) inst.hide();
        }
    </script>

</asp:Content>
