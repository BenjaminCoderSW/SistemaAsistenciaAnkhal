<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PrincipalEmpleados.aspx.cs" Inherits="GrupoAnkhalAsistencia.PrincipalEmpleados" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h3 class="text-center mt-3">Avisos de Recursos Humanos</h3>

<asp:Repeater ID="rptAvisos" runat="server">
    <ItemTemplate>
        <div class="card shadow-sm mb-3 border-left-primary">
            <div class="card-body">
                <h5 class="card-title">
                    <%# Eval("Titulo") %>
                </h5>

                <p class="card-text">
                    <%# Eval("Mensaje") %>
                </p>

                <small class="text-muted">
                    <%# Convert.ToDateTime(Eval("Fecha")).ToString("dd/MM/yyyy hh:mm tt") %>
                </small>

                <span class="badge 
                    <%# Eval("Importancia").ToString() == "Urgente" ? "badge-danger" :
                        Eval("Importancia").ToString() == "Alta" ? "badge-warning" : "badge-info" %>">
                    <%# Eval("Importancia") %>
                </span>
            </div>
        </div>
    </ItemTemplate>
</asp:Repeater>
</asp:Content>
