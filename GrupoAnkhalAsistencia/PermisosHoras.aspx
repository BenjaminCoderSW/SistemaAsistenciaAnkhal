<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PermisosHoras.aspx.cs" Inherits="GrupoAnkhalAsistencia.PermisosHoras" %>
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
 <!-- CONTENIDO PRINCIPAL -->
<div class="container-fluid py-4">
  <div class="row justify-content-center">
    <div class="col-12 col-lg-10 col-xl-8">

      <div class="card shadow-lg border-0 rounded-3">
        <div class="card-header text-white fw-bold d-flex align-items-center" style="background-color: #003366;">
          <i class="fa fa-clock me-2"></i> Permiso por horas
        </div>

        <div class="card-body bg-light">
          <asp:Panel ID="pnlPermiso" runat="server">
            <asp:ValidationSummary ID="valSummary" runat="server" CssClass="text-danger" />

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
              <div class="col-md-6">
                <label class="form-label fw-semibold">Correo Jefe (*)</label>
                <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control"
                  Placeholder="Correo del jefe" ReadOnly="true"></asp:TextBox>
              </div>

              <div class="col-md-6">
                <label class="form-label fw-semibold">Fecha Permiso</label>
                  <asp:TextBox ID="txtDia" runat="server" CssClass="form-control" placeholder="Seleccione fecha"></asp:TextBox>
                  <ajaxToolkit:CalendarExtender ID="CalendarExtender2" runat="server" TargetControlID="txtDia" Format="dd/MM/yyyy"></ajaxToolkit:CalendarExtender>
              </div>
            </div>

            <div class="row mb-3">
              <div class="col-md-6">
                <label class="form-label fw-semibold">Hora Inicio</label>
              <asp:TextBox ID="txtHoraInicio" runat="server" TextMode="Time"  CssClass="form-control" AutoPostBack="true" OnTextChanged="CalcularHoras" />

              </div>

              <div class="col-md-6">
                <label class="form-label fw-semibold">Hora Fin</label>
                <asp:TextBox ID="txtHoraFin" runat="server" TextMode="Time"  CssClass="form-control" AutoPostBack="true" OnTextChanged="CalcularHoras" />
              </div>
            </div>

            <div class="row mb-3">
              <div class="col-md-6">
                <label class="form-label fw-semibold">¿Cuántas horas?</label>
                <asp:TextBox ID="txtHoras" runat="server" TextMode="Number" CssClass="form-control" ReadOnly="true" Placeholder="0.0"></asp:TextBox>

              </div>

              <div class="col-md-6">
                <label class="form-label fw-semibold">Tipo permiso</label>
                <asp:TextBox ID="txtTipoPermiso" runat="server" CssClass="form-control"
                  Text="Permiso por horas" ReadOnly="true"></asp:TextBox>
              </div>
            </div>

            <div class="row mb-4">
              <div class="col-12">
                <label class="form-label fw-semibold">Motivo</label>
                <asp:DropDownList ID="ddlMotivo" runat="server" CssClass="form-control">
                  <asp:ListItem Text="Seleccione..." Value="" />
                  <asp:ListItem Text="Personal" Value="Personal" />
                  <asp:ListItem Text="Consulta Médica IMSS" Value="Consulta Médica IMSS" />
                  <asp:ListItem Text="Tramites" Value="Tramites" />
                  <asp:ListItem Text="Finiquitos" Value="Finiquitos" />
                  <asp:ListItem Text="Banco Apertura" Value="Banco Apertura" />
                </asp:DropDownList>
              </div>
            </div>
            
            <div class="text-end">
              <asp:Button ID="btnRegistrar" runat="server" CssClass="btn btn-primary px-4 py-2 fw-semibold"
                Text="Registrar permiso" OnClick="btnRegistrar_Click" />
            </div>

          </asp:Panel>
        </div>

      </div>
    </div>
  </div>
</div>


<!-- ESTILOS PERSONALIZADOS -->
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

</asp:Content>
