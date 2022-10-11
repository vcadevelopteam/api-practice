using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class OdooProduct
    {
        [Column("solicitud")]
        public string Solicitud { get; set; }

        [Column("orden")]
        public string Orden { get; set; }

        [Column("orden_descripcion")]
        public string OrdenDescripcion { get; set; }

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

        [Column("producto")]
        public string Producto { get; set; }

        [Column("cantidad")]
        public long Cantidad { get; set; }

        [Column("problema")]
        public string Problema { get; set; }

        [Column("causa")]
        public string Causa { get; set; }

        [Column("solución")]
        public string Solución { get; set; }
    }
}