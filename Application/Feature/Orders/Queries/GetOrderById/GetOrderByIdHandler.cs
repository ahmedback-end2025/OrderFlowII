using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Orders.Queries.GetOrderById
{
    public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<GetOrderByIdHandler> _logger;

        public GetOrderByIdHandler(IApplicationDbContext context, ILogger<GetOrderByIdHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching order details for Order ID: {OrderId}", request.Id);

            var order = await _context.Orders
                .AsNoTracking()
                .Where(i => i.Id == request.Id)
                .Select(order => new OrderDto(
                    order.Id,
                    order.orderStatus.ToString(),
                    order.TotalPrice,
                    order.CreatedAt,
                    order.Items.Select(item => new OrderItemsDto(
                        item.Name,
                        item.Price,
                        item.Quantity
                    )).ToList()))
                .FirstOrDefaultAsync(cancellationToken);

            if (order is null)
            {
                _logger.LogWarning("Order with ID: {OrderId} was not found", request.Id);
                return null;
            }

            _logger.LogInformation("Order with ID: {OrderId} retrieved successfully", request.Id);
            return order;
        }
    }
}