<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ReportePermisos.aspx.cs" Inherits="GrupoAnkhalAsistencia.ReportePermisos" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
                              <link href="css/gridviewPantalla.css" rel="stylesheet" />
<script src="scriptspropios/sweetalert2@11.js"></script>
<script src="scriptspropios/propios.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
                                            <h2>Reporte de Permisos Autorizados por dias</h2>
    <!-- Grid de datos -->
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

    <asp:GridView ID="dvgPermiso" runat="server" AutoGenerateColumns="False"
        CssClass="table table-bordered table-striped custom-grid"
        AllowPaging="True" PageSize="5"
        OnPageIndexChanging="dvgPermiso_PageIndexChanging">
        <Columns>

            <asp:BoundField DataField="Empleado" HeaderText="Empleado" />
            <asp:BoundField DataField="Jefe" HeaderText="Jefe" />
            <asp:BoundField DataField="CorreoJefe" HeaderText="CorreoJefe" />
            <asp:BoundField DataField="Motivo" HeaderText="Motivo" />
            <asp:BoundField DataField="FechaInicio" HeaderText="FechaInicio" />
            <asp:BoundField DataField="FechaFin" HeaderText="FechaFin" />
            <asp:BoundField DataField="Dias" HeaderText="Dias" />
            <asp:BoundField DataField="Observaciones" HeaderText="Observaciones" />
            <asp:BoundField DataField="EstatusTexto" HeaderText="Estatus" />

            <asp:TemplateField HeaderText="Acciones">
                <ItemTemplate>


                    <asp:Button ID="btnEliminar" runat="server"
                                Text="Eliminar"
                                CommandArgument='<%# Eval("IdPermisoDias") %>'
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
            title: '¿Estas seguro?',
            text: "Se actualizará el estatus a eliminado.",
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

        return false; // Detener el postback hasta confirmar
    }
</script>
</asp:Content>
