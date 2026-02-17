<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" 
    CodeBehind="ReporteVacaciones.aspx.cs" Inherits="GrupoAnkhalAsistencia.ReporteVacaciones" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="css/gridviewPantalla.css" rel="stylesheet" />
    <script src="scriptspropios/sweetalert2@11.js"></script>
    <script src="scriptspropios/propios.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h2>Reporte de Vacaciones</h2>
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
            AllowPaging="True" PageSize="10"
            OnPageIndexChanging="dvgVacaciones_PageIndexChanging">
            <Columns>
                <asp:BoundField DataField="Empleado"     HeaderText="Empleado" />
                <asp:BoundField DataField="Jefe"         HeaderText="Jefe" />
                <asp:BoundField DataField="CorreoJefe"   HeaderText="Correo Jefe" />
                <asp:BoundField DataField="FechaInicio"  HeaderText="Fecha Inicio" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:BoundField DataField="FechaFin"     HeaderText="Fecha Fin"    DataFormatString="{0:dd/MM/yyyy}" />
                <asp:BoundField DataField="Dias"         HeaderText="Días" />
                <asp:BoundField DataField="EstatusTexto" HeaderText="Estatus" />
            </Columns>
        </asp:GridView>
    </div>

    <script>
        function confirmarEliminar(btn) {
            Swal.fire({
                title: '¿Estás seguro?',
                text: "Se eliminará el registro de vacaciones.",
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