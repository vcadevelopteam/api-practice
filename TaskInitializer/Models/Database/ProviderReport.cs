using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class ProviderReport
    {
        [Column("distribuidor")]
        public string Distribuidor { get; set; }

        [Column("solicitud_mantenimiento")]
        public string SolicitudMantenimiento { get; set; }

        [Column("numero_orden")]
        public string NumeroOrden { get; set; }

        [Column("codigo_equipo")]
        public string CodigoEquipo { get; set; }

        [Column("descripcion_equipo")]
        public string DescripcionEquipo { get; set; }

        [Column("clave_cliente")]
        public string ClaveCliente { get; set; }

        [Column("nombre_cliente")]
        public string NombreCliente { get; set; }

        [Column("direccion")]
        public string Direccion { get; set; }

        [Column("tipo_mantenimiento")]
        public string TipoMantenimiento { get; set; }

        [Column("equipo_mantenimiento")]
        public string EquipoMantenimiento { get; set; }

        [Column("tecnico_responsable")]
        public string TecnicoResponsable { get; set; }

        [Column("fecha_solicitada")]
        public DateTime? FechaSolicitada { get; set; }

        [Column("fecha_finalización_ejecución")]
        public DateTime? FechaFinalizaciónEjecución { get; set; }

        [Column("estado")]
        public string Estado { get; set; }

        [Column("causa_reportada")]
        public string CausaReportada { get; set; }

        [Column("descripcion")]
        public string Descripcion { get; set; }

        [Column("reposicion_productos")]
        public string ReposicionProductos { get; set; }

        [Column("descripcion_producto")]
        public string DescripcionProducto { get; set; }

        [Column("cantidad")]
        public long? Cantidad { get; set; }

        [Column("costo")]
        public float? Costo { get; set; }

        [Column("porcentaje_reconocimiento")]
        public float? PorcentajeReconocimiento { get; set; }

        [Column("subtotal")]
        public float? Subtotal { get; set; }

        [Column("causa")]
        public string Causa { get; set; }

        [Column("problema")]
        public string Problema { get; set; }
    }
}