using GrupoAnkhalAsistencia.Modelo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MedicaMedens.Sesion
{
    public class SesionState
    {
        private const string SESSION_KEY = "UsuarioActual";

        public static tUsuario usuario
        {
            get
            {
                if (HttpContext.Current != null && HttpContext.Current.Session != null)
                {
                    return HttpContext.Current.Session[SESSION_KEY] as tUsuario;
                }
                return null;
            }
            set
            {
                if (HttpContext.Current != null && HttpContext.Current.Session != null)
                {
                    HttpContext.Current.Session[SESSION_KEY] = value;
                }
            }
        }
    }
}