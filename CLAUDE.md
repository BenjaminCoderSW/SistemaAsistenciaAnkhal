# CLAUDE.md — Sistema de Asistencia Ankhal

Guía de referencia rápida para Claude Code. Lee esto antes de cualquier tarea en el proyecto.

---

## Stack tecnológico

| Capa | Tecnología |
|---|---|
| Framework | ASP.NET WebForms (.aspx + code-behind .aspx.cs) |
| Lenguaje | C# / .NET 4.7.2 |
| ORM | LINQ-to-SQL (`dbAsistenciaDataContext`) |
| Base de datos | SQL Server (remoto en somee.com) |
| UI / CSS | AdminLTE 3 + Bootstrap 4 |
| Alertas JS | SweetAlert2 |
| Tablas JS | DataTables (con extensiones) |
| Idioma del sistema | Español (es-MX) |

---

## Estructura del repositorio

```
GrupoAnkhalAsistencia/           ← raíz del repositorio / solución .sln
└── GrupoAnkhalAsistencia/       ← proyecto web ASP.NET (Web Root)
    ├── Modelo/
    │   ├── dbAsistencia.cs
    │   └── dbAsistencia.designer.cs   ← LINQ-to-SQL auto-generado (NO editar a mano)
    ├── Sesion/
    │   └── SesionState.cs             ← sesión centralizada (static property)
    ├── Helpers/
    │   └── VacacionesHelper.cs        ← lógica de vacaciones (static methods)
    ├── dist/                          ← CSS/JS compilados de AdminLTE
    ├── plugins/                       ← librerías terceros (Bootstrap, jQuery, etc.)
    ├── scriptsPropios/                ← scripts JS propios del proyecto
    ├── css/img/                       ← recursos estáticos
    ├── Site.Master / Site.Master.cs   ← master page global (nav + sesión)
    ├── Global.asax / Global.asax.cs   ← timers automáticos nocturnos
    ├── Web.config                     ← connection strings + config .NET
    └── [44 páginas .aspx]
```

---

## Conexión a base de datos

**Web.config** — cadena principal:
```xml
<add name="AsistenciaAnkhalConnectionString"
     connectionString="Data Source=AsistenciaAnkhal.mssql.somee.com;
                       Initial Catalog=AsistenciaAnkhal;
                       User ID=SISTEMAS_SQLLogin_1;
                       Password=qiwxt3bycm;
                       TrustServerCertificate=True"
     providerName="System.Data.SqlClient" />
```
> Hay también `ConnectionString1`, `2`, `3` con la misma cadena (redundancia histórica).

**Instanciación en cada page** — patrón estándar:
```csharp
public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
    ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);
```

El contexto se instancia a nivel de clase (field), no dentro de métodos.

---

## Patrón del code-behind (todas las páginas lo siguen)

```csharp
public partial class MiPagina : System.Web.UI.Page
{
    // 1. DataContext a nivel de clase
    public dbAsistenciaDataContext db = new dbAsistenciaDataContext(
        ConfigurationManager.ConnectionStrings["AsistenciaAnkhalConnectionString"].ConnectionString);

    // 2. Page_Load con guard IsPostBack
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
            CargarDatos();
    }

    // 3. Datos: LINQ joins → .ToList() → .DataBind()
    private void CargarDatos()
    {
        var query = from m in db.tTabla
                    join r in db.tOtra on m.IdX equals r.IdX
                    where m.Estatus == 2
                    select new { /* solo columnas necesarias */ };
        gridView.DataSource = query.ToList();
        gridView.DataBind();
    }

    // 4. Eventos con try-catch + SweetAlert2
    protected void btnGuardar_Click(object sender, EventArgs e)
    {
        try { /* lógica */ db.SubmitChanges(); }
        catch (Exception ex) { MostrarError("Error", ex.Message); }
    }

    // 5. Helper de alertas cliente via ScriptManager
    private void MostrarError(string titulo, string msg)
    {
        string script = $"Swal.fire('{titulo}', '{msg}', 'error');";
        ScriptManager.RegisterStartupScript(this, GetType(), "swal", script, true);
    }
}
```

---

## Archivos clave del framework

### `Sesion/SesionState.cs`
Acceso centralizado al usuario logueado. Usar siempre `SesionState.usuario`.

```csharp
// Leer usuario actual desde cualquier página:
var u = SesionState.usuario;           // tUsuario object
var id = SesionState.usuario.IdUsuario;
var rol = SesionState.usuario.tRol.Rol; // "Administrador" | "Rh" | "Empleado"
```

### `Site.Master.cs`
- Valida sesión en cada request → redirect a `login.aspx` si `SesionState.usuario == null`
- Controla visibilidad de menús por rol:
  - **Administrador** / **Rh** → acceso completo
  - **Empleado** → solo Asistencia y Vacaciones
- Foto de usuario: `varbinary(max)` en BD → Base64 string → `<img src="data:image/png;base64,..."/>`

### `Global.asax.cs`
Dos timers automáticos usando `System.Threading.Timer`:
| Timer | Hora | Acción |
|---|---|---|
| `timerVacaciones` | 2:00 AM diario | `sp_ActualizarVacacionesAniversario` |
| `timerFaltas` | 11:59 PM diario | `sp_RegistrarFaltasDelDia @Fecha` |

### `Helpers/VacacionesHelper.cs`
Métodos estáticos para vacaciones:
- `ActualizarDiasVacacionesAutomatico()` — llama al SP
- `CalcularDiasSegunAntiguedad(idUsuario)` — días según `tConfiguracionVacaciones`
- `TieneDiasDisponibles(idUsuario, diasSolicitados)`
- `DescontarDias(idUsuario, diasADescontar)`
- `RestaurarDias(idUsuario, diasARestaurar)`

---

## Base de datos — Esquema completo

### Tablas principales

| Tabla | PK | Descripción |
|---|---|---|
| `tUsuario` | `IdUsuario` | Empleados del sistema. Tiene FK a tRol, tArea, tPuesto, tPlanta |
| `tAsistencia` | `IdAsistencia` | Registro de asistencia diaria (tabla central, ~40 columnas) |
| `tPlanta` | `IdPlanta` | Ubicaciones físicas con coordenadas GPS y rango de IPs |
| `tHorario` | `IdHorario` | Horarios laborales (HoraInicio, HoraFin) |
| `tDia` | `IdDia` | Catálogo de días (Lunes–Domingo, numerados por DATEPART WEEKDAY) |
| `tAsignarHorario` | `IdAsignarHorario` | Asignación de horario+día a usuario. FK a tHorario, tDia, tUsuario |
| `tArea` | `IdArea` | Áreas / departamentos |
| `tPuesto` | `IdPuesto` | Puestos de trabajo |
| `tRol` | `IdRol` | Roles: "Administrador", "Rh", "Empleado" |
| `tJefe` | `IdJefe` | Jefes/supervisores (separados de tUsuario, tienen correo propio) |
| `tVacaciones` | `IdVacaciones` | Solicitudes de vacaciones |
| `tConfiguracionVacaciones` | `IdConfigVacaciones` | Tabla de política: AñosAntiguedad → DiasCorresponden |
| `tPermisoHora` | `IdPermisoHora` | Permisos por horas (con rango HoraInicio–HoraFin) |
| `tPermisoDias` | `IdPermisoDias` | Permisos por días (con rango FechaInicio–FechaFin) |
| `tComisionHoras` | `IdComisionHoras` | Comisiones por horas (salida y regreso mismo día) |
| `tComisionDia` | `IdComisionDia` | Comisiones por días (incluye Viajes, Hospedaje, Transporte) |
| `tJustificacion` | `IdJustificacion` | Justificaciones de falta/retardo. FK a tAsistencia |
| `tPapeleta` | `IdPapeleta` | Recibos de nómina (percepciones y deducciones) |
| `tAvisos` | `IdAviso` | Avisos/anuncios del sistema con FechaVigencia |
| `ConfigCorreo` | `IdConfig` | Configuración SMTP para envío de correos |

### Columnas clave de `tUsuario`
```
IdUsuario, IdRol, IdArea, IdPuesto, IdPlanta
Nombre, ApellidoPaterno, ApellidoMaterno
CURP, RfC, NumeroEmpleado, FechaNacimiento, FechaIngreso
Usuario (login), Clave (password — plaintext, sin hash)
Foto (varbinary MAX → imagen Base64 en UI)
DiasVacacionesDisponibles, FechaUltimaActualizacionVacaciones
Dispositivo1, Mac1, Dispositivo2, Mac2 (para control de checador)
Estatus (1=activo)
```

### Columnas clave de `tAsistencia`
```
IdAsistencia, IdUsuario, IdPlanta, IdAsignarHorario
IdPermisoDias, IdPermisoHoras, IdComisionDias, IdComisionHoras, IdVacaciones, IdJustificacion
Fecha, HoraEntrada, HoraSalida, HoraSalidaComer, HoraEntradaComer
HorasTrabajadas (time), HorasTrabajadasDecimal (decimal 18,2)
HoraComida (decimal), TipoPermiso
HoraSalidaPermiso, HoraEntradaPermiso, HorasPermiso
HoraSalidaComision, HoraEntradaComision, horasComision
DiasSalidaComision, DiasRegresoComision, DiasComision
EstatusEntrada, EstatusSalida, EstatusComida (varchar: "Puntual", "Retardo", "Falta", etc.)
latitud, longitud (decimal 18,10 — GPS entrada)
latitudSalida, longitudSalida (GPS salida)
MacEntrada, MacSalida, IP
DiaSalidaVacaciones, DiaEntradaVacaciones, DiasVacaciones
HorasExtras (decimal 18,2), EstatusHorasExtras
```

### Convención `Estatus` (int) — usada en casi todas las tablas

| Valor | Significado |
|---|---|
| `1` | Pendiente (o Activo en catálogos) |
| `2` | Aprobado / Aceptado / Autorizado |
| `3` | Rechazado / Cancelado |

### Vistas de base de datos

| Vista | Propósito |
|---|---|
| `v_validarhorario` | Horario del usuario para el día de HOY (usa DATEPART WEEKDAY) |
| `V_REPORTE_ASISTENCIA` | Reporte completo de asistencia con GPS concatenado |
| `V_HISTORIAL_EMPLEADO` | Historial de asistencia por empleado |
| `V_REPORTE_COMIDA` | Reporte de tiempo de comida con cálculo de duración |
| `V_DIAS_VACACIONES_USUARIOS` | Días disponibles + antigüedad calculada con OUTER APPLY |
| `V_PAPELETAS` | Nómina con TotalPercepciones, TotalDeducciones y NetoPagar calculados |
| `V_PermisosComisiones` | UNION de permisos/días, permisos/horas, comisiones/días, comisiones/horas |
| `vJustificacion` | Justificaciones con nombre de empleado, planta y horario |
| `v_aceptarJustificaion` | Vista para módulo de aprobación de justificaciones |
| `principal` | Vista simple de asistencia con nombre del empleado |

### Stored Procedures

| SP | Disparador | Descripción |
|---|---|---|
| `sp_ActualizarVacacionesAniversario` | Timer 2 AM / manual | Actualiza `DiasVacacionesDisponibles` en `tUsuario` según antigüedad y `tConfiguracionVacaciones` |
| `sp_RegistrarFaltasDelDia` | Timer 11:59 PM diario | Registra faltas automáticas del día con `@Fecha` como parámetro |

---

## Las 44 páginas .aspx

### Autenticación / Portales
| Página | Función |
|---|---|
| `login.aspx` | Login → redirige a PrincipalAdmin o PrincipalEmpleados según rol |
| `Default.aspx` | Landing vacío |
| `Panel.aspx` | Panel principal |
| `PrincipalAdmin.aspx` | Portal admin/RH |
| `PrincipalEmpleados.aspx` | Portal empleado |

### Asistencia
| Página | Función |
|---|---|
| `Checar.aspx` | Checador web (entrada/comida/salida + GPS + IP) |
| `RegistrarAsistencia.aspx` | Registro manual de asistencia (admin) |
| `RegistroEmpleado.aspx` | Registro multi-etapa con validación de horario, IP y GPS |
| `RegistroPlanta1.aspx` | Registro específico de planta |
| `HistorialEmpleado.aspx` | Historial de asistencia del empleado logueado |
| `GraficaEmpleado.aspx` | Gráfica de asistencia individual |
| `GraficaPuntualidad.aspx` | Gráfica de puntualidad |

### Comisiones
| Página | Función |
|---|---|
| `ComisionDias.aspx` | Solicitar comisión por días |
| `ComisionHoras.aspx` | Solicitar comisión por horas |
| `AprobarComisionDias.aspx` | Aprobar comisiones de días (admin) |
| `AceptarComisonHoras.aspx` | Aprobar comisiones de horas (admin) |
| `ReporteComisionesDias.aspx` | Reporte de comisiones por días |
| `ReporteComisionesHora.aspx` | Reporte de comisiones por horas |

### Permisos
| Página | Función |
|---|---|
| `PermisoDias.aspx` | Solicitar permiso por días |
| `PermisosHoras.aspx` | Solicitar permiso por horas |
| `AprobarPermisoDias.aspx` | Aprobar permisos de días |
| `AprobarPermisosHora.aspx` | Aprobar permisos de horas |
| `ReportePermisos.aspx` | Reporte de permisos por días |
| `ReportePermisosHoras.aspx` | Reporte de permisos por horas |

### Justificaciones
| Página | Función |
|---|---|
| `Justificacion.aspx` | Solicitar justificación de falta/retardo |
| `AceptarJustificacion.aspx` | Aprobar/rechazar justificaciones (admin) |
| `ReporteJustificacion.aspx` | Reporte de justificaciones |

### Vacaciones
| Página | Función |
|---|---|
| `PedirVacaciones.aspx` | Solicitar vacaciones (descuenta días disponibles) |
| `AprobarVacaciones.aspx` | Aprobar solicitudes de vacaciones |
| `ReporteVacaciones.aspx` | Reporte de vacaciones |
| `ConfiguracionVacaciones.aspx` | Configurar tabla AñosAntiguedad → DiasCorresponden |

### Reportes y Formatos
| Página | Función |
|---|---|
| `ReporteAsistencia.aspx` | Reporte general de asistencia |
| `ReporteComida.aspx` | Reporte de tiempos de comida |
| `Papeleta.aspx` | Gestión de papeletas de nómina |
| `ImprimirPapeleta.aspx` | Impresión/PDF de papeleta |
| `Avisos.aspx` | Gestión de avisos/anuncios |

### Catálogos (CRUD simples)
| Página | Función |
|---|---|
| `Area.aspx` | Catálogo de áreas |
| `Planta.aspx` | Catálogo de plantas (con GPS y rango IP) |
| `Puesto.aspx` | Catálogo de puestos |
| `Horarios.aspx` | Catálogo de horarios |
| `Jefe.aspx` | Catálogo de jefes/supervisores |
| `Usuario.aspx` | Alta/edición de empleados |
| `AsignarHorario.aspx` | Asignar horario + día a un empleado |
| `RegistrarFaltas.aspx` | Registro manual de faltas (admin) |

---

## Lógica de negocio importante

### Checador (`RegistroEmpleado.aspx`)
El check-in es multi-etapa en el día:
1. **Entrada** — registra HoraEntrada, latitud, longitud, IP, Mac
2. **Salida a comer** — registra HoraSalidaComer
3. **Regreso de comer** — registra HoraEntradaComer
4. **Salida** — registra HoraSalida, calcula HorasTrabajadas, HorasExtras

Validaciones al registrar:
- Detección de planta por **rango de IP** (`tPlanta.IP_INICIO`, `IP_FIN`)
- **GPS** capturado con la Geolocation API del navegador
- Validación de horario contra la vista `v_validarhorario` (filtra por DATEPART WEEKDAY)
- Cálculo automático de **retardo** vs puntual comparando contra `HoraInicio` del horario
- Integración de permisos de horas aprobados (`tPermisoHora.Estatus = 2`) que se descuentan del cálculo de horas trabajadas

### Control de zonas horarias
El servidor usa **Central Standard Time Mexico** — se usa `TimeZoneInfo.ConvertTime` al capturar asistencia para asegurar hora local correcta.

### Seguridad (importante: limitaciones conocidas)
- Contraseñas en `tUsuario.Clave` guardadas en **plaintext** (sin hash). No agregar hash sin coordinarlo.
- Autenticación: solo validación contra BD + `Session`. Sin tokens ni JWT.
- Autorización: solo por rol en `Site.Master.cs` (control de visibilidad de menú, no de acceso por URL).

---

## Convenciones de código

- **Naming de tablas**: prefijo `t` (tUsuario, tAsistencia) excepto `ConfigCorreo`
- **Naming de vistas**: prefijo `V_` mayúscula o `v_` minúscula (inconsistente en BD)
- **Naming de SPs**: prefijo `sp_`
- **PKs**: siempre `Id{NombreTabla}` (IdUsuario, IdAsistencia...)
- **FKs**: `Id{TablaReferenciada}` en tabla hija
- **Estatus**: siempre `int` con convención 1=Pendiente/Activo, 2=Aprobado, 3=Rechazado
- **Queries**: LINQ-to-SQL con proyección anónima para GridViews. `ExecuteQuery<T>()` para consultas complejas con SQL crudo.
- **Alertas**: siempre SweetAlert2 (`Swal.fire(...)`) invocado desde `ScriptManager.RegisterStartupScript`
- **Namespace del proyecto**: `MedicaMedens` (nombre heredado — no corresponde al proyecto actual)

---

## Tareas automáticas (Global.asax.cs)

```
Application_Start
├── IniciarActualizacionAutomaticaVacaciones()
│   └── Timer → cada 24h a las 02:00 AM → sp_ActualizarVacacionesAniversario
└── IniciarRegistroAutomaticoFaltas()
    └── Timer → cada 24h a las 23:59 PM → sp_RegistrarFaltasDelDia @Fecha=HOY
```

---

## Notas para futuras sesiones

- El namespace `MedicaMedens` en `SesionState.cs` es un vestigio de un proyecto anterior. No cambiarlo sin verificar referencias.
- `dbAsistencia.designer.cs` es auto-generado por el diseñador LINQ-to-SQL de Visual Studio. **No editar manualmente**. Cambios al modelo se hacen desde el diseñador `.dbml`.
- Hay 4 connection strings idénticos en Web.config (AsistenciaAnkhalConnectionString, 1, 2, 3). Siempre usar el primero (sin sufijo numérico).
- La columna `tComisionDia.Viajes` fue agregada recientemente (commit `6e75810`). Verificar que las vistas o reportes existentes la incluyan si es necesario.
- El campo `AñosAntiguedad` en `tConfiguracionVacaciones` tiene carácter especial (ñ) en el nombre de columna SQL — puede causar problemas con encoding en scripts externos.
