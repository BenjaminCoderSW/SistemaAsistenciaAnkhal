<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AprobarVacacionesJefe.aspx.cs" Inherits="GrupoAnkhalAsistencia.AprobarVacacionesJefe" ResponseEncoding="utf-8" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta charset="utf-8" />
    <link href="css/gridviewPantalla.css" rel="stylesheet" />
    <script src="scriptspropios/sweetalert2@11.js"></script>
    <script src="scriptspropios/propios.js"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h2>Aprobaci&oacute;n de Vacaciones &mdash; Jefe de Planta</h2>
    <br />

    <p class="text-muted">
        <i class="fas fa-info-circle"></i>
        Revisa las solicitudes de vacaciones de tu planta. Escribe un motivo y selecciona Aprobar o Rechazar, luego haz clic en <strong>Guardar</strong>.
    </p>

    <div class="table-responsive">
        <div class="col-md-6">
            <asp:TextBox ID="txtBuscar" runat="server"
                CssClass="form-control"
                Placeholder="Buscar empleado..."
                AutoPostBack="true"
                OnTextChanged="txtBuscar_TextChanged" />
        </div>

        <br />

        <asp:GridView ID="gvVacaciones" runat="server"
            AutoGenerateColumns="False"
            CssClass="table table-bordered table-striped custom-grid"
            AllowPaging="True" PageSize="10"
            DataKeyNames="IdVacaciones"
            OnPageIndexChanging="gvVacaciones_PageIndexChanging">
            <Columns>
                <asp:BoundField DataField="Empleado" HeaderText="Empleado" />
                <asp:BoundField DataField="Planta" HeaderText="Planta" />
                <asp:BoundField DataField="JefeSeleccionado" HeaderText="Jefe Seleccionado" />
                <asp:BoundField DataField="FechaInicio" HeaderText="Fecha Inicio" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:BoundField DataField="FechaFin" HeaderText="Fecha Fin" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:BoundField DataField="Dias" HeaderText="D&iacute;as" />
                <asp:BoundField DataField="FechaSolicitud" HeaderText="Solicitado el" DataFormatString="{0:dd/MM/yyyy HH:mm}" />

                <asp:TemplateField HeaderText="Motivo (requerido)">
                    <ItemTemplate>
                        <asp:TextBox ID="txtMotivo" runat="server"
                            CssClass="form-control form-control-sm"
                            Width="220px"
                            placeholder="Escribe el motivo..." />
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Decisi&oacute;n">
                    <ItemTemplate>
                        <asp:DropDownList ID="ddlDecision" runat="server" CssClass="form-control form-control-sm">
                            <asp:ListItem Value="0" Text="-- Sin cambio --" />
                            <asp:ListItem Value="1" Text="Aprobar" />
                            <asp:ListItem Value="2" Text="Rechazar" />
                        </asp:DropDownList>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Acci&oacute;n">
                    <ItemTemplate>
                        <asp:Button ID="btnGuardar" runat="server"
                            Text="Guardar"
                            CssClass="btn btn-primary btn-sm"
                            CommandName="Guardar"
                            CommandArgument='<%# Eval("IdVacaciones") %>'
                            OnClick="btnGuardar_Click" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>
