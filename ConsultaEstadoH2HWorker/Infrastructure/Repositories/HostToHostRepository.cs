using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ConsultaEstadoH2HWorker.Application.DTO;

namespace ConsultaEstadoH2HWorker.Infrastructure.Repositories
{
    public class HostToHostRepository
    {
        private readonly string _connectionString;

        public HostToHostRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SPCR")!;
        }

        public async Task<IEnumerable<TransaccionPendienteDTO>> ObtenerTransaccionesPendientesAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<TransaccionPendienteDTO>
            (
                "espConsultaTransaccionesPendientesH2H", 
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task ActualizarTransaccionesPendientesAsync(string xml)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new
            {
                transacciones = xml
            };

            await connection.ExecuteAsync("espActualizarTransaccionesPendientesH2H", parameters, commandType: CommandType.StoredProcedure);
        }
    }
}
