using Application.Common.Interfaces;
using Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Orders.Commands.CreateOrder
{
    public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<CreateOrderHandler> _logger;

        public CreateOrderHandler(
            IApplicationDbContext context,
            ILogger<CreateOrderHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting order creation for Customer: {CustomerName} with {ItemCount} items",
                request.CustomerName, request.Items.Count);

            var order = new Order(request.CustomerName);
            foreach (var item in request.Items)
            {
                order.AddItem(item.name, item.price, item.Quantity);
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order {OrderId} created successfully with Total Price: {TotalPrice}",
                order.Id, order.TotalPrice);

            return order.Id;
        }
    }
}
