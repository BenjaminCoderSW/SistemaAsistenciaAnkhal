<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="GraficaPuntualidad.aspx.cs" Inherits="GrupoAnkhalAsistencia.GraficaPuntualidad" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <!-- ✅ AGREGADO: Referencia a Chart.js -->
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    
    <style>
        .card-filtro {
            background: #fff;
            padding: 20px;
            border-radius: 15px;
            box-shadow: 0 4px 20px rgba(0,0,0,0.1);
            margin-bottom: 25px;
        }

        .card-grafica {
            background: #fff;
            padding: 30px;
            border-radius: 15px;
            box-shadow: 0 4px 20px rgba(0,0,0,0.1);
            margin-bottom: 25px;
        }

        canvas {
            width: 100% !important;
            max-height: 400px !important;
        }

        h2, h3 {
            text-align: center;
            margin-bottom: 30px;
            color: #003366;
        }

        .filtro label {
            font-weight: 500;
            margin-bottom: 5px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mt-4">
        <h2>Top 5 Empleados Más Puntuales</h2>

        <!-- Tarjeta de filtros -->
        <div class="card-filtro">
            <div class="row mb-3">
                <div class="col-md-4">
                    <label>Fecha inicio</label>
                    <asp:TextBox ID="txtFechaInicio" CssClass="form-control" TextMode="Date" runat="server"></asp:TextBox>
                </div>

                <div class="col-md-4">
                    <label>Fecha fin</label>
                    <asp:TextBox ID="txtFechaFin" CssClass="form-control" TextMode="Date" runat="server"></asp:TextBox>
                </div>

                <div class="col-md-4 d-flex align-items-end">
                    <asp:Button ID="btnBuscar" CssClass="btn btn-primary w-100" Text="Generar Gráfica" OnClick="btnBuscar_Click" runat="server" />
                </div>
            </div>

            <div class="row">
                <div class="col-12">
                    <asp:Label ID="lblMensaje" runat="server" CssClass="text-danger" />
                </div>
            </div>
        </div>

        <!-- Tarjeta de la gráfica -->
        <div class="card-grafica">
            <h3>Empleados con más entradas a tiempo</h3>
            <canvas id="graficaPuntualidad"></canvas>
        </div>

        <asp:Literal ID="ltScript" runat="server"></asp:Literal>
    </div>
</asp:Content>