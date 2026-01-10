using GrupoAnkhalAsistencia.Modelo;
using System;
using System.Configuration;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GrupoAnkhalAsistencia
{
    public partial class RegistroPlanta1 : System.Web.UI.Page
    {
        public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
       ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnChecar_Click(object sender, EventArgs e)
        {
            try
            {
                string fotoBase64 = hdFotoChecada.Value;

                if (string.IsNullOrEmpty(fotoBase64))
                {
                    MostrarAlerta("error", "Error", "No se pudo tomar la foto.");
                    return;
                }

                // Quitar encabezado base64
                fotoBase64 = fotoBase64.Replace("data:image/jpeg;base64,", "")
                                       .Replace("data:image/png;base64,", "");

                byte[] fotoBytes = Convert.FromBase64String(fotoBase64);

                // Buscar usuario comparando la foto
                var usuario = BuscarUsuarioPorRostro(fotoBytes);

                if (usuario == null)
                {
                    MostrarAlerta("error", "Error", "No se reconoció el rostro.");
                    return;
                }

                RegistrarAsistencia(usuario.IdUsuario);

                MostrarAlerta("success", "Correcto", "Asistencia registrada para " + usuario.Nombre);
            }
            catch (Exception ex)
            {
                MostrarAlerta("error", "Error", ex.Message);
            }
        }


        // ----------------------------------------
        // RECONOCIMIENTO POR ROSTRO (BÁSICO)
        // ----------------------------------------
        private tUsuario BuscarUsuarioPorRostro(byte[] fotoNueva)
        {
            // Trae solo usuarios con foto
            var usuarios = db.tUsuario.Where(x => x.Foto != null && x.Estatus == 1).ToList();

            foreach (var u in usuarios)
            {
                // Calcular similitud
                if (u.Foto == null) continue;

                double similitud = CompararBytes(fotoNueva, u.Foto.ToArray());

                // Ajusta el % según tus pruebas (0.90 = 90%)
                if (similitud >= 0.90)
                    return u;
            }

            return null;
        }

        // Comparación muy básica (NO IA real)
        private double CompararBytes(byte[] a, byte[] b)
        {
            int len = Math.Min(a.Length, b.Length);
            int iguales = 0;

            for (int i = 0; i < len; i++)
            {
                if (a[i] == b[i])
                    iguales++;
            }

            return (double)iguales / len;
        }


        // ----------------------------------------
        // REGISTRAR ASISTENCIA
        // ----------------------------------------
        private void RegistrarAsistencia(int idUsuario)
        {
            tAsistencia nueva = new tAsistencia
            {
                IdUsuario = idUsuario,
                Fecha = DateTime.Now,
               
            };

            db.tAsistencia.InsertOnSubmit(nueva);
            db.SubmitChanges();
        }


        // ----------------------------------------
        // SWEET ALERT
        // ----------------------------------------
        private void MostrarAlerta(string icono, string titulo, string mensaje)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
                $"Swal.fire({{ icon: '{icono}', title: '{titulo}', text: '{mensaje}' }});", true);
        }

    }
}
