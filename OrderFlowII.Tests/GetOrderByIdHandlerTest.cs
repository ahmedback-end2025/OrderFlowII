using Application.Feature.Orders.Queries.GetOrderById;
using Domain;
using Microsoft.Extensions.Logging;
using Moq;

namespace OrderFlowII.Tests
{
    public class GetOrderByIdHandlerTest : SqliteTestBase
    {
        private readonly GetOrderByIdHandler _handler;

        public GetOrderByIdHandlerTest()
        {
            var logger = new Mock<ILogger<GetOrderByIdHandler>>();
            _handler = new GetOrderByIdHandler(Context, logger.Object);
        }


        //First Id Is Null

        [Fact]
        public async Task Handle_whenIdIsNull_ReturnNull()
        {
            var query = new GetOrderByIdQuery(Guid.NewGuid());

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.Null(result);
        }


        [Fact]
        public async Task Handle_WhenOrderIsExist_RetrunOrder()
        {
            var order = new Order("Samy");
            order.AddItem("Mouse", 100M, 5);
            Context.Orders.Add(order);
            await Context.SaveChangesAsync();
            Context.ChangeTracker.Clear();

            var query = new GetOrderByIdQuery(order.Id);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Context.ChangeTracker.Clear();
            var saved = await Context.Orders.FindAsync(order.Id);
            Assert.Equal(order.Id, saved!.Id);
            Assert.Equal(order.Id, result.Id);
            Assert.Equal(500M, result.TotalPrice);
            Assert.Contains(result.Items, i => i.Name == "Mouse" && i.Price == 100M && i.Quantity == 5);
            
        }

        [Fact]
        public async Task Handle_WhenMultipleOrdersExist_ReturnsOnlyRequestedOrder()
        {
            // Arrange
            var target = new Order("Ahmed");
            target.AddItem("Mouse", 100M, 1);
            var other = new Order("Sara");
            other.AddItem("Monitor", 900M, 1);
            Context.Orders.AddRange(target, other);
            await Context.SaveChangesAsync();

            // Act
            var result = await _handler.Handle(new GetOrderByIdQuery(target.Id), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(target.Id, result.Id);
            Assert.Single(result.Items);
            Assert.Equal("Mouse", result.Items[0].Name);
        }
    }
}
