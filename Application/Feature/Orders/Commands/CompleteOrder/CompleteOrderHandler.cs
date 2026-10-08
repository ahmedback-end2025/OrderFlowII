using Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Orders.Commands.CompleteOrder
{
    public class CompleteOrderHandler : IRequestHandler<CompleteOrderCommand>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<CompleteOrderHandler> _logger;

        public CompleteOrderHandler(IApplicationDbContext context, ILogger<CompleteOrderHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Handle(CompleteOrderCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attempting to complete Order ID: {OrderId}", request.Id);

            var order = await _context.Orders.FindAsync(new object[] { request.Id }, cancellationToken);

            if (order == null)
                throw new KeyNotFoundException($"Order with ID {request.Id} not found.");

            order.CompleteOrder();

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order ID: {OrderId} has been successfully completed", order.Id);

        }
    }
}
