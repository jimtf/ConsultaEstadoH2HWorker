using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ConsultaEstadoH2HWorker.Infrastructure.ExternalServices.Auth;
using ConsultaEstadoH2HWorker.Infrastructure.ExternalServices.Config;
using ConsultaEstadoH2HWorker.Application.DTO;

namespace ConsultaEstadoH2HWorker.Infrastructure.ExternalServices
{
    public class H2HApiClient
    {
        private readonly H2HOptions _options;
        private readonly H2HTokenStore _tokenStore;
        private readonly HttpClient _httpClient;

        public H2HApiClient(HttpClient httpClient, IOptions<H2HOptions> options, H2HTokenStore tokenStore)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _tokenStore = tokenStore;

            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(60);
        }

        public async Task LoginAsync()
        {
            var loginRequest = new LoginH2HRequestDTO
            {
                UserName = _options.UserName,
                Password = _options.Password
            };

            string json = JsonConvert.SerializeObject(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(_options.LoginEndpoint, content);
            string responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error en login ({response.StatusCode}): {responseJson}");
            }

            var loginResponse = JsonConvert.DeserializeObject<LoginH2HResponseDTO>(responseJson);

            if (loginResponse == null || string.IsNullOrWhiteSpace(loginResponse.Token))
            {
                throw new Exception($"Login inválido. Respuesta: {responseJson}");
            }

            _tokenStore.GuardarToken(loginResponse.Token, loginResponse.ExpiresAt);
        }

        private async Task AsegurarTokenAsync(CancellationToken cancellationToken)
        {
            if (!_tokenStore.TieneTokenValido())
            {
                await LoginAsync();
            }

            string token = _tokenStore.ObtenerToken();

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new Exception("No se pudo obtener un token válido");
            }

            _httpClient.DefaultRequestHeaders.Authorization =new System.Net.Http.Headers.AuthenticationHeaderValue
            (
                "Bearer",
                token
            );

        }
        public async Task<ConsultaEstadoH2HResponseDTO> ConsultarAsync(Guid id, CancellationToken cancellationToken)
        {
            await AsegurarTokenAsync(cancellationToken);

            HttpResponseMessage response = await _httpClient.GetAsync( $"{_options.ConsultarEstadoEndpoint}/{id}", cancellationToken);
            string responseJson = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _tokenStore.Limpiar();

                await AsegurarTokenAsync(cancellationToken);

                response = await _httpClient.GetAsync( $"{_options.ConsultarEstadoEndpoint}/{id}", cancellationToken);

                responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception( $"Error consultando el estado de la transacción : {responseJson}");
            }

            var result = JsonConvert.DeserializeObject<ConsultaEstadoH2HResponseDTO>(responseJson);

            if (result == null)
            {
                throw new Exception("Respuesta inválida del servicio bancario:");
            }

            return result;
        }
    }
}
