<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="RegistrarAsistencia.aspx.cs" Inherits="GrupoAnkhalAsistencia.RegistrarAsistencia" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .form-control {
    border-radius: 6px;
    height: 40px;
}

.form-control[disabled], 
.form-control[readonly] {
    background-color: #f1f1f1;
    color: #334;
    font-weight: 600;
}

.card label {
    font-weight: 600;
}

#txtHora {
    font-size: 1.6rem;
    font-weight: 700;
}

    </style>
    <script src="scriptspropios/sweetalert2@11.js"></script>
<script src="scriptspropios/propios.js"></script>
    <script src="https://openfpcdn.io/fingerprintjs/v3"></script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <!-- CAMPOS OCULTOS PARA LATITUD / LONGITUD -->
    <asp:HiddenField ID="hdLat" runat="server" />
    <asp:HiddenField ID="hdLon" runat="server" />

   <div class="container mt-4">

        <div class="card shadow" style="border-radius: 10px; max-width: 1000px; margin: 0 auto;">

            <!-- Encabezado azul -->
            <div style="background-color:#0b3360; padding:15px; border-radius:10px 10px 0 0;">
                <h5 class="text-white m-0">
                    <i class="fa fa-clock-o"></i> Asistencia
                </h5>
            </div>

            <div class="card-body">

                <!-- Empleado -->
                <div class="row mb-3">
                    <div class="col-md-12">
                        <label>Empleado (*)</label>
                        <asp:TextBox ID="txtNombreEmpleado" runat="server" CssClass="form-control" Enabled="false"></asp:TextBox>
                    </div>
                </div>

                <!-- Hora y Fecha -->
                <div class="row mb-3">
                    <div class="col-md-6">
                        <label>Hora (*)</label>
                        <asp:TextBox ID="txtHora" runat="server" CssClass="form-control" Enabled="false"></asp:TextBox>
                    </div>

                    <div class="col-md-6">
                        <label>Fecha</label>
                        <asp:TextBox ID="txtFecha" runat="server" CssClass="form-control" Enabled="false"></asp:TextBox>
                    </div>
                </div>
                <asp:HiddenField ID="hdFingerprint" runat="server" />


               <!-- Botón REGISTRAR -->
                <div class="mt-4">
                    <asp:Button ID="btnRegistrar" runat="server" Text="Registrar asistencia"
                        CssClass="btn btn-primary w-100"
                        Style="background-color:#0b3360; border:none; padding:12px; font-size:16px;"
                        OnClick="btnRegistrar_Click"
                        OnClientClick="return registrarAsistencia();" />
                </div>

            </div>
        </div>

    </div>
   <script>
       // Actualizar hora y fecha
       function actualizarFechaHora() {
           var ahora = new Date();

           // ----- HORA -----
           var horas = String(ahora.getHours()).padStart(2, '0');
           var minutos = String(ahora.getMinutes()).padStart(2, '0');
           var segundos = String(ahora.getSeconds()).padStart(2, '0');

           var horaCompleta = horas + ":" + minutos + ":" + segundos;
           document.getElementById("<%= txtHora.ClientID %>").value = horaCompleta;

        // ----- FECHA -----
        var dia = String(ahora.getDate()).padStart(2, '0');
        var mes = String(ahora.getMonth() + 1).padStart(2, '0');  // +1 porque enero es 0
        var anio = ahora.getFullYear();

        var fechaCompleta = dia + "/" + mes + "/" + anio;
        document.getElementById("<%= txtFecha.ClientID %>").value = fechaCompleta;
       }

       // Ejecutar cada segundo
       setInterval(actualizarFechaHora, 1000);

       // Ejecutar inmediatamente al cargar
       actualizarFechaHora();

       // ------------------------------------
       //   FUNCIÓN PARA ENVIAR UBICACIÓN
       // ------------------------------------
       <%--function registrarAsistencia() {

           navigator.geolocation.getCurrentPosition(
               function (pos) {

                   // Validar precisión (accuracy viene en METROS)
                   if (pos.coords.accuracy > 80) {
                       alert("La señal de ubicación es muy débil. Muévete a un área abierta e intenta de nuevo.\nPrecisión: "
                           + pos.coords.accuracy.toFixed(1) + "m");
                       return;
                   }

                   // Guardar lat/lon en los HiddenFields ASP.NET
                   document.getElementById("<%= hdLat.ClientID %>").value = pos.coords.latitude;
            document.getElementById("<%= hdLon.ClientID %>").value = pos.coords.longitude;

            // Hacer el postback al botón ASP.NET
            __doPostBack('<%= btnRegistrar.UniqueID %>', '');
        },

        function (err) {
            alert("No se pudo obtener la ubicación.\nError: " + err.message);
        },

        {
            enableHighAccuracy: true,  // 🔥 pedir GPS con máxima precisión
            timeout: 10000,            // esperar hasta 10s para obtenerla
            maximumAge: 0              // evitar coordenadas guardadas en cache
        }
    );

           return false; // Evita que el botón haga postback inmediato
       }--%>


       let intentosGPS = 0;
       const maxIntentos = 5;

       function registrarAsistencia() {
           obtenerUbicacion();
           return false;
       }

       function obtenerUbicacion() {
           navigator.geolocation.getCurrentPosition(
               function (pos) {

                   intentosGPS++;

                   // Validar precisión
                   if (pos.coords.accuracy > 200) {

                       if (intentosGPS < maxIntentos) {
                           // Reintentar automáticamente
                           console.log("Intento " + intentosGPS +
                               " - Precisión mala (" + pos.coords.accuracy + "m). Reintentando...");
                           setTimeout(obtenerUbicacion, 1500);
                           return;
                       }

                       // Después de reintentos falló
                       alert("La señal de ubicación es muy débil.\n" +
                           "Precisión: " + pos.coords.accuracy.toFixed(1) + "m\n" +
                           "Intenta acercarte a una ventana o encender el GPS.");
                       return;
                   }

                   // Si la precisión es buena:
                   document.getElementById("<%= hdLat.ClientID %>").value = pos.coords.latitude;
            document.getElementById("<%= hdLon.ClientID %>").value = pos.coords.longitude;

            __doPostBack('<%= btnRegistrar.UniqueID %>', '');
        },

        function (err) {
            alert("No se pudo obtener la ubicación.\nError: " + err.message);
        },

        {
            enableHighAccuracy: true,
            timeout: 10000,
            maximumAge: 0
        }
    );
       }




      // Clave única del dispositivo evita que chequen por el 
       async function obtenerFingerprint() {
           // Cargar FingerprintJS
           const fp = await FingerprintJS.load();
           const result = await fp.get();

           // Este es el ID único del dispositivo
           const fingerprint = result.visitorId;

           // Lo mandas al HiddenField
           document.getElementById("<%= hdFingerprint.ClientID %>").value = fingerprint;

           console.log("Fingerprint del dispositivo:", fingerprint);
       }

       // Ejecutarlo
       obtenerFingerprint();


   </script>

 


</asp:Content>
