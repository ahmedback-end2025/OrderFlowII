using FluentAssertions;
using OrderFlowII.IntegrationTests.Contracts.Responses;
using System;
using System.Net;
using System.Net.Http.Json;

namespace OrderFlowII.IntegrationTests
{
    [Collection("Api")]
    public class GetOrderByIdEndPointTest:IntegrationTestBase
    {
       

        public GetOrderByIdEndPointTest(SqlServerContainerFixture fixture):base(fixture)
        {
            
        }


        private static object NewOrder() => new
        {
            customername = "Ahmed",
            Items = new[]
            {
               new {name = "Ahmed" , price = 180m , quantity =2 }

            }
        };

        [Fact]
        public async Task GetOrderById_ExistingOrder_Returns200WithData()
        {
            var response = await _client.PostAsJsonAsync("/api/order", NewOrder());
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var id = await response.Content.ReadFromJsonAsync<Guid>();
            id.Should().NotBe(Guid.Empty);

            var getresponse = await _client.GetAsync($"/api/order/{id}");
            getresponse.Should().Be(HttpStatusCode.OK);

            var order = await getresponse.Content.ReadFromJsonAsync<OrderResponse>();


            order.Should().NotBeNull();

            order.Should().BeEquivalentTo(new
            {
                Id = id,
                Status = "Pending",
                TotalPrice = 360m,
                Items = new[]
                {
        new
        {
            Name = "Ahmed",
            Price = 180m,
            Quantity = 2
        }
    }
            },
            options => options.ExcludingMembersNamed("CreatedAt"));

        }
        [Fact]
        public async Task GetOrderById_NotExisting_Returns404()
        {

            var getresponse = await _client.GetAsync($"/api/order/{Guid.NewGuid()}");
            getresponse.Should().Be(HttpStatusCode.NotFound);

        }


        [Fact]
        public async Task GetOrderById_WithInvalidGuid_Returns404()
        {
            var getresponse = await _client.GetAsync($"/api/order/not-a-guid");
            getresponse.Should().Be(HttpStatusCode.NotFound);
        }


    }
}
