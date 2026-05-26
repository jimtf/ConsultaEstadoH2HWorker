using Newtonsoft.Json;

namespace ConsultaEstadoH2HWorker.Application.DTO
{
    public class LoginH2HRequestDTO
    {
        [JsonProperty("userName")]
        public string? UserName { get; set; }

        [JsonProperty("password")]
        public string? Password { get; set; }
    }
}
