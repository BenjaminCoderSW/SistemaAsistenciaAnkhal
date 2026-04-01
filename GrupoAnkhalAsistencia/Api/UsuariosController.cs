using GrupoAnkhalAsistencia.Api.Models;
using GrupoAnkhalAsistencia.Modelo;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http;

namespace GrupoAnkhalAsistencia.Api
{
    /// <summary>
    /// API de empleados — consumida por sistemas internos (Inventario, etc.)
    /// Autenticación: Bearer token (ApiKey en Web.config)
    ///
    /// Endpoints:
    ///   GET  /api/usuarios/{id}                → datos completos de 1 empleado
    ///   POST /api/usuarios/bulk                → datos de N empleados por IDs
    ///   GET  /api/usuarios/disponibles         → activos sin cuenta en Inventario
    ///   GET  /api/usuarios/foto/{id}           → foto en base64
    /// </summary>
    [RoutePrefix("api/usuarios")]
    public class UsuariosController : ApiController
    {
        private string ConnStr =>
            ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString;

        // ── GET /api/usuarios/5 ──────────────────────────────────────────────────
        [HttpGet, Route("{id:int}")]
        public IHttpActionResult ObtenerUsuario(int id)
        {
            try
            {
                using (var db = new dbAsistenciaDataContext(ConnStr))
                {
                    var u = db.tUsuario.FirstOrDefault(t => t.IdUsuario == id);
                    if (u == null) return NotFound();
                    return Ok(MapDto(u));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UsuariosApi.ObtenerUsuario] {ex.Message}");
                return InternalServerError(ex);
            }
        }

        // ── POST /api/usuarios/bulk ──────────────────────────────────────────────
        // Body: { "ids": [1, 9, 82] }
        // Retorna lista completa de EmpleadoDto para los IDs solicitados.
        [HttpPost, Route("bulk")]
        public IHttpActionResult ObtenerBulk([FromBody] BulkRequest request)
        {
            if (request?.Ids == null || request.Ids.Count == 0)
                return Ok(new List<EmpleadoDto>());

            try
            {
                var ids = request.Ids; // List<int> — LINQ to SQL soporta Contains
                using (var db = new dbAsistenciaDataContext(ConnStr))
                {
                    var lista = db.tUsuario
                        .Where(t => ids.Contains(t.IdUsuario))
                        .Select(t => t)
                        .ToList()
                        .Select(t => MapDto(t))
                        .ToList();

                    return Ok(lista);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UsuariosApi.ObtenerBulk] {ex.Message}");
                return InternalServerError(ex);
            }
        }

        // ── GET /api/usuarios/disponibles?excluirIds=1,9,82 ─────────────────────
        // Retorna empleados activos cuyos IdUsuario NO están en excluirIds.
        // Inventario pasa los UsuarioIDs que ya tienen cuenta.
        [HttpGet, Route("disponibles")]
        public IHttpActionResult ObtenerDisponibles([FromUri] string excluirIds = "")
        {
            try
            {
                var excluir = new List<int>();
                if (!string.IsNullOrEmpty(excluirIds))
                {
                    foreach (var s in excluirIds.Split(','))
                        if (int.TryParse(s.Trim(), out int id))
                            excluir.Add(id);
                }

                using (var db = new dbAsistenciaDataContext(ConnStr))
                {
                    var lista = db.tUsuario
                        .Where(t => t.Estatus == 1 && !excluir.Contains(t.IdUsuario))
                        .OrderBy(t => t.Nombre)
                        .ToList()
                        .Select(t => new EmpleadoResumenDto
                        {
                            IdUsuario = t.IdUsuario,
                            NombreCompleto =
                                (t.Nombre + " " + t.ApellidoPaterno +
                                 (!string.IsNullOrEmpty(t.ApellidoMaterno)
                                     ? " " + t.ApellidoMaterno : "") +
                                 " — " + (t.NumeroEmpleado ?? "")).Trim()
                        })
                        .ToList();

                    return Ok(lista);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UsuariosApi.ObtenerDisponibles] {ex.Message}");
                return InternalServerError(ex);
            }
        }

        // ── GET /api/usuarios/foto/5 ─────────────────────────────────────────────
        // Retorna { "foto": "base64string" } o { "foto": null }
        [HttpGet, Route("foto/{id:int}")]
        public IHttpActionResult ObtenerFoto(int id)
        {
            try
            {
                using (var db = new dbAsistenciaDataContext(ConnStr))
                {
                    var u = db.tUsuario.FirstOrDefault(t => t.IdUsuario == id);
                    if (u == null) return Ok(new FotoDto { Foto = null });

                    string base64 = null;
                    if (u.Foto != null && u.Foto.Length > 0)
                        base64 = Convert.ToBase64String(u.Foto.ToArray());

                    return Ok(new FotoDto { Foto = base64 });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UsuariosApi.ObtenerFoto] {ex.Message}");
                return InternalServerError(ex);
            }
        }

        // ── Mapper ───────────────────────────────────────────────────────────────
        private static EmpleadoDto MapDto(tUsuario u) => new EmpleadoDto
        {
            IdUsuario        = u.IdUsuario,
            Nombre           = u.Nombre           ?? "",
            ApellidoPaterno  = u.ApellidoPaterno  ?? "",
            ApellidoMaterno  = u.ApellidoMaterno  ?? "",
            Telefono         = u.Telefono         ?? "",
            TelefonoFamiliar = u.TelefonoFamiliar ?? "",
            Email            = u.Email            ?? "",
            NumeroEmpleado   = u.NumeroEmpleado   ?? "",
            Estatus          = u.Estatus          ?? 0
        };
    }
}
