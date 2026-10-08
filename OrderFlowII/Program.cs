using Application;
using Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OrderFlowII.Diagnostics;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;

namespace OrderFlowII;

public class Program
{
    public static void Main(string[] args)
    {
        // 1. ????? Serilog ???????
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithSpan() // ??? TraceId ? SpanId ???????? ???? ?? ??? Log
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.GrafanaLoki("http://localhost:3100", labels: new[]
            {
                new LokiLabel { Key = "app", Value = "orderflow-api" }
            })
            .CreateLogger();

        try
        {
            var builder = WebApplication.CreateBuilder(args);

            // 2. ??????? Serilog ????? ??????? ???????
            builder.Host.UseSerilog();

            // 3. ??? ????? ??????? ??????? ???????
            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);

            // 4. ????? ??? Health Checks ???? SQL Server
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
            builder.Services.AddHealthChecks()
                .AddSqlServer(connectionString, name: "sql_server", tags: new[] { "db", "ready" });

            // 5. ????? ??? HealthCheck Publisher
            builder.Services.Configure<HealthCheckPublisherOptions>(options =>
            {
                options.Delay = TimeSpan.FromSeconds(5);
                options.Period = TimeSpan.FromSeconds(10);
            });
            builder.Services.AddSingleton<IHealthCheckPublisher, MetricsHealthCheckPublisher>();

            // 6. ????? ???? ???????? ????? ????????
            builder.Services.AddSingleton<OrderMetrics>();

            // 7. ????? OpenTelemetry (Metrics & Tracing)
            const string serviceName = "OrderFlow.Api";

            builder.Services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName))
                .WithMetrics(metrics =>
                {
                    metrics
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddMeter(MetricsHealthCheckPublisher.MeterName)
                        .AddMeter(OrderMetrics.MeterName)
                        .AddPrometheusExporter();
                })
                .WithTracing(tracing =>
                {
                    tracing
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddEntityFrameworkCoreInstrumentation(options =>
                        {
                            options.SetDbStatementForText = true; // ????? ?? ??????? SQL ?? ??? Trace
                        })
                        .AddOtlpExporter(opt =>
                        {
                            opt.Endpoint = new Uri("http://localhost:5317"); // ???? Jaeger OTLP
                        });
                });

            // 8. ????? ??? Web
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // 9. ????? ???? ?? ??? HTTP ??? Serilog
            app.UseSerilogRequestLogging();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();

            // 10. ???? ??????? ??? Health Checks ???? Prometheus
            app.MapHealthChecks("/health");
            app.MapPrometheusScrapingEndpoint();

            app.MapControllers();

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}