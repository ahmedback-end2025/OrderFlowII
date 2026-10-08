using System.Diagnostics.Metrics;

namespace OrderFlowII.Diagnostics
{
    public class OrderMetrics
    {
        public const string MeterName = "OrderFlow.Orders";
        private readonly Counter<long> _ordersCreatedCounter;
        private readonly Counter<long> _ordersCompletedCounter;
        private readonly Counter<long> _ordersCancelledCounter;

        public OrderMetrics(IMeterFactory meterFactory)
        {
            var meter = meterFactory.Create(MeterName);
            _ordersCreatedCounter = meter.CreateCounter<long>("orders_created_total", description: "Total number of created orders");
            _ordersCompletedCounter = meter.CreateCounter<long>("orders_completed_total", description: "Total number of completed orders");
            _ordersCancelledCounter = meter.CreateCounter<long>("orders_cancelled_total", description: "Total number of cancelled orders");
        }

        public void OrderCreated() => _ordersCreatedCounter.Add(1);
        public void OrderCompleted() => _ordersCompletedCounter.Add(1);
        public void OrderCancelled() => _ordersCancelledCounter.Add(1);
    }
}
