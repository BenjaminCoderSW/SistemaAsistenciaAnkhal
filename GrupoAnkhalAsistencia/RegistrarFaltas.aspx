<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="RegistrarFaltas.aspx.cs" Inherits="GrupoAnkhalAsistencia.RegistrarFaltas" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="css/gridviewPantalla.css" rel="stylesheet" />
    <script src="scriptspropios/sweetalert2@11.js"></script>
    <style>
        .section-title  { color: #003366; font-weight: 700; border-left: 4px solid #003366; padding-left: 10px; margin-bottom: 20px; }
        .card-accion    { background: #fff; border-radius: 12px; padding: 28px; box-shadow: 0 2px 10px rgba(0,0,0,0.09); margin-bottom: 24px; }
        .info-box       { border-radius: 8px; padding: 14px 18px; margin-bottom: 0; font-size: 14px; }
        .resumen-card   { border-radius: 10px; padding: 18px 20px; color: #fff; text-align: center; }
        .resumen-num    { font-size: 38px; font-weight: 700; line-height: 1; }
        .resumen-lbl    { font-size: 13px; opacity: .9; margin-top: 6px; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <h2 class="section-title">Registro de Faltas</h2>

    <!-- Tarjetas de resumen -->
    <div class="row mb-4">
        <div class="col-md-4">
            <div class="resumen-card" style="background:#dc3545;">
                <div class="resumen-num"><asp:Label ID="lblTotalFaltasHoy" runat="server" Text="—" /></div>
                <div class="resumen-lbl">Faltas registradas hoy</div>
            </div>
        </div>
        <div class="col-md-4">
            <div class="resumen-card" style="background:#003366;">
                <div class="resumen-num"><asp:Label ID="lblTotalFaltasMes" runat="server" Text="—" /></div>
                <div class="resumen-lbl">Faltas en el mes actual</div>
            </div>
        </div>
        <div class="col-md-4">
            <div class="resumen-card" style="background:#6c757d;">
                <div class="resumen-num"><asp:Label ID="lblUltimaEjecucion" runat="server" Text="—" /></div>
                <div class="resumen-lbl">Ultima fecha procesada</div>
            </div>
        </div>
    </div>

    <!-- Panel de accion -->
    <div class="card-accion">
        <div class="row align-items-end">

            <div class="col-md-4">
                <label class="form-label fw-semibold">Fecha a procesar</label>
                <asp:TextBox ID="txtFechaProcesar" runat="server"
                    CssClass="form-control"
                    TextMode="Date" />
                <small class="text-muted">Por defecto se usa la fecha de hoy.</small>
            </div>

            <div class="col-md-4 mt-3">
                <asp:Button ID="btnRegistrarFaltas" runat="server"
                    Text="Registrar Faltas"
                    CssClass="btn btn-danger btn-lg px-4"
                    OnClientClick="return confirmarProceso(this);"
                    OnClick="btnRegistrarFaltas_Click" />
            </div>

            <div class="col-md-4 mt-3">
                <div class="alert alert-warning mb-0 py-2">
                    <i class="fas fa-exclamation-triangle"></i>
                    Ejecuta esto al <strong>final del dia</strong> cuando ya no haya
                    posibilidad de que los empleados chequen.
                </div>
            </div>

        </div>
    </div>

    <!-- Resultado del ultimo proceso -->
    <asp:Panel ID="pnlResultado" runat="server" Visible="false">
        <div class="card-accion">
            <h5 class="fw-bold text-success mb-3">Resultado del proceso</h5>
            <asp:Label ID="lblResultado" runat="server" CssClass="fs-5" />
        </div>
    </asp:Panel>

    <!-- Historial de faltas recientes -->
    <div class="card-accion">
        <h5 class="fw-bold mb-3" style="color:#003366;">Faltas registradas recientes</h5>

        <div class="row mb-3">
            <div class="col-md-3">
                <asp:TextBox ID="txtFiltroInicio" runat="server" CssClass="form-control" TextMode="Date" />
            </div>
            <div class="col-md-3">
                <asp:TextBox ID="txtFiltroFin" runat="server" CssClass="form-control" TextMode="Date" />
            </div>
            <div class="col-md-3">
                <asp:TextBox ID="txtFiltroEmpleado" runat="server" CssClass="form-control"
                    Placeholder="Buscar empleado..." />
            </div>
            <div class="col-md-3">
                <asp:Button ID="btnFiltrar" runat="server" Text="Filtrar"
                    CssClass="btn btn-primary w-100"
                    OnClick="btnFiltrar_Click" />
            </div>
        </div>

        <div class="table-responsive">
            <asp:GridView ID="dvgFaltas" runat="server" AutoGenerateColumns="False"
                CssClass="table table-bordered table-striped custom-grid"
                AllowPaging="True" PageSize="15"
                OnPageIndexChanging="dvgFaltas_PageIndexChanging"
                EmptyDataText="No hay faltas registradas en el periodo seleccionado.">
                <Columns>
                    <asp:BoundField DataField="Empleado"      HeaderText="Empleado" />
                    <asp:BoundField DataField="NumeroEmpleado" HeaderText="N Empleado" />
                    <asp:BoundField DataField="Planta"        HeaderText="Planta" />
                    <asp:BoundField DataField="Fecha"         HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                    <asp:BoundField DataField="HorarioInicio" HeaderText="Hora Programada Inicio" />
                    <asp:BoundField DataField="HorarioFin"    HeaderText="Hora Programada Fin" />
                    <asp:TemplateField HeaderText="Estatus">
                        <ItemTemplate>
                            <span style="background:#dc3545;color:#fff;padding:3px 10px;border-radius:4px;font-size:12px;">
                                Falta
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>

    <script>
        function confirmarProceso(btn) {
            var fecha = document.getElementById('<%= txtFechaProcesar.ClientID %>').value;
            var fechaTexto = fecha ? fecha : 'hoy';

            Swal.fire({
                title: 'Registrar faltas',
                html: 'Se registrara una falta a todos los empleados que <strong>no checaron</strong> el <strong>' + fechaTexto + '</strong>.<br><br>Esta accion no afecta a empleados que ya tienen registro.',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#dc3545',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Si, registrar faltas',
                cancelButtonText: 'Cancelar'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack(btn.name, '');
                }
            });
            return false;
        }
    </script>

</asp:Content>
