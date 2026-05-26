using Newtonsoft.Json;

namespace ConsultaEstadoH2HWorker.Application.DTO
{
    public class ConsultaEstadoH2HDetalleDTO
    {
        [JsonProperty("origen")]
        public string? Origen { get; set; }

        [JsonProperty("destino")]
        public string? Destino { get; set; }

        [JsonProperty("monto")]
        public decimal Monto { get; set; }

        [JsonProperty("moneda")]
        public int Moneda { get; set; }

        [JsonProperty("concepto")]
        public string? Concepto { get; set; }

        [JsonProperty("idBeneficiaria")]
        public int IdBeneficiaria { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("exitoso")]
        public bool Exitoso { get; set; }
    }
}
