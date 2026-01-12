<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ImprimirPapeleta.aspx.cs" Inherits="GrupoAnkhalAsistencia.ImprimirPapeleta" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xlink">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>Papeleta de Nómina</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: Arial, sans-serif;
            font-size: 11px;
            padding: 20px;
        }

        .container {
            width: 100%;
            max-width: 800px;
            margin: 0 auto;
            border: 2px solid #000;
        }

        .header {
            background-color: #ffffff;
            padding: 10px 15px;
            display: flex;
            justify-content: space-between;
            align-items: center;
            border-bottom: 1px solid #000;
        }

        .header-left {
            flex: 1;
        }

        .header-title {
            font-weight: bold;
            font-size: 11px;
            margin-bottom: 3px;
        }

        .header-rfc {
            font-size: 11px;
        }

        .header-right {
            text-align: right;
        }

        .logo {
            max-width: 150px;
            height: auto;
        }

        .info-section {
            background-color: #d9d9d9;
            padding: 5px 15px;
            border-bottom: 1px solid #000;
        }

        .info-row {
            display: flex;
            justify-content: space-between;
            margin-bottom: 2px;
            line-height: 1.4;
        }

        .info-row:last-child {
            margin-bottom: 0;
        }

        .info-label {
            font-weight: bold;
            display: inline-block;
            width: 150px;
        }

        .info-value {
            display: inline-block;
            flex: 1;
        }

        .section-header {
            background-color: #d9d9d9;
            padding: 6px 15px;
            font-weight: bold;
            text-align: center;
            border-bottom: 1px solid #000;
            font-size: 12px;
        }

        .table-container {
            display: flex;
        }

        .table-half {
            flex: 1;
            border-right: 1px solid #000;
        }

        .table-half:last-child {
            border-right: none;
        }

        .concept-header-row {
            display: flex;
            background-color: #ffffff;
            border-bottom: 1px solid #000;
            font-weight: bold;
            font-size: 11px;
        }

        .concept-header-label {
            flex: 1;
            padding: 5px 15px;
            border-right: 1px solid #000;
        }

        .concept-header-value {
            width: 120px;
            padding: 5px 10px;
            text-align: center;
        }

        .concept-row {
            display: flex;
            border-bottom: 1px solid #d9d9d9;
            min-height: 24px;
        }

        .concept-label {
            flex: 1;
            padding: 4px 15px;
            border-right: 1px solid #d9d9d9;
            display: flex;
            align-items: center;
        }

        .concept-value {
            width: 120px;
            padding: 4px 10px;
            text-align: right;
            display: flex;
            align-items: center;
            justify-content: flex-end;
        }

        .total-row {
            display: flex;
            background-color: #ffffff;
            font-weight: bold;
            border-top: 2px solid #000;
            border-bottom: 1px solid #000;
        }

        .total-label {
            flex: 1;
            padding: 6px 15px;
            border-right: 1px solid #000;
            text-decoration: underline;
        }

        .total-value {
            width: 120px;
            padding: 6px 10px;
            text-align: right;
            text-decoration: underline;
        }

        .summary-section {
            border-top: 1px solid #000;
        }

        .summary-row {
            display: flex;
            border-bottom: 1px solid #d9d9d9;
            background-color: #d9d9d9;
        }

        .summary-row:last-child {
            border-bottom: none;
        }

        .summary-label {
            flex: 1;
            padding: 6px 15px;
            font-weight: bold;
            border-right: 1px solid #000;
        }

        .summary-value {
            width: 200px;
            padding: 6px 15px;
            text-align: right;
            font-weight: bold;
        }

        .neto-row {
            background-color: #d9d9d9;
            border-bottom: 2px solid #000 !important;
        }

        @media print {
            body {
                padding: 0;
            }
            
            .no-print {
                display: none;
            }
            
            @page {
                margin: 1cm;
            }
        }

        .btn-print {
            padding: 10px 20px;
            background-color: #003366;
            color: white;
            border: none;
            border-radius: 5px;
            cursor: pointer;
            font-size: 14px;
            margin-bottom: 20px;
        }

        .btn-print:hover {
            background-color: #004488;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="no-print" style="text-align: center;">
            <button type="button" class="btn-print" onclick="window.print();">
                🖨️ Imprimir Papeleta
            </button>
            <button type="button" class="btn-print" onclick="window.close();" style="background-color: #666;">
                ✖ Cerrar
            </button>
        </div>

        <div class="container">
            <!-- HEADER -->
            <div class="header">
                <div class="header-left">
                    <div class="header-title">
                        <asp:Literal ID="litEmpresa" runat="server" Text="GRUPOINDUSTRIALYDESERVICIOSANKHAL S DE RL DE CV"></asp:Literal>
                    </div>
                    <div class="header-rfc">
                        <strong>R.F.C.</strong> <asp:Literal ID="litRFCEmpresa" runat="server" Text="GIS201023P87"></asp:Literal>
                    </div>
                </div>
                <div class="header-right">
                    <img src="img/GrupoAnkhalPapeleta.jpeg" alt="Logo Grupo Ankhal" class="logo" />
                </div>
            </div>

            <!-- INFORMACIÓN DEL EMPLEADO -->
            <div class="info-section">
                <div class="info-row">
                    <div>
                        <span class="info-label">N. Trabajador</span>
                        <span class="info-value"><asp:Literal ID="litNTrabajador" runat="server"></asp:Literal></span>
                    </div>
                </div>
                <div class="info-row">
                    <div>
                        <span class="info-label">Nombre</span>
                        <span class="info-value"><asp:Literal ID="litNombre" runat="server"></asp:Literal></span>
                    </div>
                </div>
                <div class="info-row">
                    <div>
                        <span class="info-label">Puesto</span>
                        <span class="info-value"><asp:Literal ID="litPuesto" runat="server"></asp:Literal></span>
                    </div>
                </div>
                <div class="info-row">
                    <div style="display: inline-block; width: 55%;">
                        <span class="info-label">Periodo de pago</span>
                        <span class="info-value"><asp:Literal ID="litPeriodo" runat="server"></asp:Literal></span>
                    </div>
                    <div style="display: inline-block; width: 40%;">
                        <span class="info-label" style="width: 120px;">Fecha de pago</span>
                        <span class="info-value"><asp:Literal ID="litFechaPago" runat="server"></asp:Literal></span>
                    </div>
                </div>
                <div class="info-row">
                    <div>
                        <span class="info-label">Días pagados</span>
                        <span class="info-value"><asp:Literal ID="litDiasPagados" runat="server"></asp:Literal></span>
                    </div>
                </div>
            </div>

            <!-- PERCEPCIONES Y DEDUCCIONES -->
            <div class="table-container">
                <!-- PERCEPCIONES -->
                <div class="table-half">
                    <div class="section-header">PERCEPCIONES</div>
                    <div class="concept-header-row">
                        <div class="concept-header-label">Concepto</div>
                        <div class="concept-header-value">Importe</div>
                    </div>
                    
                    <div class="concept-row">
                        <div class="concept-label">Sueldo del periodo</div>
                        <div class="concept-value"><asp:Literal ID="litSueldoPeriodo" runat="server"></asp:Literal></div>
                    </div>
                    
                    <div class="concept-row">
                        <div class="concept-label">Horas extras</div>
                        <div class="concept-value"><asp:Literal ID="litHorasExtras" runat="server"></asp:Literal></div>
                    </div>
                    
                    <div class="concept-row">
                        <div class="concept-label">Bonos</div>
                        <div class="concept-value"><asp:Literal ID="litBonos" runat="server"></asp:Literal></div>
                    </div>
                    
                    <div class="concept-row">
                        <div class="concept-label">Días pendientes de pago</div>
                        <div class="concept-value"><asp:Literal ID="litDiasPendientes" runat="server"></asp:Literal></div>
                    </div>
                    
                    <div class="concept-row">
                        <div class="concept-label">Veladas</div>
                        <div class="concept-value"><asp:Literal ID="litVeladas" runat="server"></asp:Literal></div>
                    </div>
                    
                    <div class="concept-row">
                        <div class="concept-label">Otros ingresos</div>
                        <div class="concept-value"><asp:Literal ID="litOtrosIngresos" runat="server"></asp:Literal></div>
                    </div>
                    
                    <div class="total-row">
                        <div class="total-label">Total de percepciones</div>
                        <div class="total-value"><asp:Literal ID="litTotalPercepciones" runat="server"></asp:Literal></div>
                    </div>
                </div>

                <!-- DEDUCCIONES -->
                <div class="table-half">
                    <div class="section-header">DEDUCCIONES</div>
                    <div class="concept-header-row">
                        <div class="concept-header-label">Concepto</div>
                        <div class="concept-header-value">Importe</div>
                    </div>
                    
                    <div class="concept-row">
                        <div class="concept-label">Días no laborados</div>
                        <div class="concept-value"><asp:Literal ID="litDiasNoLaborados" runat="server"></asp:Literal></div>
                    </div>
                    
                    <div class="concept-row">
                        <div class="concept-label">Descuento de horas</div>
                        <div class="concept-value"><asp:Literal ID="litDescuentoHoras" runat="server"></asp:Literal></div>
                    </div>
                    
                    <div class="concept-row">
                        <div class="concept-label">Otros descuentos</div>
                        <div class="concept-value"><asp:Literal ID="litOtrosDescuentos" runat="server"></asp:Literal></div>
                    </div>
                    
                    <!-- Filas vacías para alinear -->
                    <div class="concept-row">
                        <div class="concept-label"></div>
                        <div class="concept-value"></div>
                    </div>
                    
                    <div class="concept-row">
                        <div class="concept-label"></div>
                        <div class="concept-value"></div>
                    </div>
                    
                    <div class="concept-row">
                        <div class="concept-label"></div>
                        <div class="concept-value"></div>
                    </div>
                    
                    <div class="total-row">
                        <div class="total-label">Total de deducciones</div>
                        <div class="total-value"><asp:Literal ID="litTotalDeducciones" runat="server"></asp:Literal></div>
                    </div>
                </div>
            </div>

            <!-- RESUMEN FINAL -->
            <div class="summary-section">
                <div class="summary-row">
                    <div class="summary-label">Subtotal</div>
                    <div class="summary-value"><asp:Literal ID="litSubtotal" runat="server"></asp:Literal></div>
                </div>
                <div class="summary-row">
                    <div class="summary-label">Descuento</div>
                    <div class="summary-value"><asp:Literal ID="litDescuento" runat="server"></asp:Literal></div>
                </div>
                <div class="summary-row neto-row">
                    <div class="summary-label">Neto a pagar</div>
                    <div class="summary-value"><asp:Literal ID="litNetoPagar" runat="server"></asp:Literal></div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
