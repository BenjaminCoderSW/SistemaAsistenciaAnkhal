<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ImprimirActaResumenHorasExtra.aspx.cs" Inherits="GrupoAnkhalAsistencia.ImprimirActaResumenHorasExtra" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>Acta Resumen de Horas Extra por Empleado</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }

        body {
            font-family: Arial, sans-serif;
            font-size: 11px;
            padding: 20px;
            background: #f5f5f5;
        }

        .container {
            width: 100%;
            max-width: 900px;
            margin: 0 auto;
            border: 2px solid #000;
            background: #fff;
        }

        .header {
            padding: 10px 15px;
            display: flex;
            justify-content: space-between;
            align-items: center;
            border-bottom: 2px solid #000;
        }
        .header-title {
            font-weight: bold;
            font-size: 13px;
            margin-bottom: 3px;
        }
        .header-subtitle {
            font-size: 15px;
            font-weight: bold;
            color: #003366;
            margin-top: 6px;
            text-transform: uppercase;
            letter-spacing: 1px;
        }
        .logo { max-width: 150px; height: auto; }

        .info-section {
            background-color: #d9d9d9;
            padding: 7px 15px;
            border-bottom: 1px solid #000;
        }
        .info-row {
            display: flex;
            margin-bottom: 3px;
            line-height: 1.5;
        }
        .info-row:last-child { margin-bottom: 0; }
        .info-label { font-weight: bold; width: 160px; display: inline-block; }
        .info-value { flex: 1; }

        .table-section table {
            width: 100%;
            border-collapse: collapse;
            font-size: 11px;
        }
        .table-section thead tr {
            background-color: #003366;
            color: #fff;
        }
        .table-section thead th {
            padding: 6px 8px;
            border: 1px solid #000;
            text-align: center;
        }
        .table-section tbody td {
            padding: 5px 8px;
            border: 1px solid #ccc;
            vertical-align: middle;
        }
        .table-section tbody tr:nth-child(even) { background-color: #f9f9f9; }
        .row-empleado { background-color: #d4edda !important; }

        .totales-section {
            background-color: #d9d9d9;
            padding: 7px 15px;
            border-top: 2px solid #000;
            display: flex;
            gap: 30px;
            font-weight: bold;
            font-size: 12px;
        }

        .firmas-section {
            padding: 40px 60px 25px 60px;
            border-top: 1px solid #ccc;
            display: flex;
            justify-content: space-around;
        }
        .firma-box { text-align: center; width: 220px; }
        .firma-linea {
            border-top: 1px solid #000;
            margin-bottom: 6px;
            margin-top: 50px;
        }
        .firma-rol { font-weight: bold; font-size: 11px; text-transform: uppercase; }
        .firma-nombre { font-size: 10px; color: #444; margin-top: 2px; }

        .sin-datos {
            text-align: center;
            padding: 30px;
            font-style: italic;
            color: #666;
            font-size: 13px;
        }

        .btn-accion {
            padding: 10px 20px;
            border: none;
            border-radius: 5px;
            cursor: pointer;
            font-size: 14px;
            margin-right: 10px;
        }
        .btn-imprimir { background-color: #003366; color: white; }
        .btn-imprimir:hover { background-color: #004488; }
        .btn-cerrar { background-color: #666; color: white; }
        .btn-cerrar:hover { background-color: #444; }

        @media print {
            body { padding: 0; background: #fff; }
            .no-print { display: none !important; }
            @page { margin: 1.5cm; }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">

        <div class="no-print" style="text-align:center; margin-bottom:16px;">
            <button type="button" class="btn-accion btn-imprimir" onclick="window.print();">
                &#128438; Imprimir Acta
            </button>
            <button type="button" class="btn-accion btn-cerrar" onclick="window.close();">
                &#10006; Cerrar
            </button>
        </div>

        <div class="container">

            <div class="header">
                <div>
                    <div class="header-title">GRUPO ANKHAL</div>
                    <div class="header-subtitle">Acta Resumen de Horas Extra por Empleado</div>
                </div>
                <div>
                    <img src="img/ankhal.png" alt="Logo Grupo Ankhal" class="logo" />
                </div>
            </div>

            <div class="info-section">
                <div class="info-row">
                    <span class="info-label">Jefe de Planta:</span>
                    <span class="info-value"><asp:Literal ID="litJefePlanta" runat="server" /></span>
                </div>
                <div class="info-row">
                    <span class="info-label">Planta:</span>
                    <span class="info-value"><asp:Literal ID="litPlanta" runat="server" /></span>
                </div>
                <div class="info-row">
                    <span class="info-label">Per&iacute;odo:</span>
                    <span class="info-value"><asp:Literal ID="litPeriodo" runat="server" /></span>
                </div>
                <div class="info-row">
                    <span class="info-label">Fecha de emisi&oacute;n:</span>
                    <span class="info-value"><asp:Literal ID="litFechaEmision" runat="server" /></span>
                </div>
            </div>

            <div class="table-section">
                <asp:Literal ID="litTablaResumen" runat="server" />
            </div>

            <asp:Literal ID="litTotales" runat="server" />

            <div class="firmas-section">
                <div class="firma-box">
                    <div class="firma-linea"></div>
                    <div class="firma-rol">Jefe de Planta</div>
                    <div class="firma-nombre"><asp:Literal ID="litFirmaNombre" runat="server" /></div>
                </div>
                <div class="firma-box">
                    <div class="firma-linea"></div>
                    <div class="firma-rol">Nombre y Firma RH</div>
                </div>
            </div>

        </div>
    </form>
</body>
</html>
