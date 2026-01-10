<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AceptarJustificacion.aspx.cs" Inherits="GrupoAnkhalAsistencia.AceptarJustificacion" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
                              <link href="css/gridviewPantalla.css" rel="stylesheet" />
<script src="scriptspropios/sweetalert2@11.js"></script>
<script src="scriptspropios/propios.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
                                            <h2>Gestión de justificaciones</h2>
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

    <asp:GridView ID="dvgJustificaion" runat="server" AutoGenerateColumns="False"
        CssClass="table table-bordered table-striped custom-grid"
        AllowPaging="True" PageSize="5"
        OnPageIndexChanging="dvgJustificaion_PageIndexChanging">
        <Columns>
            <asp:BoundField DataField="IdAsistencia" HeaderText="IdAsistencia" />
            <asp:BoundField DataField="fechaAsistencia" HeaderText="fechaAsistencia" />
            <asp:BoundField DataField="nombreCompleto" HeaderText="nombreCompleto" />
            <asp:BoundField DataField="fechaJustificaion" HeaderText="fechaJustificaion" />
            <asp:BoundField DataField="Motivo" HeaderText="Motivo" />
            <asp:BoundField DataField="Observaciones" HeaderText="Observaciones" />
            <asp:BoundField DataField="Estatus" HeaderText="Estatus" />
            <asp:BoundField DataField="HoraInicio" HeaderText="HoraInicio" />
            <asp:TemplateField HeaderText="Acciones">
                <ItemTemplate>

                    <asp:Button ID="btnAutorizar" 
                                runat="server" 
                                Text="Autorizar" 
                                CssClass="btn btn-primary"
                               CommandArgument='<%# Eval("IdJustificacion") + "|" + Eval("IdAsistencia")+ "|" + Eval("HoraInicio") %>'
                                OnClick="btnAutorizar_Click" />

                    <asp:Button ID="btnEliminar" runat="server"
                                Text="Eliminar"
                                CommandArgument='<%# Eval("IdJustificacion") %>'
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
