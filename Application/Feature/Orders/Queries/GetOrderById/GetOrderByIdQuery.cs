using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Orders.Queries.GetOrderById
{
    public record OrderItemsDto(string Name, decimal Price, int Quantity);

    public record OrderDto(
        Guid Id,
        string Status,
        decimal TotalPrice,
        DateTime CreatedAt,
        List<OrderItemsDto> Items
    );


    public record GetOrderByIdQuery(Guid Id) : IRequest<OrderDto?>;
}
