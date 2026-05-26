using System.Diagnostics;
using ConsultaEstadoH2HWorker.Application.DTO;
using ConsultaEstadoH2HWorker.Infrastructure.ExternalServices;
using ConsultaEstadoH2HWorker.Infrastructure.Repositories;
using static ConsultaEstadoH2HWorker.Application.Enums.EstadoTransaccionH2HEnum;

namespace ConsultaEstadoH2HWorker.Application.Services
{
    public class TransaccionesService
    {
        private readonly HostToHostRepository _repository;
        private readonly H2HApiClient _h2hApiClient;
        private readonly ILogger _logger;

        public TransaccionesService(HostToHostRepository repository, H2HApiClient h2HApiClient, ILoggerFactory loggerFactory)
        {
            _repository = repository;
            _h2hApiClient = h2HApiClient;
            _logger = loggerFactory.CreateLogger("TransaccionesService");
        }

        public async Task ConsultarEstadosAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("ℹ️ Consultando transacciones pendientes");

            var pendientes = await _repository.ObtenerTransaccionesPendientesAsync();
            var pendientesList = pendientes.ToList();
            var actualizables = new List<(TransaccionPendienteDTO transaccion, ConsultaEstadoH2HResponseDTO respuesta)>();

            if (!pendientesList.Any())
            {
                _logger.LogInformation("ℹ️ No se encontraron transacciones pendientes");
                return;
            }

            _logger.LogInformation("ℹ️ Consultando el estado de {cantidad} transacciones en banco", pendientesList.Count());

            foreach (var transaccion in pendientesList)
            {

                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    _logger.LogInformation("ℹ️ Consultando identificador {IdTransaccionBanco}", transaccion.IdTransaccionBanco);

                    var bancoSw = Stopwatch.StartNew();

                    var respuesta = await _h2hApiClient.ConsultarAsync(transaccion.IdTransaccionBanco, cancellationToken);

                    bancoSw.Stop();

                    _logger.LogInformation("ℹ️ Consulta banco finalizada en {ElapsedMs} ms para {IdTransaccionBanco}",
                        bancoSw.ElapsedMilliseconds, transaccion.IdTransaccionBanco);

                    if (respuesta.Estado == (int)EstadoTransaccionH2H.Pendiente_Procesar) continue;

                    actualizables.Add((transaccion, respuesta));
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("🛑 Cancelando consulta de transacciones");
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❎ Error consultando transacción {IdTransaccionBanco}", transaccion.IdTransaccionBanco);
                }
            }

            _logger.LogInformation("ℹ️ Finalizó consulta de estado de transacciones en banco");

            if (!actualizables.Any())
            {
                _logger.LogInformation("ℹ️ No hay transacciones por actualizar");
                return;
            }

            try
            {
                _logger.LogInformation("ℹ️ Actualizando el estado de {cantidad} transacciones en SPCR", actualizables.Count);

                string xml = GenerarXmlBatch(actualizables);

                var updateSw = Stopwatch.StartNew();

                await _repository.ActualizarTransaccionesPendientesAsync(xml);

                updateSw.Stop();

                _logger.LogInformation("✅ Estado de transacciones actualizado en SPCR en {ElapsedMs} ms", updateSw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❎ Error actualizando batch H2H");
            }
        }

        private static string GenerarXmlBatch(List<(TransaccionPendienteDTO transaccion, ConsultaEstadoH2HResponseDTO respuesta)> items)
        {
            var xml = new System.Xml.XmlDocument();
            var root = xml.CreateElement("Bloques");

            foreach (var item in items)
            {
                var bloqueNode = xml.CreateElement("Bloque");

                bloqueNode.SetAttribute("idBloque", item.transaccion.IdBloque.ToString());
                bloqueNode.SetAttribute("codEstado", item.respuesta.Estado.ToString());
                bloqueNode.SetAttribute("referenciaBanco", item.respuesta.RefBCT ?? string.Empty);

                foreach (var detalle in item.respuesta.Detalle)
                {
                    var detalleNode = xml.CreateElement("Detalle");

                    detalleNode.SetAttribute("concepto", detalle.Concepto);
                    detalleNode.SetAttribute("id", detalle.Id.ToString());
                    detalleNode.SetAttribute("exitoso", detalle.Exitoso ? "1" : "0");

                    bloqueNode.AppendChild(detalleNode);
                }

                root.AppendChild(bloqueNode);
            }

            xml.AppendChild(root);

            return xml.InnerXml;
        }
    }
}
