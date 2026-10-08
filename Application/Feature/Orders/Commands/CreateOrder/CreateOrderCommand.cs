using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Orders.Commands.CreateOrder
{
    public record OrderItemsDto(string name, decimal price, int Quantity);
   public record CreateOrderCommand(string CustomerName ,List<OrderItemsDto> Items ): IRequest<Guid>;
}
