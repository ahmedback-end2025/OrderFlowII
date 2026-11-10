using FluentAssertions;
using OrderFlowII.IntegrationTests.Contracts.Responses;
using System.Net;
using System.Net.Http.Json;


namespace OrderFlowII.IntegrationTests
{
    [Collection("Api")]
    public class GetAllOrdersEndPointTests : IntegrationTestBase
    {
        public GetAllOrdersEndPointTests(SqlServerContainerFixture fixture) : base(fixture)
        {

        }

        [Fact]
        public async Task GetAllOrders_WhenDatabaseIsEmpty_ReturnsEmptyList()
        {
            var response = await _client.GetFromJsonAsync<List<OrderResponse>>("/api/order");

            response.Should().NotBeNull();
            response.Should().BeEmpty();
        }
        [Fact]
        public async Task GetAllOrders_ReturnsNewestFirst()
        {
            var request1 = new
            {
                customername = "Ahmed",
                Items = new[]
                {
                    new {name ="Mouse" , price = 250M ,Quantity = 2 },
                    //new {name ="Keyboard" , price = 350M ,Quantity = 2 }
                }
            };
            var response1 = await _client.PostAsJsonAsync("/api/order", request1);
            response1.StatusCode.Should().Be(HttpStatusCode.Created);
            var id1 = await response1.Content.ReadFromJsonAsync<Guid>();
            await Task.Delay(100);
            var request2 = new
            {
                customername = "Emam",
                Items = new[]
                {
                    new {name ="Charger" , price = 550M ,Quantity = 2 },
                    //new {name ="Keyboard" , price = 350M ,Quantity = 2 }
                }
            };
            var response2 = await _client.PostAsJsonAsync("/api/order", request2);
            response2.StatusCode.Should().Be(HttpStatusCode.Created);
            var id2 = await response2.Content.ReadFromJsonAsync<Guid>();
            await Task.Delay(150);
            var request3 = new
            {
                customername = "Saeed",
                Items = new[]
                {
                    new {name ="Monitor" , price = 2510M ,Quantity = 1 },
                    //new {name ="Keyboard" , price = 350M ,Quantity = 2 }
                }
            };
            var response3 = await _client.PostAsJsonAsync("/api/order", request3);
            response3.StatusCode.Should().Be(HttpStatusCode.Created);
            var id3 = await response3.Content.ReadFromJsonAsync<Guid>();



            //var response = await _client.PostAsJsonAsync("/api/order", request);
            //response.StatusCode.Should().Be(HttpStatusCode.Created);

            var getresponse = await _client.GetFromJsonAsync<List<OrderResponse>>("/api/order");
            getresponse.Should().NotBeEmpty();

            getresponse.Select(o => o.Id).Should().Equal(id3, id2, id1);

        }

        [Theory]
        [InlineData(1, 2, 2)]
        [InlineData(2, 2, 2)]
        [InlineData(3, 2, 1)]
        [InlineData(4, 2, 0)]
        [InlineData(1, 10, 5)]
        public async Task GetAllOrders_WithPaging_ReturnsExpectedCount(int pageNumber, int pageSize, int expectedCount)
        {

            var request = new
            {
                customername = "Ahmed",
                Items = new[]
                {
                    new {name ="Mouse" , price = 250M ,Quantity = 2 },
                    //new {name ="Keyboard" , price = 350M ,Quantity = 2 }
                }

            };

            for (int i = 0; i < 5; i++)
            {
                var response = await _client.PostAsJsonAsync("/api/order", request);
                response.StatusCode.Should().Be(HttpStatusCode.Created);

            }

            var orders =
                await _client.GetFromJsonAsync<List<OrderResponse>>($"/api/order?PageNumber={pageNumber}&PageSize={pageSize}");
            orders.Should().NotBeNull();
            orders.Count.Should().Be(expectedCount);
        }


        [Fact]
        public async Task GetAllOrders_WithoutPaging_UsesDefaultPageSizeOf10()
        {
            var request = new
            {
                customername = "Ahmed",
                Items = new[]
                {
                    new {name ="Mouse" , price = 250M ,Quantity = 2 },
                    //new {name ="Keyboard" , price = 350M ,Quantity = 2 }
                }

            };

            for (int i = 0; i < 11; i++)
            {
                var response = await _client.PostAsJsonAsync("/api/order", request);
                response.StatusCode.Should().Be(HttpStatusCode.Created);

            }

            var orders = await Client.GetFromJsonAsync<List<OrderResponse>>("/api/order");

            Assert.Equal(10, orders!.Count);
        }
        [Theory]
        [InlineData(0, 10)]
        [InlineData(-1, 10)]
        [InlineData(1, 0)]
        [InlineData(1, -5)]
        [InlineData(1, 101)]
        public async Task GetAllOrders_WithInvalidPaging_Returns400(  //Execption Because No Validation On PageNumber & PageSize
        int pageNumber, int pageSize)
        {
            var response = await Client.GetAsync(
                $"/api/order?PageNumber={pageNumber}&PageSize={pageSize}");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            //Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}

