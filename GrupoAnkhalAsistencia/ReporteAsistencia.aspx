<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="ReporteAsistencia.aspx.cs" Inherits="GrupoAnkhalAsistencia.ReporteAsistencia" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="css/gridviewPantalla.css" rel="stylesheet" />
    <script src="scriptspropios/sweetalert2@11.js"></script>
    <script src="scriptspropios/propios.js"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <h2>Reporte Asistencia</h2>

    <br />

    <!-- FILTROS -->
    <div class="row">
        <div class="col-md-2">
            <label for="txtFechaInicio" class="font-weight-bold">Fecha Inicio:</label>
            <asp:TextBox ID="txtFechaInicio" runat="server" CssClass="form-control"
                TextMode="Date" />
        </div>

        <div class="col-md-2">
            <label for="txtFechaFin" class="font-weight-bold">Fecha Fin:</label>
            <asp:TextBox ID="txtFechaFin" runat="server" CssClass="form-control"
                TextMode="Date" />
        </div>

        <div class="col-md-3">
            <label for="txtEmpleado">Empleado:</label>
            <asp:TextBox ID="txtEmpleado" runat="server" CssClass="form-control"
                Placeholder="Empleado..." />
        </div>

        <div class="col-md-3">
            <label>Planta:</label>
            <asp:DropDownList ID="ddlPlanta" runat="server" CssClass="form-control" />
        </div>

        <div class="col-md-2 d-flex align-items-end">
            <asp:Button ID="btnFiltrar" runat="server" CssClass="btn btn-primary"
                Text="Filtrar" OnClick="btnFiltrar_Click" />
        </div>
    </div>

    <br />

    <!-- BUSCADOR ORIGINAL -->
    <div class="col-md-6">

        <br /><br />

        <asp:Button ID="btnExportExcel" runat="server" Text="Exportar a Excel"
            CssClass="btn btn-success" OnClick="btnExportExcel_Click" />

        <asp:Button ID="btnExportPDF" runat="server" Text="Exportar a PDF"
            CssClass="btn btn-danger" OnClick="btnExportPDF_Click" />

        <br /><br />

    </div>

    <br />

    <!-- GRID -->
    <div class="table-responsive">
        <asp:GridView ID="dvgHistorialEmpleado" runat="server" AutoGenerateColumns="False"
            CssClass="table table-bordered table-striped custom-grid"
            AllowPaging="True" PageSize="5"
            OnPageIndexChanging="dvgHistorialEmpleado_PageIndexChanging">

            <Columns>

                <asp:BoundField DataField="EMPLEADO" HeaderText="EMPLEADO" />
                <asp:BoundField DataField="Planta" HeaderText="Planta" />
                <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:BoundField DataField="HoraEntrada" HeaderText="HoraEntrada" />
                <asp:BoundField DataField="HoraSalida" HeaderText="HoraSalida" />
                <asp:BoundField DataField="HoraSalidaComer" HeaderText="HoraSalidaComer" />
                <asp:BoundField DataField="HoraEntradaComer" HeaderText="HoraEntradaComer" />
                <asp:BoundField DataField="HorasTrabajadas" HeaderText="HorasTrabajadas" />
                <asp:BoundField DataField="tiempoComida" HeaderText="Tiempo Comida" />

                <asp:BoundField DataField="EstatusEntrada" HeaderText="EstatusEntrada" />
                <asp:BoundField DataField="EstatusSalida" HeaderText="EstatusSalida" />
                <asp:BoundField DataField="EstatusComida" HeaderText="EstatusComida" />

                <asp:BoundField DataField="TipoPermiso" HeaderText="TipoPermiso" />
                <asp:BoundField DataField="HoraSalidaPermiso" HeaderText="HoraSalidaPermiso" />
                <asp:BoundField DataField="HoraEntradaPermiso" HeaderText="HoraEntradaPermiso" />
                <asp:BoundField DataField="HorasPermiso" HeaderText="HorasPermiso" DataFormatString="{0:N2}" />

                <asp:BoundField DataField="horaSalidaComision" HeaderText="horaSalidaComision" />
                <asp:BoundField DataField="horaEntradaComision" HeaderText="horaEntradaComision" />
                <asp:BoundField DataField="horasComision" HeaderText="horasComision" />

                <asp:BoundField DataField="HorasExtras" HeaderText="Horas Extras" DataFormatString="{0:N2}" />
                <asp:BoundField DataField="EstatusHorasExtras" HeaderText="Estatus Horas Extras" />

                <asp:TemplateField HeaderText="Ubicación Entrada">
                    <ItemTemplate>
                        <%# GetMapaLink(Eval("UbicacionEntrada")?.ToString()) %>
                        <%# GetPlantaHtml(Eval("UbicacionEntrada")?.ToString()) %>
                        <%# GetSinGpsHtml(Eval("UbicacionEntrada")?.ToString(), Eval("EstatusEntrada")?.ToString()) %>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Ubicación Salida">
                    <ItemTemplate>
                        <%# GetMapaLink(Eval("UbicacionSalida")?.ToString()) %>
                        <%# GetPlantaHtml(Eval("UbicacionSalida")?.ToString()) %>
                        <%# GetSinGpsHtml(Eval("UbicacionSalida")?.ToString(), Eval("EstatusSalida")?.ToString()) %>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Selfies">
                    <ItemTemplate>
                        <%# GetFotosHtml(Eval("IdAsistencia")) %>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:BoundField DataField="MacEntrada" HeaderText="MacEntrada" />
                <asp:BoundField DataField="MacSalida" HeaderText="MacSalida" />
                <asp:BoundField DataField="IP" HeaderText="IP" />

                <asp:TemplateField HeaderText="Estatus Checada">
                    <ItemTemplate>
                        <%# GetEstatusChecada(
                                Eval("HoraEntrada"),
                                Eval("HoraSalida"),
                                Eval("HoraSalidaComer"),
                                Eval("HoraEntradaComer")
                            ) %>
                    </ItemTemplate>
                </asp:TemplateField>

            </Columns>

        </asp:GridView>
    </div>

    <!-- MODAL SELFIE -->
    <div class="modal fade" id="modalFotoSelfie" tabindex="-1" role="dialog" aria-hidden="true">
        <div class="modal-dialog modal-sm" role="document">
            <div class="modal-content">
                <div class="modal-header" style="background-color:#0b3360;">
                    <h5 class="modal-title text-white" id="modalFotoTitulo">Selfie del empleado</h5>
                    <button type="button" class="close text-white" data-dismiss="modal"><span>&times;</span></button>
                </div>
                <div class="modal-body text-center p-2">
                    <img id="imgSelfieVista" src="" style="max-width:100%; border-radius:6px;"
                         onerror="this.style.display='none'; document.getElementById('divSelfieError').style.display='block';" />
                    <div id="divSelfieError" style="display:none; color:#dc3545; font-size:13px;">No se pudo cargar la foto.</div>
                </div>
                <div class="modal-footer p-2">
                    <button type="button" class="btn btn-sm btn-secondary" data-dismiss="modal">Cerrar</button>
                </div>
            </div>
        </div>
    </div>
    <script>
        function verFoto(idAsistencia, tipo) {
            document.getElementById('modalFotoTitulo').innerText = tipo === 'entrada' ? 'Selfie de Entrada' : 'Selfie de Salida';
            var img = document.getElementById('imgSelfieVista');
            img.style.display = 'block';
            document.getElementById('divSelfieError').style.display = 'none';
            img.src = 'FotoAsistencia.ashx?id=' + idAsistencia + '&tipo=' + tipo + '&t=' + Date.now();
            $('#modalFotoSelfie').modal('show');
        }
    </script>

</asp:Content>