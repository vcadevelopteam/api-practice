using System.ComponentModel.DataAnnotations.Schema;

namespace TaskInitializer.Models.Database
{
    public class ReportCron
    {
        [Column("tiempopromediorespuestaasesor2")]
        public string TiempoPromedioRespuestaAsesor2 { get; set; }

        [Column("tiempopromediorespuestapersona")]
        public string TiempoPromedioRespuestaPersona { get; set; }

        [Column("tiempopromediorespuestaasesor")]
        public string TiempoPromedioRespuestaAsesor { get; set; }

        [Column("tiempoprimerarespuestaasesor")]
        public string TiempoPrimeraRespuestAasesor { get; set; }

        [Column("fechaprimeraconversacion")]
        public string FechaPrimeraConversacion { get; set; }

        [Column("fechaprimerainteracion")]
        public string FechaPrimeraInteraccion { get; set; }

        [Column("fechaultimaconversacion")]
        public string FechaUltimaConversacion { get; set; }

        [Column("tiempoprimeraasignacion")]
        public string TiempoPrimerAasignacion { get; set; }

        [Column("tiempopromediorespuesta")]
        public string TiempoPromedioRespuesta { get; set; }

        [Column("canalpersonareferencia")]
        public string CanalPersonaReferencia { get; set; }

        [Column("horaprimerainteracion")]
        public string HoraPrimeraInteraccion { get; set; }

        [Column("tiempoprimerarespuesta")]
        public string TiempoPrimeraRespuesta { get; set; }

        [Column("estadoconversacion")]
        public string EstadoConversacion { get; set; }

        [Column("holdingwaitingtime")]
        public string HoldingWaitingTime { get; set; }

        [Column("canalpersonatipo")]
        public string CanalPersonaTipo { get; set; }

        [Column("estadoevaluacion")]
        public string EstadoEvaluacion { get; set; }

        [Column("tiemposuspension")]
        public string TiempoSuspension { get; set; }

        [Column("fechaderivacion")]
        public string FechaDerivacion { get; set; }

        [Column("attentiongroup")]
        public string AttentionGroup { get; set; }

        [Column("horaderivacion")]
        public string HoraDerivacion { get; set; }

        [Column("asesorinicial")]
        public string AsesorInicial { get; set; }

        [Column("duracionpausa")]
        public string DuracionPausa { get; set; }

        [Column("duraciontotal")]
        public string DuracionTotal { get; set; }

        [Column("tipodocumento")]
        public string TipoDocumento { get; set; }

        [Column("duracionreal")]
        public string DuracionReal { get; set; }

        [Column("fechahandoff")]
        public string FechaHandoff { get; set; }

        [Column("numeroticket")]
        public string NumeroTicket { get; set; }

        [Column("tipificacion")]
        public string Tipificacion { get; set; }

        [Column("asesorfinal")]
        public string AsesorFinal { get; set; }

        [Column("displayname")]
        public string DisplayName { get; set; }

        [Column("fechainicio")]
        public string FechaInicio { get; set; }

        [Column("tipocierre2")]
        public string TipoCierre2 { get; set; }

        [Column("cerradopor")]
        public string CerradoPor { get; set; }

        [Column("horainicio")]
        public string HoraInicio { get; set; }

        [Column("horaticket")]
        public string HoraTicket { get; set; }

        [Column("tipocierre")]
        public string TipoCierre { get; set; }

        [Column("abandoned")]
        public string Abandoned { get; set; }

        [Column("firstname")]
        public string FirstName { get; set; }

        [Column("tmoasesor")]
        public string TmoAsesor { get; set; }

        [Column("fechafin")]
        public string FechaFin { get; set; }

        [Column("lastname")]
        public string LastName { get; set; }

        [Column("cliente")]
        public string Cliente { get; set; }

        [Column("horafin")]
        public string HoraFin { get; set; }

        [Column("asesor")]
        public string Asesor { get; set; }

        [Column("labels")]
        public string Labels { get; set; }

        [Column("origin")]
        public string Origin { get; set; }

        [Column("canal")]
        public string Canal { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("phone")]
        public string Phone { get; set; }

        [Column("dni")]
        public string Dni { get; set; }

        [Column("tmg")]
        public string Tmg { get; set; }

        [Column("tmo")]
        public string Tmo { get; set; }

        [Column("balancetimes")]
        public double? BalanceTimes { get; set; }

        [Column("semana")]
        public double? Semana { get; set; }

        [Column("anio")]
        public double? Anio { get; set; }

        [Column("hora")]
        public double? Hora { get; set; }

        [Column("dia")]
        public double? Dia { get; set; }

        [Column("mes")]
        public double? Mes { get; set; }

        [Column("variablecontext")]
        public string VariableContext { get; set; }
    }
}