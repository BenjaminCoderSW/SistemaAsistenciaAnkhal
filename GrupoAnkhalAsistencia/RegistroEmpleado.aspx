<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" 
    CodeBehind="RegistroEmpleado.aspx.cs" Inherits="GrupoAnkhalAsistencia.RegistroEmpleado" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .form-control {
            border-radius: 6px;
            height: 40px;
        }
        .form-control[disabled] {
            background-color: #f1f1f1;
            font-weight: 600;
        }
        .card {
            border-radius: 10px;
        }
    </style>
    <script src="scriptspropios/sweetalert2@11.js"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <!-- Hidden para guardar la IP -->
    <asp:HiddenField ID="hdIP" runat="server" />

    <div class="container mt-4">
        <div class="card shadow" style="max-width: 600px; margin: 0 auto;">

            <!-- Encabezado -->
            <div style="background-color:#0b3360; padding:15px; border-radius:10px 10px 0 0;">
                <h5 class="text-white m-0">
                    Registro de Empleado
                </h5>
            </div>

            <div class="card-body">

                <!-- Nombre empleado -->
                <div class="mb-3">
                    <label>Empleado</label>
                    <asp:TextBox ID="txtEmpleado" runat="server" CssClass="form-control" 
                        Enabled="false"></asp:TextBox>
                </div>

                <!-- Fecha -->
                <div class="mb-3">
                    <label>Fecha</label>
                    <asp:TextBox ID="txtFecha" runat="server" CssClass="form-control" Enabled="false"></asp:TextBox>
                </div>

                <!-- Hora -->
                <div class="mb-3">
                    <label>Hora</label>
                    <asp:TextBox ID="txtHora" runat="server" CssClass="form-control" Enabled="false"></asp:TextBox>
                </div>

                <asp:HiddenField ID="hdLat" runat="server" />
                <asp:HiddenField ID="hdLon" runat="server" />
                <asp:HiddenField ID="hdFingerprint" runat="server" />

               

                <asp:Button ID="btnRegistrar" runat="server" Text="Registrar asistencia"
    CssClass="btn btn-primary w-100"
    Style="background-color:#0b3360; border:none; padding:12px; font-size:16px;"
    OnClientClick="return registrarAsistenciaGPS();"
    OnClick="btnRegistrar_Click" />


     

            </div>
        </div>
    </div>

    <!-- SCRIPT -->
    <script>

        // ===============================
        // 1) Obtener IP pública
        // ===============================
        async function obtenerIP() {
            try {
                let resp = await fetch("https://api.ipify.org?format=json");
                let data = await resp.json();

                document.getElementById("<%= hdIP.ClientID %>").value = data.ip;

                console.log("IP detectada:", data.ip);

            } catch (e) {
                console.log("Error al obtener IP:", e);
            }
        }

        obtenerIP();

        // ===============================
        // 2) Registrar Asistencia (sin GPS)
        // ===============================
        function registrarAsistencia() {

            let ip = document.getElementById("<%= hdIP.ClientID %>").value;

            if (!ip || ip.length < 7) {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'No se pudo obtener la IP. No puedes registrar asistencia.'
                });
                return false;
            }

            // Enviar al servidor
            __doPostBack('<%= btnRegistrar.UniqueID %>', '');

            return false;
        }


        // ===============================
        // 3) Actualizar hora/fecha
        // ===============================
        function actualizarFechaHora() {
            var ahora = new Date();

            var h = ahora.getHours().toString().padStart(2, '0');
            var m = ahora.getMinutes().toString().padStart(2, '0');
            var s = ahora.getSeconds().toString().padStart(2, '0');
            document.getElementById("<%= txtHora.ClientID %>").value = `${h}:${m}:${s}`;

            var dia = ahora.getDate().toString().padStart(2, '0');
            var mes = (ahora.getMonth() + 1).toString().padStart(2, '0');
            var anio = ahora.getFullYear();
            document.getElementById("<%= txtFecha.ClientID %>").value = `${dia}/${mes}/${anio}`;
        }

        actualizarFechaHora();
        setInterval(actualizarFechaHora, 1000);

        //validar ubicaion y clave unica

        function registrarAsistenciaGPS() {
            navigator.geolocation.getCurrentPosition(
                function (pos) {
                    document.getElementById("<%= hdLat.ClientID %>").value = pos.coords.latitude.toString().replace(',', '.');
                    document.getElementById("<%= hdLon.ClientID %>").value = pos.coords.longitude.toString().replace(',', '.');

            document.getElementById("<%= hdFingerprint.ClientID %>").value = generarFingerprint();

            // Postback ahora que ya tenemos los valores
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

            return false; // cancelar postback inmediato
        }



        function obtenerUbicacion() {
            // Generar fingerprint simple
            const fingerprint = [
                navigator.userAgent,
                screen.width + "x" + screen.height,
                navigator.language,
                Intl.DateTimeFormat().resolvedOptions().timeZone,
                navigator.hardwareConcurrency || 0,
                navigator.deviceMemory || 0
            ].join('|');

            let hash = 0;
            for (let i = 0; i < fingerprint.length; i++) {
                const chr = fingerprint.charCodeAt(i);
                hash = ((hash << 5) - hash) + chr;
                hash |= 0; // 32-bit
            }

            // Guardar fingerprint en el hidden field
            document.getElementById("<%= hdFingerprint.ClientID %>").value = hash.toString();

            // Obtener ubicación
            navigator.geolocation.getCurrentPosition(
                function (pos) {
                    document.getElementById("<%= hdLat.ClientID %>").value = pos.coords.latitude.toString().replace(',', '.');
                    document.getElementById("<%= hdLon.ClientID %>").value = pos.coords.longitude.toString().replace(',', '.');


            // Hacer postback
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


        function generarFingerprint() {
            const fingerprint = [
                navigator.userAgent,
                screen.width + "x" + screen.height,
                navigator.language,
                Intl.DateTimeFormat().resolvedOptions().timeZone,
                navigator.hardwareConcurrency || 0,
                navigator.deviceMemory || 0
            ].join('|');

            let hash = 0;
            for (let i = 0; i < fingerprint.length; i++) {
                const chr = fingerprint.charCodeAt(i);
                hash = ((hash << 5) - hash) + chr;
                hash |= 0; // convertir a 32-bit
            }
            return hash.toString();
        }


    </script>

</asp:Content>
