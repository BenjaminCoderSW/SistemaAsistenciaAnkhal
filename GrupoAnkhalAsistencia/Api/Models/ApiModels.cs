using System.Collections.Generic;

namespace GrupoAnkhalAsistencia.Api.Models
{
    // ── Respuesta completa de un empleado ────────────────────────────────────────
    public class EmpleadoDto
    {
        public int    IdUsuario        { get; set; }
        public string Nombre           { get; set; }
        public string ApellidoPaterno  { get; set; }
        public string ApellidoMaterno  { get; set; }
        public string Telefono         { get; set; }
        public string TelefonoFamiliar { get; set; }
        public string Email            { get; set; }
        public string NumeroEmpleado   { get; set; }
        public int    Estatus          { get; set; }
    }

    // ── Resumen ligero: solo ID + nombre completo (para dropdowns) ────────────────
    public class EmpleadoResumenDto
    {
        public int    IdUsuario      { get; set; }
        public string NombreCompleto { get; set; }
    }

    // ── Request para consulta bulk ────────────────────────────────────────────────
    public class BulkRequest
    {
        public List<int> Ids { get; set; }
    }

    // ── Respuesta de foto (base64 o null) ─────────────────────────────────────────
    public class FotoDto
    {
        public string Foto { get; set; }
    }
}
