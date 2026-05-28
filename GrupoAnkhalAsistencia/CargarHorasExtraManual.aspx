<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CargarHorasExtraManual.aspx.cs" Inherits="GrupoAnkhalAsistencia.CargarHorasExtraManual" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta charset="utf-8" />
    <link href="css/gridviewPantalla.css" rel="stylesheet" />
    <script src="scriptspropios/sweetalert2@11.js"></script>
    <script src="scriptspropios/propios.js"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="card card-primary card-outline">
        <div class="card-header">
            <h3 class="card-title">
                <i class="fas fa-user-edit"></i> Cargar Horas Extra Manual
            </h3>
        </div>
        <div class="card-body">

            <p class="text-muted">
                <i class="fas fa-info-circle"></i>
                Use este formulario para registrar horas extra realizadas fuera del sistema de checador
                (viajes, actividades en campo, guardias nocturnas, etc.).
                Los registros quedar&aacute;n pendientes de aprobaci&oacute;n en <strong>Aprobar Horas Extra</strong>.
            </p>

            <div class="row">
                <div class="col-md-4">
                    <div class="form-group">
                        <label class="font-weight-bold">Empleado: <span class="text-danger">*</span></label>
                        <asp:DropDownList ID="ddlEmpleado" runat="server" CssClass="form-control" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label class="font-weight-bold">Fecha: <span class="text-danger">*</span></label>
                        <asp:TextBox ID="txtFecha" runat="server" CssClass="form-control" TextMode="Date" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label class="font-weight-bold">Horas Extra: <span class="text-danger">*</span></label>
                        <div class="input-group">
                            <asp:TextBox ID="txtHorasH" runat="server" CssClass="form-control"
                                placeholder="0" style="max-width:70px" />
                            <div class="input-group-append input-group-prepend">
                                <span class="input-group-text">h</span>
                            </div>
                            <asp:DropDownList ID="ddlMinutos" runat="server" CssClass="form-control" style="max-width:95px">
                                <asp:ListItem Value="0">00 min</asp:ListItem>
                                <asp:ListItem Value="30">30 min</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <small class="text-muted">Ej: <strong>3</strong> h + <strong>30 min</strong></small>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="form-group">
                        <label class="font-weight-bold">Descripci&oacute;n / Motivo: <span class="text-danger">*</span></label>
                        <asp:TextBox ID="txtDescripcion" runat="server" CssClass="form-control"
                            TextMode="MultiLine" Rows="2"
                            Placeholder="Ej: Viaje a Monterrey, guardia nocturna, entrega urgente..." />
                    </div>
                </div>
            </div>

            <div class="row">
                <div class="col-md-12">
                    <asp:Button ID="btnRegistrar" runat="server" Text="Registrar Horas Extra"
                        CssClass="btn btn-primary mr-2" OnClick="btnRegistrar_Click" />
                    <asp:Button ID="btnLimpiar" runat="server" Text="Limpiar"
                        CssClass="btn btn-secondary" OnClick="btnLimpiar_Click" CausesValidation="false" />
                </div>
            </div>

        </div>
    </div>

    <div class="card card-outline card-info mt-3">
        <div class="card-header">
            <h3 class="card-title">
                <i class="fas fa-list"></i> Registros Recientes (&uacute;ltimos 30 d&iacute;as)
            </h3>
        </div>
        <div class="card-body">
            <p class="text-muted small">
                Los registros en estado <span class="badge badge-warning">Pendiente</span> pueden eliminarse.
                Los que ya fueron aprobados o rechazados no se pueden borrar.
            </p>
            <div class="table-responsive">
                <asp:GridView ID="gvRecientes" runat="server"
                    AutoGenerateColumns="False"
                    CssClass="table table-bordered table-striped table-sm custom-grid"
                    DataKeyNames="IdHorasExtraManual"
                    EmptyDataText="No hay registros en los &uacute;ltimos 30 d&iacute;as."
                    OnRowCommand="gvRecientes_RowCommand"
                    OnRowDataBound="gvRecientes_RowDataBound">
                    <Columns>
                        <asp:BoundField DataField="Empleado" HeaderText="Empleado" />
                        <asp:BoundField DataField="Fecha" HeaderText="Fecha" />
                        <asp:BoundField DataField="HorasFormato" HeaderText="Horas" />
                        <asp:BoundField DataField="Descripcion" HeaderText="Descripci&oacute;n" />
                        <asp:BoundField DataField="FechaCaptura" HeaderText="Capturado el" />
                        <asp:BoundField DataField="EstatusTexto" HeaderText="Estatus" />
                        <asp:TemplateField HeaderText="Eliminar">
                            <ItemTemplate>
                                <asp:LinkButton ID="lbEliminar" runat="server"
                                    CssClass="btn btn-sm btn-outline-danger"
                                    CommandName="Eliminar"
                                    CommandArgument='<%# Eval("IdHorasExtraManual") %>'
                                    Visible='<%# (int)Eval("EstatusAprobacion") == 1 %>'
                                    OnClientClick="return confirm('¿Eliminar este registro?');">
                                    <i class="fas fa-trash"></i>
                                </asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </div>

</asp:Content>
