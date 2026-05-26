using Newtonsoft.Json;

namespace ConsultaEstadoH2HWorker.Application.DTO
{
    public class ConsultaEstadoH2HResponseDTO
    {
        [JsonProperty("codigoUnico")]
        public Guid CodigoUnico { get; set; }

        [JsonProperty("estado")]
        public int Estado { get; set; }

        [JsonProperty("refBCT")]
        public string RefBCT { get; set; } = string.Empty;

        [JsonProperty("detalle")]
        public List<ConsultaEstadoH2HDetalleDTO> Detalle { get; set; } = new();

        [JsonProperty("mensaje")]
        public string? Mensaje { get; set; }
    }
}
