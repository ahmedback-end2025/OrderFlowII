using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Orders.Commands.CompleteOrder
{
    public record CompleteOrderCommand(Guid Id) : IRequest;
}
