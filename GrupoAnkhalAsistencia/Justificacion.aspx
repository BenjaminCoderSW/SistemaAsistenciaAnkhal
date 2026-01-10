<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Justificacion.aspx.cs" Inherits="GrupoAnkhalAsistencia.Justificacion" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
                              <link href="css/gridviewPantalla.css" rel="stylesheet" />
<script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

<script src="scriptspropios/propios.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h2>Gestión de retardo</h2>
  <br />

<div class="table-responsive">
    <div class="col-md-6"> 
      <div class="row">
    <div class="col-md-3">
        <asp:TextBox ID="txtFechaInicio" runat="server" CssClass="form-control" TextMode="Date" />
    </div>

    <div class="col-md-3">
        <asp:TextBox ID="txtFechaFin" runat="server" CssClass="form-control" TextMode="Date" />
    </div>

    <div class="col-md-2">
        <asp:Button ID="btnBuscarFechas" runat="server" Text="Buscar"
            CssClass="btn btn-success btn-block"
            OnClick="btnBuscarFechas_Click" />
    </div>
</div>

<br />

    </div>

    <br />

    <asp:GridView ID="dvgJustificaionHoras" runat="server" AutoGenerateColumns="False"
        CssClass="table table-bordered table-striped custom-grid"
        AllowPaging="True" PageSize="5"
        OnPageIndexChanging="dvgJustificaion_PageIndexChanging">
        <Columns>

            <asp:BoundField DataField="IdAsistencia" HeaderText="IdAsistencia" />
            <asp:BoundField DataField="NombreCompleto" HeaderText="NombreCompleto" />
            <asp:BoundField DataField="Planta" HeaderText="Planta" />
            <asp:BoundField DataField="Horario" HeaderText="Horario" />
            <asp:BoundField DataField="IdJustificacion" HeaderText="IdJustificacion" />
            <asp:BoundField DataField="Justificacion" HeaderText="Justificacion" />
            <asp:BoundField DataField="Fecha" HeaderText="Fecha" />
            <asp:BoundField DataField="HoraEntrada" HeaderText="HoraEntrada" />
            <asp:BoundField DataField="HoraSalida" HeaderText="HoraSalida" />
            <asp:BoundField DataField="EstatusEntrada" HeaderText="EstatusEntrada" />
            <asp:BoundField DataField="EstatusSalida" HeaderText="EstatusSalida" />

            <asp:TemplateField HeaderText="Acciones">
                <ItemTemplate>

                    <asp:Button ID="btnJustificar" 
                                runat="server" 
                                Text="Justificar" 
                                CssClass="btn btn-primary"
                                CommandArgument='<%# Eval("IdAsistencia") %>' 
                                OnClick="btnJustificar_Click" 
                      Visible='<%# (Eval("Justificacion") == null 
            || Eval("Justificacion").ToString() == "" 
            || Eval("Justificacion").ToString() == "Pendiente")
            ? true : false %>' />

                </ItemTemplate>
            </asp:TemplateField>

        </Columns>
    </asp:GridView>
</div>
   <div class="modal fade" id="modalJustificacion" tabindex="-1">
    <div class="modal-dialog modal-dialog-centered modal-lg">
        <div class="modal-content shadow-lg border-0" style="border-radius: 18px;">

            <!-- HEADER -->
            <div class="modal-header" style="background: linear-gradient(135deg, #1f2c3e, #1f2c3e); color:white; border-radius: 18px 18px 0 0;">
                <h5 class="modal-title fw-bold">
                    <i class="bi bi-file-earmark-text me-2"></i> Registrar Justificación
                </h5>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
            </div>

            <!-- BODY -->
            <div class="modal-body p-4">
                <asp:HiddenField ID="hfIdAsistencia" runat="server" />

                <!-- Empleado -->
                <div class="mb-4">
                    <label class="form-label fw-semibold">
                        <i class="bi bi-person-circle me-1"></i> Empleado
                    </label>
                    <asp:TextBox ID="txtNombreEmpleado" runat="server"
                        CssClass="form-control form-control-lg"
                        ReadOnly="true"
                        Style="background:#f8f9fa; border-radius:10px;" />
                </div>

                <!-- Motivo -->
                <div class="mb-4">
                    <label class="form-label fw-semibold">
                        <i class="bi bi-list-check me-1"></i> Motivo
                    </label>
                    <asp:DropDownList ID="ddlMotivo" runat="server"
                        CssClass="form-control form-select form-select-lg"
                        Style="border-radius:10px;">
                        <asp:ListItem Value="">-- Selecciona un motivo --</asp:ListItem>
                        <asp:ListItem Value="Transporte">Transporte</asp:ListItem>
                        <asp:ListItem Value="Tráfico">Tráfico</asp:ListItem>
                        <asp:ListItem Value="Motivo de salud">Motivo de salud</asp:ListItem>
                        <asp:ListItem Value="Falla de vehículo">Falla de vehículo</asp:ListItem>
                        <asp:ListItem Value="Asunto familiar">Asunto familiar</asp:ListItem>
                        <asp:ListItem Value="Olvido de hora">Olvido de hora</asp:ListItem>
                        <asp:ListItem Value="Clima / lluvia">Clima / lluvia</asp:ListItem>
                        <asp:ListItem Value="Otro">Otro</asp:ListItem>
                    </asp:DropDownList>
                </div>

                <!-- Comentarios -->
                <div class="mb-3">
                    <label class="form-label fw-semibold">
                        <i class="bi bi-chat-left-text me-1"></i> Comentarios
                    </label>
                    <asp:TextBox ID="txtComentarios" TextMode="MultiLine" runat="server"
                        CssClass="form-control"
                        Style="border-radius:10px; height:120px; resize:none;" />
                </div>
            </div>

            <!-- FOOTER -->
            <div class="modal-footer d-flex justify-content-between px-4 pb-4">

                <asp:Button ID="btnGuardarJustificacion"
                            runat="server"
                            Text="Guardar"
                            CssClass="btn btn-primary btn-lg px-4 shadow-sm"
                            Style="border-radius:12px;"
                            OnClick="btnGuardarJustificacion_Click" />

                <button type="button" class="btn btn-outline-secondary btn-lg px-4"
                        style="border-radius:12px;"
                        data-bs-dismiss="modal">
                    Cancelar
                </button>

            </div>

        </div>
    </div>
</div>

    <script>
        function abrirModalJustificar() {
            var myModal = new bootstrap.Modal(document.getElementById('modalJustificacion'));
            myModal.show();
        }

        function cerrarModalJustificar() {
            var inst = bootstrap.Modal.getInstance(document.getElementById('modalJustificacion'));
            if (inst) inst.hide();
        }
    </script>

</asp:Content>
