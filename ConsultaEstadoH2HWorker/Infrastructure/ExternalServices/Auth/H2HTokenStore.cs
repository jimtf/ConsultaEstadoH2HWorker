
namespace ConsultaEstadoH2HWorker.Infrastructure.ExternalServices.Auth
{
    public class H2HTokenStore
    {
        private readonly object _lock = new object();
        private string? _token;
        private DateTimeOffset _expiresAt;

        public bool TieneTokenValido()
        {
            lock (_lock)
            {
                return !string.IsNullOrWhiteSpace(_token) && DateTimeOffset.UtcNow < _expiresAt.AddSeconds(-60);
            }
        }

        public string ObtenerToken()
        {
            lock (_lock)
            {
                return _token;
            }
        }

        public void GuardarToken(string token, DateTimeOffset expiresAt)
        {
            lock (_lock)
            {
                _token = token;
                _expiresAt = expiresAt;
            }
        }

        public void Limpiar()
        {
            lock (_lock)
            {
                _token = null;
                _expiresAt = DateTimeOffset.MinValue;
            }
        }
    }
}
