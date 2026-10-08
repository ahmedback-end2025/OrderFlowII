using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Orders.Commands.CancelOrder
{
    public class CancelOrderHandler : IRequestHandler<CancelOrderCommand, bool>
    {
        private readonly IApplicationDbContext context;
        private readonly ILogger<CancelOrderHandler> _logger;

        public CancelOrderHandler(IApplicationDbContext context, ILogger<CancelOrderHandler> logger)
        {
            this.context = context;
            _logger = logger;
        }

        public async Task<bool> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attempting to cancel order with ID: {OrderId}", request.Id);

            var order = await context.Orders
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (order is null)
            {
                _logger.LogWarning("Failed to cancel order: Order with ID: {OrderId} not found", request.Id);
                return false;
            }

            if (!order.CancelOrder())
            {
                _logger.LogWarning("Failed to cancel order with ID: {OrderId}. Order cannot be cancelled in its current state", request.Id);
                return false;
            }

            await context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order with ID: {OrderId} has been successfully cancelled", request.Id);
            return true;
        }
    }
}