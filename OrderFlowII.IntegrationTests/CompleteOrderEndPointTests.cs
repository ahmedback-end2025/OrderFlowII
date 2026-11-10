using FluentAssertions;
using OrderFlowII.IntegrationTests.Contracts.Responses;
using System.Net;
using System.Net.Http.Json;

namespace OrderFlowII.IntegrationTests
{
    [Collection("Api")]
    public class CompleteOrderEndPointTests : IntegrationTestBase
    {
        public CompleteOrderEndPointTests(SqlServerContainerFixture fixture) : base(fixture)
        {

        }

        [Fact]
        public async Task CompleteOrder_PendingOrder_Returns204AndStatusIsCompleted()
        {
            var request = new
            {
                CustomerName = "Ahmed",
                Items = new[]
              {
                    new { name = "Mouse", Price = 150M, Quantity = 2 }
                }
            };

            var response = await _client.PostAsJsonAsync("/api/order", request);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var id = await response.Content.ReadFromJsonAsync<Guid>();
            id.Should().NotBeEmpty();

            var completeResponse = await _client.PutAsync($"/api/order/{id}/complete", null);
            completeResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

            //Check Status Completed
            var getResponse = await _client.GetAsync($"/api/order/{id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var order = await getResponse.Content.ReadFromJsonAsync<OrderResponse>();

            order.Should().NotBeNull();
            order.Status.Should().Be("Completed");

        }

        [Fact]
        public async Task CompleteOrder_NotExisting_Returns404()
        {
            var completeResponse = await _client.PutAsync($"/api/order/{Guid.NewGuid()}/complete", null);
            completeResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CompleteOrder_AlreadyCompleted_Returns409()
        {
            var request = new
            {
                CustomerName = "Ahmed",
                Items = new[]
              {
                    new { name = "Mouse", Price = 150M, Quantity = 2 }
                }
            };

            var response = await _client.PostAsJsonAsync("/api/order", request);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var id = await response.Content.ReadFromJsonAsync<Guid>();
            id.Should().NotBeEmpty();

            var completeResponse = await _client.PutAsync($"/api/order/{id}/complete", null);
            completeResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

            completeResponse = await _client.PutAsync($"/api/order/{id}/complete", null);
            completeResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);


            //Check Status UnChanged
            var getResponse = await _client.GetAsync($"/api/order/{id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var order = await getResponse.Content.ReadFromJsonAsync<OrderResponse>();

            order.Should().NotBeNull();
            order.Status.Should().Be("Completed");

        }

        [Fact]
        public async Task CompleteOrder_CanceledOrder_Returns409()
        {
            var request = new
            {
                CustomerName = "Ahmed",
                Items = new[]
           {
                    new { name = "Mouse", Price = 150M, Quantity = 2 }
                }
            };

            var response = await _client.PostAsJsonAsync("/api/order", request);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var id = await response.Content.ReadFromJsonAsync<Guid>();
            id.Should().NotBeEmpty();

            var cancelResponse = await _client.PutAsync($"/api/order/{id}/cancel", null);
            cancelResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

            var completeResponse = await _client.PutAsync($"/api/order/{id}/complete", null);
            completeResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);






            //Check Status UnChanged
            var getResponse = await _client.GetAsync($"/api/order/{id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var order = await getResponse.Content.ReadFromJsonAsync<OrderResponse>();

            order.Should().NotBeNull();
            order.Status.Should().Be("Canceled");

        }

    }
}
