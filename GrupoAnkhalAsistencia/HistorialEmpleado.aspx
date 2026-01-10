<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="HistorialEmpleado.aspx.cs" Inherits="GrupoAnkhalAsistencia.HistorialEmpleado" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
                          <link href="css/gridviewPantalla.css" rel="stylesheet" />
<script src="scriptspropios/sweetalert2@11.js"></script>
<script src="scriptspropios/propios.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
                                        <h2>Historial Empleado</h2>
    <!-- Grid de datos -->
  <br />

<div class="table-responsive">
    <div class="col-md-6"> 
        <asp:TextBox ID="txtBuscar" runat="server"
            CssClass="form-control"
            Placeholder="Buscar Empleado..."
            AutoPostBack="true"
            OnTextChanged="txtBuscar_TextChanged" />

        <br />
        <br />
        <asp:Button ID="btnExportExcel" runat="server" Text="Exportar a Excel" CssClass="btn btn-success"
    OnClick="btnExportExcel_Click" />

<asp:Button ID="btnExportPDF" runat="server" Text="Exportar a PDF" CssClass="btn btn-danger"
    OnClick="btnExportPDF_Click" />

<br /><br />

    </div>

    <br />

    <asp:GridView ID="dvgHistorialEmpleado" runat="server" AutoGenerateColumns="False"
        CssClass="table table-bordered table-striped custom-grid"
        AllowPaging="True" PageSize="5"
        OnPageIndexChanging="dvgHistorialEmpleado_PageIndexChanging">
       <Columns>

    <asp:BoundField DataField="EMPLEADO" HeaderText="EMPLEADO" />
    <asp:BoundField DataField="Planta" HeaderText="Planta" />
    <asp:BoundField DataField="Fecha" HeaderText="Fecha" />
    <asp:BoundField DataField="HoraEntrada" HeaderText="HoraEntrada" />
    <asp:BoundField DataField="HoraSalida" HeaderText="HoraSalida" />
    <asp:BoundField DataField="HoraSalidaComer" HeaderText="HoraSalidaComer" />
    <asp:BoundField DataField="HoraEntradaComer" HeaderText="HoraEntradaComer" />
    <asp:BoundField DataField="HorasTrabajadas" HeaderText="HorasTrabajadas" />
    <asp:BoundField DataField="tiempoComida" HeaderText="Tiempo Comida" />

    
    <asp:TemplateField HeaderText="Entrada">
        <ItemTemplate>
            <span style='padding:5px; color:black;
                  background-color:<%# 
                    (Eval("EstatusEntrada").ToString() == "A tiempo") ? "lightgreen" : 
                    (Eval("EstatusEntrada").ToString() == "Retardo") ? "yellow" : "white" %>'>
                <%# Eval("EstatusEntrada") %>
            </span>
        </ItemTemplate>
    </asp:TemplateField>

   
    <asp:TemplateField HeaderText="Salida">
        <ItemTemplate>
            <span style='padding:5px; color:black; background-color:<%# 
    (Eval("EstatusSalida") == null) ? "white" : 
    (Eval("EstatusSalida").ToString() == "Horario Cumplido") ? "lightgreen" :
    (Eval("EstatusSalida").ToString() == "Horario no cumplido") ? "yellow" :
    "white"
%>'>
    <%# Eval("EstatusSalida") %>
</span>

        </ItemTemplate>
    </asp:TemplateField>

   
    <asp:TemplateField HeaderText="Comida">
        <ItemTemplate>
           <span style='padding:5px; color:black;
      background-color:<%#
        (Eval("EstatusComida") == null) ? "white" :
        (Eval("EstatusComida").ToString() == "Comida a tiempo") ? "lightgreen" :
        (Eval("EstatusComida").ToString() == "Retardo Comida") ? "yellow" :
        "white" 
      %>'>
    <%# Eval("EstatusComida") %>
</span>

        </ItemTemplate>
    </asp:TemplateField>

   
    <asp:TemplateField HeaderText="Ubicación Entrada">
        <ItemTemplate>
            <%# GetMapaLink(Eval("UbicacionEntrada").ToString()) %>
        </ItemTemplate>
    </asp:TemplateField>


    <asp:TemplateField HeaderText="Ubicación Salida">
        <ItemTemplate>
            <%# GetMapaLink(Eval("UbicacionSalida").ToString()) %>
        </ItemTemplate>
    </asp:TemplateField>

    <asp:BoundField DataField="MacEntrada" HeaderText="MacEntrada" />
    <asp:BoundField DataField="MacSalida" HeaderText="MacSalida" />
    <asp:BoundField DataField="IP" HeaderText="IP" />

</Columns>

    </asp:GridView>
</div>
</asp:Content>
