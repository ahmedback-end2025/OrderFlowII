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

            
            builder.Host.UseSerilog();

            
            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);

            
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
            builder.Services.AddHealthChecks()
                .AddSqlServer(connectionString, name: "sql_server", tags: new[] { "db", "ready" });

            
            builder.Services.Configure<HealthCheckPublisherOptions>(options =>
            {
                options.Delay = TimeSpan.FromSeconds(5);
                options.Period = TimeSpan.FromSeconds(10);
            });
            builder.Services.AddSingleton<IHealthCheckPublisher, MetricsHealthCheckPublisher>();

            
            builder.Services.AddSingleton<OrderMetrics>();

            //  OpenTelemetry (Metrics & Tracing)
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
                            options.SetDbStatementForText = true; 
                        })
                        .AddOtlpExporter(opt =>
                        {
                            opt.Endpoint = new Uri("http://localhost:5317"); 
                        });
                });

            // 8. ????? ??? Web
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            
            app.UseSerilogRequestLogging();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();

            
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