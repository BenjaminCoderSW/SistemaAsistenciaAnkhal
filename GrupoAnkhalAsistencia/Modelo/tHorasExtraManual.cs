using System;
using System.Data.Linq.Mapping;

namespace GrupoAnkhalAsistencia.Modelo
{
    [Table(Name = "dbo.tHorasExtraManual")]
    public class tHorasExtraManual
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = true)]
        public int IdHorasExtraManual { get; set; }

        [Column]
        public int IdUsuario { get; set; }

        [Column]
        public int IdPlanta { get; set; }

        [Column]
        public DateTime Fecha { get; set; }

        [Column]
        public decimal HorasExtras { get; set; }

        [Column]
        public string Descripcion { get; set; }

        [Column]
        public int IdCapturador { get; set; }

        [Column]
        public DateTime FechaCaptura { get; set; }

        [Column]
        public int EstatusAprobacion { get; set; }

        [Column]
        public int? IdAprobador { get; set; }

        [Column]
        public string MotivoAprobacion { get; set; }

        [Column]
        public DateTime? FechaAprobacion { get; set; }
    }
}
