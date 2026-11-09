using FluentAssertions;
using OrderFlowII.IntegrationTests.Contracts.Responses;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlowII.IntegrationTests
{
    [Collection("Api")]
    public class CreateOrderEndPointTests
    {
        private HttpClient _client = null;
        public CreateOrderEndPointTests(SqlServerContainerFixture fixture)
        {
            _client = fixture.client;
        }

        public static object ValidOrder() => new
        {
            customername = "Ahmed",
            items = new[]
           {
               new { name = "Laptop", price = 25000m, Quantity = 1 }
            }
        };



        [Fact]
        public async Task CreateOrder_WhenValidOrder_Return201()
        {


            var response = await _client.PostAsJsonAsync("/api/order", ValidOrder());

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var id = await response.Content.ReadFromJsonAsync<Guid>();
            id.Should().NotBe(Guid.Empty);

        }

        [Fact]
        public async Task CreateOrder_whenValidOrder_WithPendingStatusAndItems()
        {
            var response = await _client.PostAsJsonAsync("/api/order", ValidOrder());
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var id = await response.Content.ReadFromJsonAsync<Guid>();
            id.Should().NotBe(Guid.Empty);


            var getresponse = await _client.GetAsync($"/api/order/{id}");
            getresponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var order = await getresponse.Content.ReadFromJsonAsync<OrderResponse>();


            Assert.Equal("Pending", order.Status);
            Assert.Equal(1, order.Items.Count);
            Assert.Contains(order.Items,
                i => i.Name == "Mouse" && i.Price == 300m && i.Quantity == 2);



        }


        [Fact]
        public async Task CreateOrder_CalculatesTotalPrice_FromAllItems()
        {
            var request = new
            {
                name = "Ahmed",
                Items = new[]
                {
                      new OrderItemResponse("Keyboard", 100m, 2),
                       new OrderItemResponse("Cable", 50m, 1)
                }
            };

            var response = await _client.PostAsJsonAsync("/api/order", request);
            response.StatusCode.Should().Be(HttpStatusCode.Created);


            var id = await response.Content.ReadFromJsonAsync<Guid>();
            id.Should().NotBe(Guid.Empty);


            var getresponse = await _client.GetAsync($"/api/order/{id}");
            getresponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var order = await getresponse.Content.ReadFromJsonAsync<OrderResponse>();

            Assert.Equal(250m, order!.TotalPrice);
        }



        public static TheoryData<string, OrderRequest> InvalidOrders() => new()
{
    {
        "Customer name is empty",
        new OrderRequest(
            "",
            new()
            {
                new OrderItemResponse("Laptop", 25000m, 1)
            }
        )
    },

    {
        "Customer name exceeds 200 characters",
        new OrderRequest(
            new string('A', 201),
            new()
            {
                new OrderItemResponse("Laptop", 25000m, 1)
            }
        )
    },

    {
        "Items is empty",
        new OrderRequest(
            "Ahmed",
            new()
        )
    },

    {
        "Item name is empty",
        new OrderRequest(
            "Ahmed",
            new()
            {
                new OrderItemResponse("", 25000m, 1)
            }
        )
    },

    {
        "Price is zero",
        new OrderRequest(
            "Ahmed",
            new()
            {
                new OrderItemResponse("Laptop", 0m, 1)
            }
        )
    },

    {
        "Price is negative",
        new OrderRequest(
            "Ahmed",
            new()
            {
                new OrderItemResponse("Laptop", -100m, 1)
            }
        )
    },

    {
        "Quantity is zero",
        new OrderRequest(
            "Ahmed",
            new()
            {
                new OrderItemResponse("Laptop", 25000m, 0)
            }
        )
    },

    {
        "Quantity is negative",
        new OrderRequest(
            "Ahmed",
            new()
            {
                new OrderItemResponse("Laptop", 25000m, -1)
            }
        )
    }
};



        [Theory]
        [MemberData(nameof(InvalidOrders))]
        public async Task CreateOrder_WithInvalidData_Returns400(string scanrioName , OrderRequest request)
        {
            var response = await _client.PostAsJsonAsync("/api/order", request);
           


            Assert.True(
           response.StatusCode == HttpStatusCode.BadRequest,
           $"Scenario '{scanrioName}' returned {(int)response.StatusCode}");

        }


        [Fact]
        public async Task CreateOrder_WithMalformedJson_Returns400()
        {
            var content = new StringContent("{\"CustomerName\" : ", Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/order", content);

            Assert.Equal( HttpStatusCode.BadRequest, response.StatusCode);
        }



    }
}
