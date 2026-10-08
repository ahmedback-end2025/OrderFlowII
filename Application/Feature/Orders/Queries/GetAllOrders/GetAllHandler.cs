using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Feature.Orders.Queries.GetAllOrders
{
    public class GetAllHandler : IRequestHandler<GetAllOrdersQuery, List<OrderDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<GetAllHandler> _logger;
        public GetAllHandler(IApplicationDbContext context, ILogger<GetAllHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<OrderDto>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving list of all orders");

            var orders = await _context.Orders
                .AsNoTracking() 
                .OrderByDescending(o => o.CreatedAt) 
                .Skip((request.PageNumber - 1) * request.PageSize) 
                .Take(request.PageSize) 
                .Select(order => new OrderDto(
                    order.Id,
                    order.orderStatus.ToString(),
                    order.TotalPrice,
                    order.CreatedAt,
                    order.Items.Select(item => new OrderItemsDto(
                        item.Name,
                        item.Price,
                        item.Quantity
                    )).ToList()
                ))
                .ToListAsync(cancellationToken);
            _logger.LogInformation("Retrieved {OrderCount} orders successfully", orders.Count);
            return orders;
        }
    }
}