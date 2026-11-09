using Application.Feature.Orders.Commands.CreateOrder;
using Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlowII.IntegrationTests
{
    [CollectionDefinition("Api")]
    public class ApiCollection : ICollectionFixture<SqlServerContainerFixture> { }

    [Collection("Api")]
    public class OrderApiTest 
    {
       
        private HttpClient _client = null;


        public OrderApiTest(SqlServerContainerFixture fixture)
        {
            //SqlServerContainerFixture ->connectionstring ->factory->client
            //_factory = new CustomWebApplicationFactory(fixture.ConnectionString);
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
            //CreateOrderCommand request = new CreateOrderCommand
            //(
            // "Ahmed",
            //   new List<OrderItemsDto>
            //    {
            //        new Application.Feature.Orders.Commands.CreateOrder.OrderItemsDto ("Laptop", 25000m,1)
            //    }
            //);

            //var request2 = new
            //{
            //    customername = "Ahmed",
            //    Items = new[] { new { name = "Laptop", price = 25000m, Quantity = 1 } }
            //};

            var response = await _client.PostAsJsonAsync("/api/order", ValidOrder());

            response.Should().Be(HttpStatusCode.Created);

            var id = response.Content.ReadFromJsonAsync<Guid>();
            id.Should().NotBe(Guid.Empty);

        }




      
    }
}
