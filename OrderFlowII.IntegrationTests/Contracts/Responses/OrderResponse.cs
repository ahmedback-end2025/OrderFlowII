namespace OrderFlowII.IntegrationTests.Contracts.Responses;


    public record OrderResponse(
    Guid Id,
    string Status,
    decimal TotalPrice,
    DateTime CreatedAt,
    List<OrderItemResponse> Items
);

public record OrderItemResponse(
    string Name,
    decimal Price,
    int Quantity
);


public record OrderRequest(string CustomerName , List<OrderItemResponse> Items);