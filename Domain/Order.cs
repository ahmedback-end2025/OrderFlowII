namespace Domain
{
    public class Order
    {
        public Guid Id { get; private set; }

        public string CustomerName { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public decimal TotalPrice { get; private set; }


        private readonly List<OrderItem> _items = new();
        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
        public OrderStatus orderStatus { get; set; }


        public Order()
        {
            
        }

        public Order(string Customername)
        {
            if (string.IsNullOrWhiteSpace(Customername))
                throw new ArgumentException("Customer name cannot be empty.", nameof(CustomerName));
            Id = Guid.NewGuid();
            CustomerName = Customername;
            orderStatus = OrderStatus.Pending;
            CreatedAt = DateTime.Now;
            TotalPrice = 0;
        }


        public void AddItem(string name, decimal price, int Quantity)
        {
            OrderItem orderItem = new OrderItem(name, price, Quantity);

            _items.Add(orderItem);

            TotalPrice += (price * Quantity);
        }


        public bool CancelOrder()
        {
            if (orderStatus == OrderStatus.Completed)
                return false;

            if (orderStatus == OrderStatus.Canceled)
                return false;

            orderStatus = OrderStatus.Canceled;

            return true;
        }

        public void CompleteOrder()
        {
            if (orderStatus != OrderStatus.Pending)
                throw new InvalidOperationException("Only pending orders can be completed.");

            orderStatus = OrderStatus.Completed;
        }
    }
}
