<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="GraficaEmpleado.aspx.cs"
    Inherits="GrupoAnkhalAsistencia.GraficaEmpleado" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>

    <style>
        .card-filtro, .card-grafica {
            background: #fff;
            padding: 20px;
            border-radius: 15px;
            box-shadow: 0 4px 20px rgba(0,0,0,0.1);
            margin-bottom: 25px;
        }

        .filtro label {
            font-weight: 500;
            margin-bottom: 5px;
        }

        #btnBuscar {
            margin-top: 25px;
        }

        canvas {
            width: 100% !important;
            height: 300px !important;
        }

        h2 {
            text-align: center;
            margin-bottom: 30px;
        }

        .filtro input, .filtro select {
            width: 100%;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <h2>Estadísticas del Empleado</h2>

    <!-- Tarjeta de filtros -->
    <div class="container">
        <div class="card-filtro row g-3">
            <asp:HiddenField ID="hfEmpleadoId" runat="server" />

            <div class="col-md-4">
                <label>Empleado:</label>
                <asp:DropDownList ID="ddlEmpleados" runat="server" CssClass="form-select"></asp:DropDownList>
            </div>

            <div class="col-md-3">
                <label>Fecha inicio:</label>
                <asp:TextBox ID="txtFechaInicio" TextMode="Date" runat="server" CssClass="form-control" />
            </div>

            <div class="col-md-3">
                <label>Fecha fin:</label>
                <asp:TextBox ID="txtFechaFin" TextMode="Date" runat="server" CssClass="form-control" />
            </div>

            <div class="col-md-2 d-flex align-items-end">
                <asp:Button ID="btnBuscar" runat="server" Text="Buscar" CssClass="btn btn-primary w-100" OnClick="btnBuscar_Click" />
            </div>

            <div class="col-12">
                <asp:Label ID="lblError" runat="server" ForeColor="Red" />
            </div>
        </div>
    </div>

    <!-- Fila con 3 gráficas -->
    <div class="container">
        <div class="row g-3">
            <!-- Gráfica Entradas/Salidas -->
            <div class="col-md-4">
                <div class="card-grafica">
                    <h5 class="text-center">Entradas y Salidas</h5>
                    <canvas id="graficaAsistencia"></canvas>
                </div>
            </div>

            <!-- Gráfica Comida -->
            <div class="col-md-4">
                <div class="card-grafica">
                    <h5 class="text-center">Estatus Comida</h5>
                    <canvas id="graficaComida"></canvas>
                </div>
            </div>

            <!-- Gráfica Permisos y Comisiones -->
            <div class="col-md-4">
                <div class="card-grafica">
                    <h5 class="text-center">Permisos y Comisiones</h5>
                    <canvas id="graficaPermisos"></canvas>
                </div>
            </div>
        </div>
    </div>

    <asp:Literal ID="ltScript" runat="server"></asp:Literal>

</asp:Content>
