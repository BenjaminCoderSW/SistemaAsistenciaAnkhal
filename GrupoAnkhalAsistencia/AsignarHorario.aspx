<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AsignarHorario.aspx.cs" Inherits="GrupoAnkhalAsistencia.AsignarHorario" EnableEventValidation="false" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="css/gridviewPantalla.css" rel="stylesheet" />
    <script src="scriptspropios/sweetalert2@11.js"></script>
    <style>
        .tabla-semana th, .tabla-semana td { text-align: center; font-size: 0.83rem; }
        .dia-asignado { color: #155724; font-weight: 600; }
        .dia-vacio    { color: #aaa; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h2>Asignaci&oacute;n de Horarios</h2>

    <asp:Button ID="btnAgregar" runat="server" Text="Agregar / Editar Empleado"
        CssClass="btn btn-primary mb-3" OnClientClick="abrirModal(); return false;" />

    <!-- Botón proxy oculto para postback desde JS -->
    <asp:Button ID="btnCargarEmpleado" runat="server" Style="display:none;"
        OnClick="btnCargarEmpleado_Click" />

    <div class="table-responsive">
        <div class="col-md-6">
            <asp:TextBox ID="txtBuscar" runat="server"
                CssClass="form-control"
                Placeholder="Buscar empleado..."
                AutoPostBack="true"
                OnTextChanged="txtBuscar_TextChanged" />
        </div>
        <br />

        <asp:GridView ID="dvgAsignacionHorario" runat="server" AutoGenerateColumns="False"
            CssClass="table table-bordered table-striped custom-grid tabla-semana"
            AllowPaging="True" PageSize="10"
            OnPageIndexChanging="dvgAsignacionHorario_PageIndexChanging">
            <Columns>
                <asp:TemplateField HeaderText="Empleado" ItemStyle-HorizontalAlign="Left">
                    <ItemTemplate>
                        <span class="truncate-text" title='<%# Eval("NombreCompleto") %>'>
                            <%# Eval("NombreCompleto") %>
                        </span>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Lun">
                    <ItemTemplate>
                        <span class='<%# string.IsNullOrEmpty((string)Eval("Lunes")) ? "dia-vacio" : "dia-asignado" %>'>
                            <%# string.IsNullOrEmpty((string)Eval("Lunes")) ? "&mdash;" : Eval("Lunes") %>
                        </span>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Mar">
                    <ItemTemplate>
                        <span class='<%# string.IsNullOrEmpty((string)Eval("Martes")) ? "dia-vacio" : "dia-asignado" %>'>
                            <%# string.IsNullOrEmpty((string)Eval("Martes")) ? "&mdash;" : Eval("Martes") %>
                        </span>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Mi&eacute;">
                    <ItemTemplate>
                        <span class='<%# string.IsNullOrEmpty((string)Eval("Miercoles")) ? "dia-vacio" : "dia-asignado" %>'>
                            <%# string.IsNullOrEmpty((string)Eval("Miercoles")) ? "&mdash;" : Eval("Miercoles") %>
                        </span>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Jue">
                    <ItemTemplate>
                        <span class='<%# string.IsNullOrEmpty((string)Eval("Jueves")) ? "dia-vacio" : "dia-asignado" %>'>
                            <%# string.IsNullOrEmpty((string)Eval("Jueves")) ? "&mdash;" : Eval("Jueves") %>
                        </span>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Vie">
                    <ItemTemplate>
                        <span class='<%# string.IsNullOrEmpty((string)Eval("Viernes")) ? "dia-vacio" : "dia-asignado" %>'>
                            <%# string.IsNullOrEmpty((string)Eval("Viernes")) ? "&mdash;" : Eval("Viernes") %>
                        </span>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="S&aacute;b">
                    <ItemTemplate>
                        <span class='<%# string.IsNullOrEmpty((string)Eval("Sabado")) ? "dia-vacio" : "dia-asignado" %>'>
                            <%# string.IsNullOrEmpty((string)Eval("Sabado")) ? "&mdash;" : Eval("Sabado") %>
                        </span>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Dom">
                    <ItemTemplate>
                        <span class='<%# string.IsNullOrEmpty((string)Eval("Domingo")) ? "dia-vacio" : "dia-asignado" %>'>
                            <%# string.IsNullOrEmpty((string)Eval("Domingo")) ? "&mdash;" : Eval("Domingo") %>
                        </span>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Acciones">
                    <ItemTemplate>
                        <button type="button" class="btn btn-warning btn-sm"
                            onclick="abrirModalSemana('<%# Eval("IdUsuario") %>')">
                            Editar semana
                        </button>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>


    <!-- MODAL UNIFICADO: Agregar / Editar semana completa de un empleado -->
    <div class="modal fade" id="modalSemana" tabindex="-1" role="dialog"
         aria-labelledby="modalSemanaLabel" aria-hidden="true">
      <div class="modal-dialog modal-lg" role="document">
        <div class="modal-content shadow-lg border-0">

          <div class="modal-header text-white" style="background-color: #003366;">
            <h5 class="modal-title" id="modalSemanaLabel">Horario Semanal del Empleado</h5>
            <button type="button" class="close text-white" data-dismiss="modal" aria-label="Cerrar">
              <span aria-hidden="true">&times;</span>
            </button>
          </div>

          <div class="modal-body">
            <asp:HiddenField ID="hfIdUsuarioEditar" runat="server" />

            <!-- Selector de empleado (para flujo "Agregar nuevo") -->
            <div class="form-group">
              <label><strong>Empleado</strong></label>
              <asp:DropDownList ID="ddlEmpleadoSemana" runat="server"
                  CssClass="form-control"
                  AutoPostBack="true"
                  OnSelectedIndexChanged="ddlEmpleadoSemana_SelectedIndexChanged" />
            </div>

            <!-- Tabla de 7 días con DDL de horario por día -->
            <div class="table-responsive mt-3">
              <table class="table table-sm table-bordered">
                <thead class="thead-light">
                  <tr>
                    <th style="width:130px">D&iacute;a</th>
                    <th>Horario asignado</th>
                  </tr>
                </thead>
                <tbody>
                  <tr>
                    <td><strong>Lunes</strong></td>
                    <td><asp:DropDownList ID="ddlLunes" runat="server" CssClass="form-control form-control-sm" /></td>
                  </tr>
                  <tr>
                    <td><strong>Martes</strong></td>
                    <td><asp:DropDownList ID="ddlMartes" runat="server" CssClass="form-control form-control-sm" /></td>
                  </tr>
                  <tr>
                    <td><strong>Mi&eacute;rcoles</strong></td>
                    <td><asp:DropDownList ID="ddlMiercoles" runat="server" CssClass="form-control form-control-sm" /></td>
                  </tr>
                  <tr>
                    <td><strong>Jueves</strong></td>
                    <td><asp:DropDownList ID="ddlJueves" runat="server" CssClass="form-control form-control-sm" /></td>
                  </tr>
                  <tr>
                    <td><strong>Viernes</strong></td>
                    <td><asp:DropDownList ID="ddlViernes" runat="server" CssClass="form-control form-control-sm" /></td>
                  </tr>
                  <tr>
                    <td><strong>S&aacute;bado</strong></td>
                    <td><asp:DropDownList ID="ddlSabado" runat="server" CssClass="form-control form-control-sm" /></td>
                  </tr>
                  <tr>
                    <td><strong>Domingo</strong></td>
                    <td><asp:DropDownList ID="ddlDomingo" runat="server" CssClass="form-control form-control-sm" /></td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          <div class="modal-footer bg-light">
            <asp:Button ID="btnGuardarSemana" runat="server" Text="Guardar"
                CssClass="btn btn-success px-4" OnClick="btnGuardarSemana_Click" />
            <button type="button" class="btn btn-secondary px-4" data-dismiss="modal">Cancelar</button>
          </div>

        </div>
      </div>
    </div>


    <script type="text/javascript">
        // Los startup scripts del servidor no pueden usar $ directamente porque jQuery
        // carga DESPUÉS del </form> en el Site.Master. Usamos window.load como puente.
        window.addEventListener('load', function () {
            if (window.__abrirModalSemana) {
                window.__abrirModalSemana = false;
                $('#modalSemana').modal('show');
            }
            if (window.__cerrarModalSemana) {
                window.__cerrarModalSemana = false;
                $('#modalSemana').modal('hide');
            }
        });

        // Abre el modal vacío para "Agregar nuevo" (llamado desde clic de botón, jQuery ya cargó)
        function abrirModal() {
            document.getElementById('<%= hfIdUsuarioEditar.ClientID %>').value = '0';
            var sel = document.getElementById('<%= ddlEmpleadoSemana.ClientID %>');
            if (sel) sel.value = '0';
            var ddlIds = [
                '<%= ddlLunes.ClientID %>',
                '<%= ddlMartes.ClientID %>',
                '<%= ddlMiercoles.ClientID %>',
                '<%= ddlJueves.ClientID %>',
                '<%= ddlViernes.ClientID %>',
                '<%= ddlSabado.ClientID %>',
                '<%= ddlDomingo.ClientID %>'
            ];
            ddlIds.forEach(function (id) {
                var el = document.getElementById(id);
                if (el) el.value = '0';
            });
            $('#modalSemana').modal('show');
        }

        // Editar desde grid: postback para que el servidor pre-cargue los DDLs
        function abrirModalSemana(idUsuario) {
            document.getElementById('<%= hfIdUsuarioEditar.ClientID %>').value = idUsuario;
            __doPostBack('<%= btnCargarEmpleado.UniqueID %>', idUsuario);
        }
    </script>

</asp:Content>
