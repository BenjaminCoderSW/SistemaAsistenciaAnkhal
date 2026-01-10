<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Avisos.aspx.cs" Inherits="GrupoAnkhalAsistencia.Avisos" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mt-4">

    <h3 class="text-center mb-4">Crear Aviso de Recursos Humanos</h3>

    <div class="card shadow">
        <div class="card-body">

            <!-- Título -->
            <div class="mb-3">
                <label for="txtTitulo" class="form-label">Título del aviso</label>
                <asp:TextBox ID="txtTitulo" runat="server" CssClass="form-control" MaxLength="200"></asp:TextBox>
            </div>

            <!-- Mensaje -->
            <div class="mb-3">
                <label for="txtMensaje" class="form-label">Mensaje</label>
                <asp:TextBox ID="txtMensaje" TextMode="MultiLine" Rows="5"
                    runat="server" CssClass="form-control"></asp:TextBox>
            </div>

            <!-- Importancia -->
            <div class="mb-3">
                <label for="ddlImportancia" class="form-label">Importancia</label>
                <asp:DropDownList ID="ddlImportancia" runat="server" CssClass="form-control">
                    <asp:ListItem Text="Normal" Value="Normal"></asp:ListItem>
                    <asp:ListItem Text="Alta" Value="Alta"></asp:ListItem>
                    <asp:ListItem Text="Urgente" Value="Urgente"></asp:ListItem>
                </asp:DropDownList>
            </div>

            <!-- Aviso general o por usuario -->
            <div class="mb-3">
                <label for="ddlUsuario" class="form-label">Enviar a:</label>
                <asp:DropDownList ID="ddlUsuario" runat="server" CssClass="form-control">
                    <asp:ListItem Value="0">Aviso general (todos los empleados)</asp:ListItem>
                </asp:DropDownList>
            </div>

            <!-- Botón Guardar -->
            <div class="text-end mt-4">
                <asp:Button ID="btnGuardar" runat="server" 
                    Text="Guardar Aviso" 
                    CssClass="btn btn-primary px-4"
                    OnClick="btnGuardar_Click" />
            </div>

        </div>
    </div>

</div>

</asp:Content>
