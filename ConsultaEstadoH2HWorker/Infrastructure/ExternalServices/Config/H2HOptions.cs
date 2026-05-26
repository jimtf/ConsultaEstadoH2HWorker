namespace ConsultaEstadoH2HWorker.Infrastructure.ExternalServices.Config
{
    public class H2HOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string LoginEndpoint { get; set; } = string.Empty;
        public string ConsultarEstadoEndpoint { get; set; } = string.Empty;
    }
}
