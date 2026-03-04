<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AceptarJustificacion.aspx.cs" Inherits="GrupoAnkhalAsistencia.AceptarJustificacion" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="css/gridviewPantalla.css" rel="stylesheet" />
    <script src="scriptspropios/sweetalert2@11.js"></script>
    <script src="scriptspropios/propios.js"></script>
    <style>
        .badge-pendiente { background-color: #ffc107; color: #000; padding: 4px 10px; border-radius: 4px; font-size: 12px; }
        .badge-aceptada  { background-color: #28a745; color: #fff; padding: 4px 10px; border-radius: 4px; font-size: 12px; }
        .badge-rechazada { background-color: #dc3545; color: #fff; padding: 4px 10px; border-radius: 4px; font-size: 12px; }
        .section-title   { color: #003366; font-weight: 700; border-left: 4px solid #003366; padding-left: 10px; margin-bottom: 16px; }
        .card-filtros    { background: #fff; border-radius: 10px; padding: 20px; box-shadow: 0 2px 8px rgba(0,0,0,0.08); margin-bottom: 20px; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <h2 class="section-title">Gestión de Justificaciones</h2>

    <!-- Filtros -->
    <div class="card-filtros">
        <div class="row align-items-end">
            <div class="col-md-4">
                <label class="form-label fw-semibold">Buscar Empleado</label>
                <asp:TextBox ID="txtBuscar" runat="server"
                    CssClass="form-control"
                    Placeholder="Nombre del empleado..."
                    AutoPostBack="true"
                    OnTextChanged="txtBuscar_TextChanged" />
            </div>
            <div class="col-md-3">
                <label class="form-label fw-semibold">Estatus</label>
                <asp:DropDownList ID="ddlEstatus" runat="server" CssClass="form-control"
                    AutoPostBack="true" OnSelectedIndexChanged="ddlEstatus_SelectedIndexChanged">
                    <asp:ListItem Value="1">Pendientes</asp:ListItem>
                    <asp:ListItem Value="2">Aceptadas</asp:ListItem>
                    <asp:ListItem Value="3">Rechazadas</asp:ListItem>
                    <asp:ListItem Value="0">Todas</asp:ListItem>
                </asp:DropDownList>
            </div>
        </div>
    </div>

    <!-- Tabla -->
    <div class="table-responsive">
        <asp:GridView ID="dvgJustificaion" runat="server" AutoGenerateColumns="False"
            CssClass="table table-bordered table-striped custom-grid"
            AllowPaging="True" PageSize="10"
            OnPageIndexChanging="dvgJustificaion_PageIndexChanging"
            EmptyDataText="No hay justificaciones con los filtros seleccionados.">
            <Columns>
                <asp:BoundField DataField="NombreCompleto"   HeaderText="Empleado" />
                <asp:BoundField DataField="FechaAsistencia"  HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:BoundField DataField="EstatusEntrada"   HeaderText="Tipo" />
                <asp:BoundField DataField="HoraEntrada"      HeaderText="Hora Entrada" />
                <asp:BoundField DataField="HoraInicio"       HeaderText="Hora Programada" />
                <asp:BoundField DataField="Motivo"           HeaderText="Motivo" />
                <asp:BoundField DataField="Observaciones"    HeaderText="Comentarios" />
                <asp:BoundField DataField="FechaJustificacion" HeaderText="Fecha Solicitud" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:TemplateField HeaderText="Estatus">
                    <ItemTemplate>
                        <%# ObtenerBadge(Eval("Estatus").ToString()) %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Acciones">
                    <ItemTemplate>
                        <!-- Autorizar -->
                        <asp:Button ID="btnAutorizar" runat="server"
                            Text="✔ Autorizar"
                            CssClass="btn btn-success btn-sm"
                            CommandArgument='<%# Eval("IdJustificacion") + "|" + Eval("IdAsistencia") + "|" + Eval("HoraInicio") %>'
                            OnClick="btnAutorizar_Click"
                            Visible='<%# Eval("Estatus").ToString() == "1" %>' />

                        <!-- Rechazar -->
                        <asp:Button ID="btnRechazar" runat="server"
                            Text="✖ Rechazar"
                            CssClass="btn btn-danger btn-sm"
                            CommandArgument='<%# Eval("IdJustificacion") + "|" + Eval("IdAsistencia") %>'
                            OnClientClick="return confirmarRechazar(this);"
                            OnClick="btnRechazar_Click"
                            Visible='<%# Eval("Estatus").ToString() == "1" %>' />

                        <asp:Label runat="server"
                            Text="—"
                            Visible='<%# Eval("Estatus").ToString() != "1" %>'
                            CssClass="text-muted" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>

    <script>
        function confirmarRechazar(btn) {
            Swal.fire({
                title: '¿Rechazar justificación?',
                text: "El empleado podrá volver a enviar una solicitud.",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Sí, rechazar',
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
