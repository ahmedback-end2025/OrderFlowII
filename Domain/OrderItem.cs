namespace Domain
{
    public class OrderItem
    {
        public int Id { get; private set; }

        public string Name { get; private set; }

        public decimal Price { get; private set; }

        public int Quantity { get; private set; }

        public OrderItem(string name, decimal price, int quantity)
        {
            
            Name = name;
            Price = price;
            Quantity = quantity;
        }






    }
}
