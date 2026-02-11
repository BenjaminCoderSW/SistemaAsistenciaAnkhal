<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Checar.aspx.cs" Inherits="GrupoAnkhalAsistencia.Checar" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Checador por QR - Grupo Ankhal</title>
    <link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css" />
    <script src="scriptspropios/sweetalert2@11.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/html5-qrcode/2.3.8/html5-qrcode.min.js"></script>
    <style>
        body, html {
            height: 100%;
            margin: 0;
            font-family: 'Segoe UI', Arial, sans-serif;
            background-color: #1f2c3e;
            overflow-x: hidden;
        }
        #bgCanvas {
            position: fixed;
            top: 0;
            left: 0;
            z-index: 0;
        }
        .checar-wrap {
            position: relative;
            z-index: 1;
            min-height: 100vh;
        }
        .checar-header {
            background: rgba(43, 42, 40, 0.95);
            color: #fff;
            padding: 14px 24px;
            display: flex;
            align-items: center;
            border-bottom: 2px solid #ff6600;
            backdrop-filter: blur(10px);
        }
        .checar-header-bar {
            width: 6px;
            height: 28px;
            border-radius: 3px;
            background: #ff6600;
            margin-right: 12px;
        }
        .checar-header .empresa { font-weight: 900; font-size: 57px; color: #ff6600; }
        .checar-header .subtitulo { font-size: 18px; opacity: 0.9; }
        .checar-card {
            max-width: 800px;
            margin: 24px auto;
            border-radius: 15px;
            overflow: hidden;
            background: rgb(43, 42, 40);
            backdrop-filter: blur(10px);
            box-shadow: 0 0 20px rgba(255, 255, 255, 0.2);
            color: #fff;
        }
        .checar-card .card-header {
            background: rgba(0, 0, 0, 0.3);
            color: #fff;
            font-weight: 600;
            padding: 16px 20px;
            font-size: 17px;
            border-bottom: 1px solid rgba(255, 102, 0, 0.3);
        }
        .checar-card .card-body { padding: 30px; }
        #qr-reader {
            border-radius: 8px;
            overflow: hidden;
            border: 2px solid rgba(255, 102, 0, 0.4);
            min-height: 500px;
        }
        #qr-reader video {
            width: 100% !important;
            max-width: 100% !important;
        }
        #qr-reader__scan_region {
            background: #1a1a1a !important;
        }
        #qr-reader__dashboard_section_csr button {
            background: #ff6600 !important;
            border-color: #ff6600 !important;
            color: #fff !important;
        }
        .checar-instruction {
            background: rgba(0, 0, 0, 0.35);
            border-left: 4px solid #ff6600;
            padding: 10px 14px;
            margin-bottom: 16px;
            border-radius: 0 8px 8px 0;
            font-size: 14px;
            color: #e0e0e0;
        }
    </style>
</head>
<body>
    <canvas id="bgCanvas"></canvas>
    <form id="form1" runat="server">
        <div class="checar-wrap">
            <div class="checar-header">
                <div class="checar-header-bar"></div>
                <div>
                    <div class="empresa"><img src="img/ankhal.png" width="109px"/> GRUPO ANKHAL</div>
                    <div class="subtitulo">Checador por credencial QR</div>
                </div>
            </div>

            <div class="container">
                <div class="checar-card card">
                    <div class="card-header">Aproxime el código QR de su credencial</div>
                    <div class="card-body">
                        <p class="checar-instruction">Enfoque el código QR de su gafete frente a la cámara. La asistencia se registrará automáticamente.</p>
                        <div id="qr-reader"></div>
                        <asp:HiddenField ID="hdQr" runat="server" />
                        <asp:HiddenField ID="hdLat" runat="server" />
                        <asp:HiddenField ID="hdLon" runat="server" />
                        <asp:Button ID="btnChecarQr" runat="server" Text="Checar" OnClick="btnChecarQr_Click" Style="display: none;" />

                        <!-- BOTON PARA VOLVER AL LOGIN -->
                        <div class="mt-3 text-center">
                            <asp:Button ID="btnVolverLogin" runat="server" Text="Volver al Login" CssClass="btn btn-secondary" OnClick="btnVolverLogin_Click" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </form>

    <!-- SCRIPT DE ANIMACIÓN DE FONDO -->
    <script type="text/javascript">
        (function () {
            var c = document.getElementById('bgCanvas');
            if (!c) return;
            var ctx = c.getContext('2d');
            c.width = window.innerWidth;
            c.height = window.innerHeight;
            function drawHexagon(x, y, size, color) {
                ctx.beginPath();
                for (var i = 0; i < 6; i++) {
                    var angle = Math.PI / 3 * i;
                    var px = x + size * Math.cos(angle);
                    var py = y + size * Math.sin(angle);
                    ctx.lineTo(px, py);
                }
                ctx.closePath();
                ctx.fillStyle = color;
                ctx.fill();
            }
            var hexagons = [];
            for (var i = 0; i < 60; i++) {
                hexagons.push({
                    x: Math.random() * c.width,
                    y: Math.random() * c.height,
                    size: 20 + Math.random() * 30,
                    vx: (Math.random() - 0.5) * 0.3,
                    vy: (Math.random() - 0.5) * 0.3,
                    color: Math.random() < 0.5 ? '#ff6600' : '#e2850cee'
                });
            }
            function animate() {
                ctx.clearRect(0, 0, c.width, c.height);
                for (var j = 0; j < hexagons.length; j++) {
                    var h = hexagons[j];
                    drawHexagon(h.x, h.y, h.size, h.color);
                    h.x += h.vx;
                    h.y += h.vy;
                    if (h.x < -h.size || h.x > c.width + h.size) h.vx *= -1;
                    if (h.y < -h.size || h.y > c.height + h.size) h.vy *= -1;
                }
                requestAnimationFrame(animate);
            }
            animate();
            window.addEventListener('resize', function () {
                c.width = window.innerWidth;
                c.height = window.innerHeight;
            });
        })();
    </script>

    <!-- SCRIPT DE SÍNTESIS DE VOZ -->
    <script type="text/javascript">
        // FUNCIÓN GLOBAL DE SÍNTESIS DE VOZ
        function speakChecar(text) {
            if (!text || !window.speechSynthesis) {
                console.log('No se puede reproducir el audio');
                return;
            }

            // Cancelar cualquier síntesis en curso
            window.speechSynthesis.cancel();

            // Esperar a que las voces estén disponibles
            var interval = setInterval(function () {
                var voices = window.speechSynthesis.getVoices();

                if (voices.length !== 0) {
                    clearInterval(interval);

                    // Lista de voces preferidas en español
                    var preferidas = [
                        'Google español (Latinoamérica)',
                        'Google español de Latinoamérica',
                        'Google español',
                        'es-MX-Standard-A',
                        'es-US-Standard-A',
                        'es-ES-Standard-A',
                        'Microsoft Laura',
                        'Microsoft Sabina',
                        'Google US Spanish',
                        'Paulina'
                    ];

                    var selectedVoice = null;

                    // Buscar voz preferida
                    for (var i = 0; i < preferidas.length; i++) {
                        selectedVoice = voices.find(function (v) {
                            return v.name.includes(preferidas[i]);
                        });
                        if (selectedVoice) {
                            console.log('Voz seleccionada:', selectedVoice.name);
                            break;
                        }
                    }

                    // Si no encuentra voz preferida, usar cualquier voz en español
                    if (!selectedVoice) {
                        selectedVoice = voices.find(function (v) {
                            return v.lang.startsWith('es');
                        });
                        console.log('Voz alternativa:', selectedVoice ? selectedVoice.name : 'ninguna');
                    }

                    // Crear y configurar el utterance
                    var utter = new SpeechSynthesisUtterance(text);

                    if (selectedVoice) {
                        utter.voice = selectedVoice;
                    }

                    utter.lang = 'es-MX';
                    utter.rate = 0.95;
                    utter.pitch = 1.1;
                    utter.volume = 1.0;

                    // Eventos para debug
                    utter.onstart = function () {
                        console.log('Iniciando síntesis de voz:', text);
                    };

                    utter.onerror = function (event) {
                        console.error('Error en síntesis de voz:', event);
                    };

                    utter.onend = function () {
                        console.log('Síntesis de voz completada');
                    };

                    // Reproducir
                    window.speechSynthesis.speak(utter);
                }
            }, 100);

            // Timeout de seguridad
            setTimeout(function () {
                clearInterval(interval);
            }, 5000);
        }

        // Cargar voces al iniciar
        window.speechSynthesis.getVoices();

        // En algunos navegadores las voces se cargan de forma asíncrona
        if (window.speechSynthesis.onvoiceschanged !== undefined) {
            window.speechSynthesis.onvoiceschanged = function () {
                console.log('Voces cargadas:', window.speechSynthesis.getVoices().length);
            };
        }
    </script>

    <!-- SCRIPT DE LECTURA QR -->
    <script type="text/javascript">
        (function () {
            var scanner = null;
            var isProcessing = false;
            var lastScannedCode = '';
            var lastScanTime = 0;
            var hdQrId = '<%= hdQr.ClientID %>';
            var btnId = '<%= btnChecarQr.ClientID %>';

            function initScanner() {
                if (scanner || isProcessing) return;
                var qrReaderElement = document.getElementById("qr-reader");
                if (!qrReaderElement) return;

                scanner = new Html5QrcodeScanner("qr-reader", {
                    fps: 10,
                    qrbox: { width: 400, height: 400 },
                    rememberLastUsedCamera: true,
                    aspectRatio: 1.0
                });
                scanner.render(onScanSuccess, onScanError);
            }

            function onScanSuccess(decodedText) {
                // Prevenir múltiples escaneos
                var now = Date.now();
                if (isProcessing || (decodedText === lastScannedCode && (now - lastScanTime) < 3000)) {
                    return;
                }

                isProcessing = true;
                lastScannedCode = decodedText;
                lastScanTime = now;

                console.log('Código QR escaneado:', decodedText);

                // ✅ CAPTURAR GPS ANTES DE ENVIAR
                if (navigator.geolocation) {
                    navigator.geolocation.getCurrentPosition(
                        function (pos) {
                            var lat = pos.coords.latitude.toFixed(10);
                            var lon = pos.coords.longitude.toFixed(10);

                            document.getElementById('<%= hdLat.ClientID %>').value = lat;
                document.getElementById('<%= hdLon.ClientID %>').value = lon;
                document.getElementById('<%= hdQr.ClientID %>').value = decodedText;

                // Detener scanner y hacer postback
                if (scanner) {
                    scanner.clear().then(function () {
                        scanner = null;
                        document.getElementById('<%= btnChecarQr.ClientID %>').click();
                    }).catch(function (err) {
                        console.warn('Error al detener scanner:', err);
                        scanner = null;
                        isProcessing = false;
                    });
                } else {
                    document.getElementById('<%= btnChecarQr.ClientID %>').click();
                }
            },
            function(err) {
                console.error('Error GPS:', err);
                // Continuar sin GPS
                document.getElementById('<%= hdQr.ClientID %>').value = decodedText;
                if (scanner) {
                    scanner.clear().then(function () {
                        scanner = null;
                        document.getElementById('<%= btnChecarQr.ClientID %>').click();
                    });
                } else {
                    document.getElementById('<%= btnChecarQr.ClientID %>').click();
                }
            },
            {
                enableHighAccuracy: true,
                timeout: 5000,
                maximumAge: 0
            }
                );
            } else {
                // Sin soporte GPS
                document.getElementById('<%= hdQr.ClientID %>').value = decodedText;
                if (scanner) {
                    scanner.clear().then(function () {
                        scanner = null;
                        document.getElementById('<%= btnChecarQr.ClientID %>').click();
                    });
                    }
                }
            }

            function onScanError(error) {
                // Silenciar errores normales de escaneo
            }

            // Reiniciar el flag después del postback
            window.addEventListener('load', function () {
                isProcessing = false;
                lastScannedCode = '';
                lastScanTime = 0;
                setTimeout(initScanner, 500);
            });

            if (document.readyState === 'loading') {
                document.addEventListener('DOMContentLoaded', function () {
                    setTimeout(initScanner, 500);
                });
            } else {
                setTimeout(initScanner, 500);
            }
        })();
    </script>
</body>
</html>