<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="login.aspx.cs" Inherits="GrupoAnkhalAsistencia.login" %>

<!DOCTYPE html>
<html lang="es">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <title>Grupo ANKHAL</title>
  <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet" />
  <style>
    body, html {
      height: 100%;
      margin: 0;
      font-family: 'Segoe UI', sans-serif;
      background-color: #1f2c3e ;
      overflow: hidden;
    }

    canvas {
      position: fixed;
      top: 0;
      left: 0;
      z-index: 0;
    }

    .login-box {
      position: relative;
      z-index: 1;
      max-width: 400px;
      margin: auto;
      top: 15%;
      background: rgb(43 42 40);
      backdrop-filter: blur(10px);
      border-radius: 15px;
      padding: 30px;
      box-shadow: 0 0 20px rgba(255, 255, 255, 0.2);
      color: #fff;
    }

    .form-control {
      background-color: rgba(0, 0, 0, 0.5);
      color: #fff;
      border: 1px solid #ccc;
    }

    .form-control::placeholder {
      color: #ccc;
    }

    .btn-login {
      background-color: #ff6600;
      color: #fff;
      border: none;
    }

    .btn-login:hover {
      background-color: #e2850cee;
      color: #fff;
    }

    .form-check-label, .forgot-link {
      color: #fff;
    }

    .forgot-link:hover {
      text-decoration: underline;
    }
  </style>
    <script src="scriptspropios/sweetalert2@11.js"></script>
</head>
<body>
  <canvas id="bgCanvas"></canvas>

  <div class="login-box text-center">
    <h3 class="mb-4 text-center">
      <img src="img/ankhal.png" width="250px" class="img-fluid border-2 rounded shadow-sm" />
      <h2 style="font-style: normal;">GRUPO ANKHAL</h2>
    </h3>

    <!-- 🚀 AQUI EL CAMBIO IMPORTANTE -->
    <form id="form1" runat="server">
      <div class="mb-3 text-start">
        <label for="txtusuario" class="form-label">Usuario:</label>
        <asp:TextBox ID="txtusuario" runat="server" CssClass="form-control" Placeholder="usuario" ClientIDMode="Static"></asp:TextBox>
      </div>

      <div class="mb-3 text-start">
        <label for="txtcontrasena" class="form-label">Clave:</label>
        <asp:TextBox ID="txtcontrasena" runat="server" CssClass="form-control" Placeholder="contraseña" TextMode="Password" ClientIDMode="Static"></asp:TextBox>
      </div>

      <asp:Button ID="btnIngresar" runat="server" Text="Ingresar" CssClass="btn btn-login w-100" OnClick="btnIngresar_Click" />
    </form>
  </div>

  <script>
      const canvas = document.getElementById('bgCanvas');
      const ctx = canvas.getContext('2d');
      canvas.width = window.innerWidth;
      canvas.height = window.innerHeight;

      function drawHexagon(x, y, size, color) {
          ctx.beginPath();
          for (let i = 0; i < 6; i++) {
              const angle = Math.PI / 3 * i;
              const px = x + size * Math.cos(angle);
              const py = y + size * Math.sin(angle);
              ctx.lineTo(px, py);
          }
          ctx.closePath();
          ctx.fillStyle = color;
          ctx.fill();
      }

      let hexagons = [];
      for (let i = 0; i < 60; i++) {
          hexagons.push({
              x: Math.random() * canvas.width,
              y: Math.random() * canvas.height,
              size: 20 + Math.random() * 30,
              vx: (Math.random() - 0.5) * 0.3,
              vy: (Math.random() - 0.5) * 0.3,
              color: Math.random() < 0.5 ? '#ff6600' : '#e2850cee'
          });
      }

      function animate() {
          ctx.clearRect(0, 0, canvas.width, canvas.height);
          hexagons.forEach(h => {
              drawHexagon(h.x, h.y, h.size, h.color);
              h.x += h.vx;
              h.y += h.vy;

              if (h.x < -h.size || h.x > canvas.width + h.size) h.vx *= -1;
              if (h.y < -h.size || h.y > canvas.height + h.size) h.vy *= -1;
          });
          requestAnimationFrame(animate);
      }

      animate();
      window.addEventListener('resize', () => {
          canvas.width = window.innerWidth;
          canvas.height = window.innerHeight;
      });
  </script>
</body>
</html>
