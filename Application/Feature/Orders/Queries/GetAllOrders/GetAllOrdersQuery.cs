using System;
using System.Collections.Generic;
using MediatR;

namespace Application.Feature.Orders.Queries.GetAllOrders
{
    public record OrderItemsDto(string Name, decimal Price, int Quantity);

    public record OrderDto(
        Guid Id,
        string Status,
        decimal TotalPrice,
        DateTime CreatedAt,
        List<OrderItemsDto> Items
    );

   
    public record GetAllOrdersQuery(int PageNumber = 1, int PageSize = 10) : IRequest<List<OrderDto>>;
}