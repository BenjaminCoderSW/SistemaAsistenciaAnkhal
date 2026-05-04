<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AprobarVacaciones.aspx.cs" Inherits="GrupoAnkhalAsistencia.AprobarVacaciones" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="css/gridviewPantalla.css" rel="stylesheet" />
    <script src="scriptspropios/sweetalert2@11.js"></script>
    <script src="scriptspropios/propios.js"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h2>Gestión de Solicitudes de Vacaciones</h2>
    <br />

    <div class="table-responsive">
        <div class="col-md-6">
            <asp:TextBox ID="txtBuscar" runat="server"
                CssClass="form-control"
                Placeholder="Buscar Empleado..."
                AutoPostBack="true"
                OnTextChanged="txtBuscar_TextChanged" />
        </div>

        <br />

        <asp:GridView ID="dvgVacaciones" runat="server" AutoGenerateColumns="False"
            CssClass="table table-bordered table-striped custom-grid"
            AllowPaging="True" PageSize="5"
            OnPageIndexChanging="dvgVacaciones_PageIndexChanging">
            <Columns>
                <asp:BoundField DataField="Empleado" HeaderText="Empleado" />
                <asp:BoundField DataField="Planta" HeaderText="Planta" />
                <asp:BoundField DataField="Jefe" HeaderText="Jefe" />
                <asp:BoundField DataField="FechaInicio" HeaderText="Fecha Inicio" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:BoundField DataField="FechaFin" HeaderText="Fecha Fin" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:BoundField DataField="Dias" HeaderText="Días" />
                <asp:BoundField DataField="FechaSolicitud" HeaderText="Solicitado el" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                <asp:BoundField DataField="DecisionJefe" HeaderText="Decisi&oacute;n del Jefe" />
                <asp:BoundField DataField="AprobadoPorJefe" HeaderText="Jefe de Planta" />
                <asp:BoundField DataField="MotivoJefe" HeaderText="Motivo del Jefe" />

                <asp:TemplateField HeaderText="Acciones">
                    <ItemTemplate>
                        <asp:Button ID="btnAutorizar"
                            runat="server"
                            Text="Autorizar"
                            CssClass="btn btn-primary btn-sm"
                            CommandArgument='<%# Eval("IdVacaciones") %>'
                            OnClick="btnAutorizar_Click" />

                        <asp:Button ID="btnRechazar" runat="server"
                            Text="Rechazar"
                            CommandArgument='<%# Eval("IdVacaciones") %>'
                            OnClientClick="return confirmarRechazar(this);"
                            OnClick="btnRechazar_Click"
                            CssClass="btn btn-danger btn-sm" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>

    <script>
        function confirmarRechazar(btn) {
            Swal.fire({
                title: '¿Rechazar solicitud?',
                text: "Se marcará como rechazada y se notificará al empleado.",
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