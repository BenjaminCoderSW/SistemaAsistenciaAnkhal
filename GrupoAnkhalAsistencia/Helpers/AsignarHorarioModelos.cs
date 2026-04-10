namespace GrupoAnkhalAsistencia
{
    // Fila del GridView pivotado: un registro por empleado activo con asignaciones.
    // Cada propiedad DiaX contiene la descripción del horario asignado ese día, o null si no aplica.
    public class HorarioSemanalRow
    {
        public int    IdUsuario      { get; set; }
        public string NombreCompleto { get; set; }

        // IdDia en tDia: 1=DOMINGO, 2=LUNES, 3=MARTES, 4=MIERCOLES, 5=JUEVES, 6=VIERNES, 7=SABADO
        public string Lunes     { get; set; }
        public string Martes    { get; set; }
        public string Miercoles { get; set; }
        public string Jueves    { get; set; }
        public string Viernes   { get; set; }
        public string Sabado    { get; set; }
        public string Domingo   { get; set; }
    }

    // Auxiliar usado dentro de btnGuardarSemana_Click para iterar los 7 días genéricamente.
    public class DiaAsignacion
    {
        public int IdDia     { get; set; }
        public int IdHorario { get; set; }  // 0 = sin horario (acción: soft-delete si existía)
    }
}
