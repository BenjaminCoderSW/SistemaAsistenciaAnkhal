using System.Data.Linq;

namespace GrupoAnkhalAsistencia.Modelo
{
    public partial class dbAsistenciaDataContext
    {
        public Table<tHorasExtraManual> tHorasExtraManual => GetTable<tHorasExtraManual>();
    }
}
