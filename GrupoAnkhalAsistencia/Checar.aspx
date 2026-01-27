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
                    <div class="card-header">Aproxime el c&oacute;digo QR de su credencial</div>
                    <div class="card-body">
                        <p class="checar-instruction">Enfoque el c&oacute;digo QR de su gafete frente a la c&aacute;mara. La asistencia se registrar&aacute; autom&aacute;ticamente.</p>
                        <div id="qr-reader"></div>
                        <asp:HiddenField ID="hdQr" runat="server" />
                        <asp:Button ID="btnChecarQr" runat="server" Text="Checar" OnClick="btnChecarQr_Click" Style="display: none;" />
                    </div>
                </div>
            </div>
        </div>
    </form>

    <script type="text/javascript">
        (function() {
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
            window.addEventListener('resize', function() {
                c.width = window.innerWidth;
                c.height = window.innerHeight;
            });
        })();
    </script>
    <script type="text/javascript">
            function speakChecar(text) {
                if (!text || !window.speechSynthesis) return;
                window.speechSynthesis.cancel();
                var u = new SpeechSynthesisUtterance(text);
                u.lang = 'es-MX';
                u.rate = 0.95;
                var vs = speechSynthesis.getVoices();
                var v = vs.find(function(x) { return x.lang.startsWith('es'); }) || vs[0];
                if (v) u.voice = v;
                speechSynthesis.speak(u);
            }
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
                    // Prevenir múltiples escaneos del mismo código en menos de 3 segundos
                    var now = Date.now();
                    if (isProcessing || (decodedText === lastScannedCode && (now - lastScanTime) < 3000)) {
                        return;
                    }

                    isProcessing = true;
                    lastScannedCode = decodedText;
                    lastScanTime = now;

                    // Detener el scanner inmediatamente
                    if (scanner) {
                        scanner.clear().then(function () {
                            scanner = null;
                            document.getElementById(hdQrId).value = decodedText;
                            document.getElementById(btnId).click();
                        }).catch(function (err) {
                            console.warn(err);
                            scanner = null;
                            isProcessing = false;
                        });
                    } else {
                        document.getElementById(hdQrId).value = decodedText;
                        document.getElementById(btnId).click();
                    }
                }

                function onScanError() { }

                // Reiniciar el flag después del postback
                window.addEventListener('load', function() {
                    isProcessing = false;
                    lastScannedCode = '';
                    lastScanTime = 0;
                    setTimeout(initScanner, 500);
                });

                if (document.readyState === 'loading') {
                    document.addEventListener('DOMContentLoaded', function() {
                        setTimeout(initScanner, 500);
                    });
                } else {
                    setTimeout(initScanner, 500);
                }
            })();
        </script>
</body>
</html>
