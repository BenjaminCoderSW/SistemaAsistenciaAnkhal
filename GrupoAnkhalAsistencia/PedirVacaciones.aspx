<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PedirVacaciones.aspx.cs" Inherits="GrupoAnkhalAsistencia.PedirVacaciones" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .card-permiso {
            max-width: 800px;
            margin: 40px auto;
            border-radius: 10px;
            box-shadow: 0 4px 8px rgba(0,0,0,0.1);
        }
        .card-header {
            background-color: #003366 !important;
            color: #fff !important;
            font-weight: 600;
            text-align: center;
            border-radius: 10px 10px 0 0 !important;
        }
        .form-label {
            font-weight: 500;
            color: #333;
        }
        .form-control, .form-select {
            border-radius: 6px !important;
        }
        .btn-primary {
            background-color: #0066cc;
            border: none;
            transition: all 0.3s ease;
        }
        .btn-primary:hover {
            background-color: #004d99;
        }
        .row > div {
            margin-bottom: 15px;
        }
    </style>
    <script src="scriptspropios/sweetalert2@11.js"></script>
    <script src="scriptspropios/propios.js"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container-fluid py-4">
        <div class="row justify-content-center">
            <div class="col-12 col-lg-10 col-xl-8">
                <div class="card shadow-lg border-0 rounded-3">
                    <div class="card-header text-white fw-bold d-flex align-items-center" style="background-color: #003366;">
                        <i class="fa fa-calendar me-2"></i> Solicitud de Vacaciones
                    </div>

                    <div class="card-body bg-light">
                        <asp:Panel ID="pnlVacaciones" runat="server">
                            
                            <div class="row mb-3">
                                <div class="col-md-6">
                                    <label class="form-label fw-semibold">Empleado (*)</label>
                                    <asp:TextBox ID="txtNombreEmpleado" runat="server" CssClass="form-control"
                                        ReadOnly="true" Placeholder="Nombre del empleado"></asp:TextBox>
                                </div>

                                <div class="col-md-6">
                                    <label class="form-label fw-semibold">Jefe (*)</label>
                                    <asp:DropDownList ID="ddlNombreJefe" runat="server"
                                        AutoPostBack="true"
                                        CssClass="form-control"
                                        OnSelectedIndexChanged="ddlNombreJefe_SelectedIndexChanged">
                                    </asp:DropDownList>
                                </div>
                            </div>

                            <div class="row mb-3">
                                <div class="col-md-12">
                                    <label class="form-label fw-semibold">Correo Jefe (*)</label>
                                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control"
                                        Placeholder="Correo del jefe" ReadOnly="true"></asp:TextBox>
                                </div>

                                <div class="col-md-6">
                                    <label class="form-label fw-semibold">Fecha Inicio</label>
                                    <asp:TextBox ID="txtFechaInicio" runat="server" CssClass="form-control" placeholder="Seleccione fecha"></asp:TextBox>
                                    <ajaxToolkit:CalendarExtender ID="CalendarExtender1" runat="server" TargetControlID="txtFechaInicio" Format="dd/MM/yyyy"></ajaxToolkit:CalendarExtender>
                                </div>

                                <div class="col-md-6">
                                    <label class="form-label fw-semibold">Fecha Fin</label>
                                    <asp:TextBox ID="txtFechaFin" runat="server" CssClass="form-control" placeholder="Seleccione fecha"></asp:TextBox>
                                    <ajaxToolkit:CalendarExtender ID="CalendarExtender2" runat="server" TargetControlID="txtFechaFin" Format="dd/MM/yyyy"></ajaxToolkit:CalendarExtender>
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-6">
                                    <label class="form-label fw-semibold">¿Cuántos días?</label>
                                    <asp:TextBox ID="txtDias" runat="server" CssClass="form-control" Enabled="false" Placeholder="0"></asp:TextBox>
                                    <asp:HiddenField ID="hdnDias" runat="server" />
                                </div>

                                <div class="col-md-6">
                                    <label class="form-label fw-semibold">Días Autorizados</label>
                                    <asp:TextBox ID="txtDiasAutorizados" runat="server" CssClass="form-control" ReadOnly="true" Placeholder="0"></asp:TextBox>
                                </div>
                            </div>

                            <br />
                            <div class="text-end">
                                <asp:Button ID="btnRegistrar" runat="server" CssClass="btn btn-primary px-4 py-2 fw-semibold"
                                    Text="Solicitar vacaciones" OnClick="btnRegistrar_Click" />
                            </div>

                        </asp:Panel>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <style>
        body {
            background-color: #f5f7fa !important;
        }
        .card {
            transition: all 0.2s ease-in-out;
        }
        .card:hover {
            transform: translateY(-3px);
        }
        .form-label {
            color: #003366;
        }
        .btn-primary {
            background-color: #003366 !important;
            border: none;
        }
        .btn-primary:hover {
            background-color: #00224e !important;
        }
        .form-control:focus, .form-select:focus {
            border-color: #003366;
            box-shadow: 0 0 0 0.15rem rgba(0, 51, 102, 0.25);
        }
    </style>

    <script>
        document.addEventListener("DOMContentLoaded", function () {
            const fechaInicio = document.getElementById("<%= txtFechaInicio.ClientID %>");
            const fechaFin = document.getElementById("<%= txtFechaFin.ClientID %>");
            const txtDias = document.getElementById("<%= txtDias.ClientID %>");
            const hdnDias = document.getElementById("<%= hdnDias.ClientID %>");

            function calcularDias() {
                const inicio = fechaInicio.value.split('/');
                const fin = fechaFin.value.split('/');

                if (inicio.length === 3 && fin.length === 3) {
                    const fecha1 = new Date(inicio[2], inicio[1] - 1, inicio[0]);
                    const fecha2 = new Date(fin[2], fin[1] - 1, fin[0]);

                    if (fecha2 >= fecha1) {
                        const diferencia = Math.ceil((fecha2 - fecha1) / (1000 * 60 * 60 * 24)) + 1;
                        txtDias.value = diferencia;
                        hdnDias.value = diferencia;
                    } else {
                        txtDias.value = "";
                        hdnDias.value = "";
                    }
                }
            }

            fechaInicio.addEventListener("change", calcularDias);
            fechaFin.addEventListener("change", calcularDias);
        });
    </script>
</asp:Content>