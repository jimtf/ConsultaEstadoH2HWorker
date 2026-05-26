using Newtonsoft.Json;

namespace ConsultaEstadoH2HWorker.Application.DTO
{
    public class LoginH2HResponseDTO
    {
        [JsonProperty("token")]
        public string? Token { get; set; }

        [JsonProperty("expiresAt")]
        public DateTime ExpiresAt { get; set; }
    }
}
