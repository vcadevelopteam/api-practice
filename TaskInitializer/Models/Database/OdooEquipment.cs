using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class OdooEquipment
    {
        [Column("solicitud")]
        public string Solicitud { get; set; }

        [Column("prioridad")]
        public string Prioridad { get; set; }

        [Column("orden")]
        public string Orden { get; set; }

        [Column("tipo_problema")]
        public string TipoProblema { get; set; }

        [Column("estado")]
        public string Estado { get; set; }

        [Column("placa")]
        public string Placa { get; set; }

        [Column("fecha_solicitud")]
        public string FechaSolicitud { get; set; }

        [Column("fecha_inicio_real")]
        public string FechaInicioReal { get; set; }

        [Column("fecha_fin_real")]
        public string FechaFinReal { get; set; }

        [Column("distribuidor")]
        public string Distribuidor { get; set; }

        [Column("idcliente")]
        public string IdCliente { get; set; }

        [Column("cliente")]
        public string Cliente { get; set; }

        [Column("ruc")]
        public string Ruc { get; set; }

        [Column("dni")]
        public string Dni { get; set; }
    }
}