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
                <asp:HiddenField ID="hdFotoAsistencia" runat="server" />

                <!-- Panel de selfie (solo visible para pasos Entrada y Salida) -->
                <div id="divCamara" style="display:none; text-align:center; margin-bottom:16px;">
                    <p class="text-muted mb-2" style="font-size:13px;">
                        <i class="fas fa-camera"></i> <strong>Toma una selfie</strong> para confirmar tu identidad
                    </p>
                    <video id="videoCamaraAsistencia" width="280" height="210"
                           autoplay playsinline
                           style="border-radius:8px; border:2px solid #0b3360; max-width:100%;"></video>
                    <canvas id="canvasFotoAsistencia" width="280" height="210"
                            style="display:none; border-radius:8px; border:2px solid #28a745; max-width:100%;"></canvas>
                    <br />
                    <button type="button" id="btnTomarFoto" class="btn btn-secondary btn-sm mt-2"
                            onclick="tomarFotoAsistencia()">
                        <i class="fas fa-camera"></i> Tomar foto
                    </button>
                    <button type="button" id="btnRetomar" class="btn btn-outline-warning btn-sm mt-2"
                            style="display:none;" onclick="retomarFoto()">
                        <i class="fas fa-redo"></i> Retomar
                    </button>
                    <div id="divFotoTomada" style="display:none; color:#28a745; margin-top:6px; font-size:13px;">
                        <i class="fas fa-check-circle"></i> Foto lista
                    </div>
                    <div id="divCamaraError" style="display:none; color:#dc3545; margin-top:6px; font-size:12px;">
                        <i class="fas fa-exclamation-circle"></i> Cámara no disponible. Puedes continuar, pero quedará registrada la omisión.
                    </div>
                </div>

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
        // 0) Selfie — cámara
        // ===============================
        var streamCamaraAsistencia = null;
        var fotoTomada = false;

        // Inicializar cámara — llamada desde ScriptManager después de definir pasoActual
        function iniciarCamara() {
            if (typeof pasoActual === 'undefined') return;
            if (pasoActual !== 'entrada' && pasoActual !== 'salida') return;

            var divCamara = document.getElementById('divCamara');
            if (!divCamara) return;
            divCamara.style.display = 'block';

            if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
                document.getElementById('divCamaraError').style.display = 'block';
                return;
            }

            navigator.mediaDevices.getUserMedia({ video: { facingMode: 'user' }, audio: false })
                .then(function (stream) {
                    streamCamaraAsistencia = stream;
                    document.getElementById('videoCamaraAsistencia').srcObject = stream;
                })
                .catch(function () {
                    document.getElementById('divCamaraError').style.display = 'block';
                });
        }

        function tomarFotoAsistencia() {
            var video  = document.getElementById('videoCamaraAsistencia');
            var canvas = document.getElementById('canvasFotoAsistencia');
            canvas.getContext('2d').drawImage(video, 0, 0, canvas.width, canvas.height);
            var dataUrl = canvas.toDataURL('image/jpeg', 0.7);

            document.getElementById('<%= hdFotoAsistencia.ClientID %>').value = dataUrl;

            canvas.style.display    = 'block';
            video.style.display     = 'none';
            document.getElementById('btnTomarFoto').style.display  = 'none';
            document.getElementById('btnRetomar').style.display    = 'inline-block';
            document.getElementById('divFotoTomada').style.display = 'block';
            fotoTomada = true;

            if (streamCamaraAsistencia) {
                streamCamaraAsistencia.getTracks().forEach(function (t) { t.stop(); });
                streamCamaraAsistencia = null;
            }
        }

        function retomarFoto() {
            fotoTomada = false;
            document.getElementById('<%= hdFotoAsistencia.ClientID %>').value = '';

            var canvas = document.getElementById('canvasFotoAsistencia');
            var video  = document.getElementById('videoCamaraAsistencia');
            canvas.style.display    = 'none';
            video.style.display     = 'block';
            document.getElementById('btnTomarFoto').style.display  = 'inline-block';
            document.getElementById('btnRetomar').style.display    = 'none';
            document.getElementById('divFotoTomada').style.display = 'none';

            navigator.mediaDevices.getUserMedia({ video: { facingMode: 'user' }, audio: false })
                .then(function (stream) {
                    streamCamaraAsistencia = stream;
                    video.srcObject = stream;
                })
                .catch(function () {
                    document.getElementById('divCamaraError').style.display = 'block';
                });
        }

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
            // Validar que el navegador soporte geolocalización
            if (!navigator.geolocation) {
                Swal.fire('Error', 'Tu navegador no soporta geolocalización', 'error');
                return false;
            }

            navigator.geolocation.getCurrentPosition(
                function (pos) {
                    // ✅ CORRECCIÓN: Guardar con precisión de 10 decimales usando punto
                    var lat = pos.coords.latitude.toFixed(10);
                    var lon = pos.coords.longitude.toFixed(10);

                    console.log('Coordenadas capturadas:', lat, lon);

                    document.getElementById("<%= hdLat.ClientID %>").value = lat;
            document.getElementById("<%= hdLon.ClientID %>").value = lon;
            document.getElementById("<%= hdFingerprint.ClientID %>").value = generarFingerprint();

            // Validar selfie (solo requerida para pasos Entrada y Salida)
            var requiereFoto = (typeof pasoActual !== 'undefined' &&
                               (pasoActual === 'entrada' || pasoActual === 'salida'));
            var camaraFallo  = document.getElementById('divCamaraError') &&
                               document.getElementById('divCamaraError').style.display !== 'none';

            if (requiereFoto && !camaraFallo && !fotoTomada) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Selfie requerida',
                    text: 'Debes tomar una foto antes de registrar tu asistencia.'
                });
                return;
            }

            // Hacer postback
            __doPostBack('<%= btnRegistrar.UniqueID %>', '');
        },
        function (err) {
            var mensaje = 'No se pudo obtener la ubicación.';

            if (err.code === 1) {
                mensaje = 'Permiso de ubicación denegado. Por favor, activa el GPS y permite el acceso.';
            } else if (err.code === 2) {
                mensaje = 'No se pudo determinar tu ubicación. Verifica tu conexión GPS.';
            } else if (err.code === 3) {
                mensaje = 'Tiempo de espera agotado al obtener ubicación.';
            }

            Swal.fire({
                icon: 'error',
                title: 'Error de ubicación',
                text: mensaje
            });
        },
        {
            enableHighAccuracy: true,
            timeout: 15000,
            maximumAge: 0
        }
    );

            return false; // Prevenir postback inmediato
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
