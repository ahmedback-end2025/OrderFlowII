using Application.Feature.Orders.Commands.CancelOrder;
using Application.Feature.Orders.Commands.CompleteOrder;
using Application.Feature.Orders.Commands.CreateOrder;
using Application.Feature.Orders.Queries.GetAllOrders;
using Application.Feature.Orders.Queries.GetOrderById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace OrderFlowII.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ISender sender;

        public OrderController(ISender sender)
        {
            this.sender = sender;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command, CancellationToken cancellationToken)
        {
            var orderId = await sender.Send(command, cancellationToken);


            return CreatedAtAction(
               actionName: nameof(GetOrderById),
               routeValues: new { id = orderId },
               value: orderId);

            //return Created($"/api/Order/{orderId}", new { id = orderId });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetOrderById(Guid id, CancellationToken cancellationToken)
        {
            var order = await sender.Send(new GetOrderByIdQuery(id), cancellationToken);

            if (order == null)
            {
                return NotFound(new { message = $"Order with Id '{id}' was not found." });
            }

            return Ok(order);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders([FromQuery] int PageNumber = 1, [FromQuery] int PageSize = 10, CancellationToken cancellationToken = default)
        {
            var query = new GetAllOrdersQuery(PageNumber, PageSize);
            var orders = await sender.Send(query, cancellationToken);

            return Ok(orders);
        }



        [HttpPut("{id:guid}/cancel")]
        public async Task<IActionResult> CancelOrder(Guid id, CancellationToken cancellationToken)
        {
            var isSuccess = await sender.Send(new CancelOrderCommand(id), cancellationToken);


            if (!isSuccess)
            {
                return BadRequest(new { message = "Order not found or cannot be cancelled in its current state." });
            }


            return NoContent();
        }

        [HttpPut("{id:guid}/complete")]
        public async Task<IActionResult> CompleteOrder(
      Guid id,
      CancellationToken cancellationToken)
        {
            await sender.Send(
                new CompleteOrderCommand(id),
                cancellationToken);

            return NoContent();
        }


    }
}
