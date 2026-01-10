<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="PrincipalAdmin.aspx.cs"
    Inherits="GrupoAnkhalAsistencia.PrincipalAdmin" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .card-info {
            padding: 25px;
            border-radius: 10px;
            color: #fff;
            margin-bottom: 20px;
            text-align: center;
            font-size: 22px;
            font-weight: bold;
        }

        .bg-total { background: #0d6efd; }
        .bg-tiempo { background: #198754; }
        .bg-tarde { background: #ffc107; color: black; }
        .bg-faltaron { background: #dc3545; }

        .table-container {
            margin-top: 30px;
        }
    </style>
</asp:Content>


<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="container-fluid mt-4">

        <h2 class="mb-4">Resumen de Asistencia (Hoy)</h2>

        <!-- CARDS -->
        <div class="row">

            <div class="col-xl-3 col-lg-4 col-md-6 mb-3">
                <div class="card-info bg-total shadow">
                    Total empleados<br />
                    <asp:Label ID="lblTotalEmpleados" runat="server" Text="0"></asp:Label>
                </div>
            </div>

            <div class="col-xl-3 col-lg-4 col-md-6 mb-3">
                <div class="card-info bg-tiempo shadow">
                    Llegaron a tiempo<br />
                    <asp:Label ID="lblLlegaronTiempo" runat="server" Text="0"></asp:Label>
                </div>
            </div>

            <div class="col-xl-3 col-lg-4 col-md-6 mb-3">
                <div class="card-info bg-tarde shadow">
                    Llegaron tarde<br />
                    <asp:Label ID="lblLlegaronTarde" runat="server" Text="0"></asp:Label>
                </div>
            </div>

            <div class="col-xl-3 col-lg-4 col-md-6 mb-3">
                <div class="card-info bg-faltaron shadow">
                    Faltaron<br />
                    <asp:Label ID="lblFaltaron" runat="server" Text="0"></asp:Label>
                </div>
            </div>

        </div>

        <!-- TABLA -->
        <div class="table-container card shadow p-3">
            <div class="table-responsive">
                <asp:GridView ID="gvAsistenciaHoy" runat="server"
                    CssClass="table table-bordered table-striped"
                    AutoGenerateColumns="False">

                    <Columns>
                        <asp:BoundField DataField="EMPLEADO" HeaderText="Empleado" />
                        <asp:BoundField DataField="Planta" HeaderText="Planta" />
                        <asp:BoundField DataField="Fecha" HeaderText="Fecha" />
                        <asp:BoundField DataField="HoraEntrada" HeaderText="Entrada" />
                        <asp:BoundField DataField="HoraSalida" HeaderText="Salida" />
                        <asp:BoundField DataField="EstatusEntrada" HeaderText="Estatus Entrada" />
                    </Columns>

                </asp:GridView>
            </div>
        </div>

    </div>

</asp:Content>
