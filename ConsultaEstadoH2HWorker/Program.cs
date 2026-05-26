using System.Text;
using ConsultaEstadoH2HWorker.Application.Services;
using ConsultaEstadoH2HWorker.Infrastructure.Repositories;
using ConsultaEstadoH2HWorker.Infrastructure.ExternalServices;
using ConsultaEstadoH2HWorker.Infrastructure.ExternalServices.Config;
using ConsultaEstadoH2HWorker.Infrastructure.ExternalServices.Auth;
using ConsultaEstadoH2HWorker.Workers;

var builder = Host.CreateApplicationBuilder(args);

Console.OutputEncoding = Encoding.UTF8;

builder.Services.Configure<H2HOptions>(builder.Configuration.GetSection("H2H"));
builder.Services.AddSingleton<H2HTokenStore>();
builder.Services.AddHttpClient<H2HApiClient>();
builder.Services.AddTransient<HostToHostRepository>();
builder.Services.AddTransient<TransaccionesService>();
builder.Services.AddHostedService<Worker>();

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
    options.SingleLine = true;
});

builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning);
builder.Logging.AddFilter("System", LogLevel.Warning);

var host = builder.Build();
host.Run();
