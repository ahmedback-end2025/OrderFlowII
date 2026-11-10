using FluentAssertions;
using OrderFlowII.IntegrationTests.Contracts.Responses;
using System.Net;
using System.Net.Http.Json;

namespace OrderFlowII.IntegrationTests
{
    [Collection("Api")]
    public class CancelOrderEndPointTests : IntegrationTestBase
    {
        public CancelOrderEndPointTests(SqlServerContainerFixture fixture) : base(fixture)
        {



        }


        [Fact]
        public async Task CancelOrder_PendingOrder_Returns204AndStatusIsCanceled()
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
            var id = await response.Content.ReadFromJsonAsync<Guid>();


            var cancelResponse = await _client.PutAsync($"/api/order/{id}/cancel", null);
            cancelResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
            // var cancelResponse = await _client.PutAsJsonAsync($"/api/order/{id}/cancel",id);

            //  var getResponse = await _client.GetFromJsonAsync($"/api/order/{id}");
            var getResponse = await _client.GetAsync($"/api/order/{id}");

            var order = await getResponse.Content.ReadFromJsonAsync<OrderResponse>();
            order.Should().NotBeNull();
            // order.Should().NotBeEmpty();
            order.Status.Should().Be("Canceled");


        }

        [Fact]
        public async Task CancelOrder_NotExisting_Returns400()
        {
            var cancelResponse = await _client.PutAsync($"/api/order/{Guid.NewGuid()}/cancel", null);
            cancelResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CancelOrder_WhenAlreadyCanceled_Returns400()
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
            var id = await response.Content.ReadFromJsonAsync<Guid>();
            id.Should().NotBeEmpty();

            var cancelResponse = await _client.PutAsync($"/api/order/{id}/cancel", null);
            cancelResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);


            cancelResponse = await _client.PutAsync($"/api/order/{id}/cancel", null);
            cancelResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);


        }


        [Fact]
        public async Task CancelOrder_CompletedOrder_Returns400AndStatusUnchanged()
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

            var cancelResponse = await _client.PutAsync($"/api/order/{id}/cancel", null);
            cancelResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);


            //Check Status UnChanged
            var getResponse = await _client.GetAsync($"/api/order/{id}");

            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var order = await getResponse.Content.ReadFromJsonAsync<OrderResponse>();

            order.Should().NotBeNull();
            order.Status.Should().Be("Completed");

        }



    }
}
