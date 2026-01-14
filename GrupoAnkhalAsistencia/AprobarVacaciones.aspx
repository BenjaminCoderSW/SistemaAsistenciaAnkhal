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
                <asp:BoundField DataField="Jefe" HeaderText="Jefe" />
                <asp:BoundField DataField="CorreoJefe" HeaderText="Correo Jefe" />
                <asp:BoundField DataField="FechaInicio" HeaderText="Fecha Inicio" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:BoundField DataField="FechaFin" HeaderText="Fecha Fin" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:BoundField DataField="Dias" HeaderText="Días" />
                <asp:BoundField DataField="EstatusTexto" HeaderText="Estatus" />

                <asp:TemplateField HeaderText="Acciones">
                    <ItemTemplate>
                        <asp:Button ID="btnAutorizar"
                            runat="server"
                            Text="Autorizar"
                            CssClass="btn btn-primary"
                            CommandArgument='<%# Eval("IdVacaciones") %>'
                            OnClick="btnAutorizar_Click" />

                        <asp:Button ID="btnEliminar" runat="server"
                            Text="Eliminar"
                            CommandArgument='<%# Eval("IdVacaciones") %>'
                            OnClientClick="return confirmarEliminar(this);"
                            OnClick="btnEliminar_Click"
                            CssClass="btn btn-danger" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>

    <script>
        function confirmarEliminar(btn) {
            Swal.fire({
                title: '¿Estás seguro?',
                text: "Se eliminará la solicitud de vacaciones.",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Sí, eliminar'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack(btn.name, '');
                }
            });
            return false;
        }
    </script>
</asp:Content>