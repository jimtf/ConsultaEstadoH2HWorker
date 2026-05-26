using System.Diagnostics;
using ConsultaEstadoH2HWorker.Application.Services;

namespace ConsultaEstadoH2HWorker.Workers;

public class Worker : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly int _intervalMinutes;

    public Worker(ILoggerFactory loggerFactory, IServiceScopeFactory scopeFactory, IConfiguration config)
    {        
        _logger = loggerFactory.CreateLogger("Worker");
        _scopeFactory = scopeFactory;
        _config = config;
        _intervalMinutes = int.Parse(_config["Worker:IntervalMinutes"]!);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("🟩 Ciclo iniciado");

                using var scope = _scopeFactory.CreateScope();

                var service = scope.ServiceProvider .GetRequiredService<TransaccionesService>();

                await service .ConsultarEstadosAsync(stoppingToken);

                stopwatch.Stop();

                _logger.LogInformation("🟧 Ciclo finalizado en {ElapsedMs} ms", stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(ex, "❎ Error ejecutando ciclo de consultas H2H luego de despu{ElapsedMs} ms", stopwatch.ElapsedMilliseconds);
            }

            await Task.Delay(TimeSpan.FromMinutes(_intervalMinutes), stoppingToken);
        }
    }
}
