<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RegistroPlanta1.aspx.cs" Inherits="GrupoAnkhalAsistencia.RegistroPlanta1" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <title>Reconocimiento Facial - Checador</title>

    <!-- Face API -->
    <script src="https://cdn.jsdelivr.net/npm/face-api.js"></script>

    <style>
        #videoCamara {
            width: 320px;
            height: 240px;
            border: 2px solid #333;
            border-radius: 10px;
        }
        #canvasFoto {
            display: none;
        }
    </style>
        <script src="scriptspropios/sweetalert2@11.js"></script>
<script src="scriptspropios/propios.js"></script>
</head>
<body>
    <form id="form1" runat="server">

        <div style="text-align:center; margin-top:20px;">
            <h2>Checador de Asistencia por Rostro</h2>

            <video id="videoCamara" autoplay muted></video><br>

            <button type="button" onclick="tomarFoto()" class="btn btn-primary">Checar Entrada</button>
            <br /><br />

            <canvas id="canvasFoto" width="320" height="240"></canvas>

            <asp:HiddenField ID="hdFotoChecada" runat="server" />

            <!-- Botón oculto para hacer postback -->
            <asp:Button ID="btnChecar" runat="server" Text="Checar" OnClick="btnChecar_Click" style="display:none;" />

        </div>

        <script>

            // ===========================
            // 1. ACTIVAR CÁMARA
            // ===========================
            async function iniciarCamara() {
                try {
                    const stream = await navigator.mediaDevices.getUserMedia({ video: true });
                    document.getElementById("videoCamara").srcObject = stream;
                } catch (e) {
                    alert("No se pudo acceder a la cámara: " + e);
                }
            }

            // ===========================
            // 2. TOMAR FOTO PARA CHECADA
            // ===========================
            function tomarFoto() {
                const video = document.getElementById("videoCamara");
                const canvas = document.getElementById("canvasFoto");
                const ctx = canvas.getContext("2d");

                ctx.drawImage(video, 0, 0, canvas.width, canvas.height);

                let fotoBase64 = canvas.toDataURL("image/jpeg");

                document.getElementById("<%= hdFotoChecada.ClientID %>").value = fotoBase64;

                // Disparar postback para validar rostro
                document.getElementById("<%= btnChecar.ClientID %>").click();
            }

            // ENCENDER CÁMARA AL CARGAR PANTALLA
            window.onload = iniciarCamara;

        </script>

    </form>
</body>
</html>
