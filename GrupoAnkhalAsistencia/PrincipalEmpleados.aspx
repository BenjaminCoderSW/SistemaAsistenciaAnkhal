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
                    <span class="badge 
                        <%# Eval("Importancia").ToString() == "Urgente" ? "badge-danger" :
                            Eval("Importancia").ToString() == "Alta" ? "badge-warning" : "badge-info" %>">
                        <%# Eval("Importancia") %>
                    </span>
                </h5>

                <p class="card-text">
                    <%# Eval("Mensaje") %>
                </p>

                <small class="text-muted">
                    <i class="far fa-calendar"></i> 
                    <%# Convert.ToDateTime(Eval("Fecha")).ToString("dd/MM/yyyy hh:mm tt") %>
                    
                    <%# Eval("FechaVigencia") != DBNull.Value ? 
                        " | Vigente hasta: " + Convert.ToDateTime(Eval("FechaVigencia")).ToString("dd/MM/yyyy") : "" %>
                </small>
            </div>
        </div>
    </ItemTemplate>
</asp:Repeater>

<div id="noAvisos" runat="server" visible="false" class="alert alert-info text-center">
    <i class="fas fa-info-circle"></i> No hay avisos disponibles en este momento.
</div>

</asp:Content>