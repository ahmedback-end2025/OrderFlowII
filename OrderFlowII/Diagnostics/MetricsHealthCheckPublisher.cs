using System.Diagnostics.Metrics;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OrderFlowII.Diagnostics;

public class MetricsHealthCheckPublisher : IHealthCheckPublisher
{
    public const string MeterName = "OrderFlow.HealthChecks";
    private static readonly Meter Meter = new(MeterName, "1.0.0");

   
    private static readonly Dictionary<string, double> HealthStatusMap = new();

    static MetricsHealthCheckPublisher()
    {
        Meter.CreateObservableGauge(
            name: "healthcheck_status",
            observeValues: () =>
            {
                lock (HealthStatusMap)
                {
                    return HealthStatusMap.Select(entry =>
                        new Measurement<double>(entry.Value, new KeyValuePair<string, object?>("check", entry.Key))
                    ).ToList();
                }
            },
            unit: "{status}",
            description: "Reports health check status (1 = Healthy, 0.5 = Degraded, 0 = Unhealthy)");
    }

    public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
    {
        lock (HealthStatusMap)
        {
            
            HealthStatusMap["overall"] = GetStatusScore(report.Status);

            
            foreach (var entry in report.Entries)
            {
                HealthStatusMap[entry.Key] = GetStatusScore(entry.Value.Status);
            }
        }

        return Task.CompletedTask;
    }

    private static double GetStatusScore(HealthStatus status) => status switch
    {
        HealthStatus.Healthy => 1.0,
        HealthStatus.Degraded => 0.5,
        HealthStatus.Unhealthy => 0.0,
        _ => 0.0
    };
}