using Application.Feature.Orders.Queries.GetAllOrders;
using Domain;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;

namespace OrderFlowII.Tests
{
    public class GetAllOrdersHandlerTest : SqliteTestBase
    {
        private readonly GetAllHandler _handler;

        public GetAllOrdersHandlerTest()
        {
            var logger = new Mock<ILogger<GetAllHandler>>();
            _handler = new GetAllHandler(Context, logger.Object);
        }

        [Fact]
        public async Task Handle_WhenDataBaseIsEmpty_ReturnEmptyCollection()
        {
            var query = new GetAllOrdersQuery();

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.Empty(result);
        }


        [Fact]
        public async Task Handel_WhenCreatedDatesIsDifferent_ReturnSortedCollection()
        {
            var order1 = new Order("Samy");
            var order2 = new Order("Mohammed");
            var order3 = new Order("Ahmed");

            Context.Orders.Add(order1);
            Context.Orders.Add(order2);
            Context.Orders.Add(order3);



            Context.Orders.Entry(order1).Property(o => o.CreatedAt).CurrentValue = new DateTime(2026, 10, 1);
            Context.Orders.Entry(order2).Property(o => o.CreatedAt).CurrentValue = new DateTime(2026, 10, 10);
            Context.Orders.Entry(order3).Property(o => o.CreatedAt).CurrentValue = new DateTime(2026, 10, 15);

            await Context.SaveChangesAsync();

            var query = new GetAllOrdersQuery();
            var result = await _handler.Handle(query, CancellationToken.None);


            Assert.NotEmpty(result);
            Assert.Equal(3, result.Count);

            Assert.Equal([order3.Id, order2.Id, order1.Id], result.Select(o => o.Id));
        }



        [Theory]
        [InlineData(1, 2, new[] { 4, 3 })]       // الصفحة 1: الأحدث
        [InlineData(2, 2, new[] { 2, 1 })]       // الصفحة 2: اللي بعدهم
        [InlineData(3, 2, new[] { 0 })]          // آخر صفحة ناقصة (عنصر واحد)
        [InlineData(10, 2, new int[] { })]       // صفحة بعد الداتا: فاضية
        [InlineData(1, 10, new[] { 4, 3, 2, 1, 0 })]  // PageSize أكبر من الداتا: كلها
        public async Task Handle_WhenPaging_ReturnsExpectedPage(int pgnum , int pgsize , int[] eIds)
        {
            var order1 = new Order("Samy");
            var order2 = new Order("Mohammed");
            var order3 = new Order("Ahmed");
            var order4 = new Order("Gharb");
            var order5 = new Order("Saeed");

            Context.Orders.Add(order1);
            Context.Orders.Add(order2);
            Context.Orders.Add(order3);
            Context.Orders.Add(order4);
            Context.Orders.Add(order5);



            Context.Orders.Entry(order1).Property(o => o.CreatedAt).CurrentValue = new DateTime(2026, 10, 1);
            Context.Orders.Entry(order2).Property(o => o.CreatedAt).CurrentValue = new DateTime(2026, 10, 10);
            Context.Orders.Entry(order3).Property(o => o.CreatedAt).CurrentValue = new DateTime(2026, 10, 15);
            Context.Orders.Entry(order4).Property(o => o.CreatedAt).CurrentValue = new DateTime(2026, 10, 16);
            Context.Orders.Entry(order5).Property(o => o.CreatedAt).CurrentValue = new DateTime(2026, 10, 17);

            await Context.SaveChangesAsync();


            var query = new GetAllOrdersQuery(pgnum,pgsize);
            var result = await _handler.Handle(query, CancellationToken.None);


            
           


            var orders = new[] { order1, order2, order3, order4, order5 };
            var expectedIds = eIds.Select(o => orders[o].Id);
            Assert.Equal(expectedIds, result.Select(o => o.Id));
        }
    }

}
