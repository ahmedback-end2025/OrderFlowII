using Application.Common.Interfaces;
using Application.Feature.Orders.Commands.CancelOrder;
using Application.Feature.Orders.Commands.CompleteOrder;
using Application.Feature.Orders.Commands.CreateOrder;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlowII.Tests
{
    public class ApplicationTesting
    {
        #region CreateOrderCommandValidator

        [Fact]
        public void Validate_WhenCommandIvalid_ReturnNoErrors()
        {
            var command = new CreateOrderCommand("Ahmed", new List<OrderItemsDto> { new OrderItemsDto("Mouse", 100M, 3) });


            var validator = new CreateOrderCommandValidator();


            var result = validator.Validate(command);



            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);

        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void Validate_WhenCustomerNameIsEmptyOrWhiteSpace_ReturnError(string customerName)
        {
            var command = new CreateOrderCommand(customerName, new List<OrderItemsDto> { new OrderItemsDto("Mouse", 100M, 3) });


            var validator = new CreateOrderCommandValidator();


            var result = validator.Validate(command);


            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == "CustomerName");

        }

        [Fact]
        public void Validate_WhenCustomerNameGreaterThan200_ReturnError()
        {
            string customerName = new string('A', 201);
            var command = new CreateOrderCommand(customerName, new List<OrderItemsDto> { new OrderItemsDto("Mouse", 100M, 3) });


            var validator = new CreateOrderCommandValidator();


            var result = validator.Validate(command);


            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == "CustomerName");
        }

        [Fact]
        public void Validate_WhenItemsAreEmpty_ReturnsError()
        {
            var command = new CreateOrderCommand("Ahmed", new List<OrderItemsDto>());
            var validator = new CreateOrderCommandValidator();


            var result = validator.Validate(command);


            Assert.False(result.IsValid);
            Assert.Contains(
        result.Errors,
        error => error.PropertyName == "Items");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void Validate_WhenItemNameIsEmptyOrWhiteSpace_ReturnError(string itemname)
        {

            var command = new CreateOrderCommand("Ahmed", new List<OrderItemsDto> { new OrderItemsDto(itemname, 100M, 3) });


            var validator = new CreateOrderCommandValidator();


            var result = validator.Validate(command);


            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == "Items[0].name");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WhenItemPriceIsNegativeOrZero_ReturnError(decimal itemprice)
        {

            var command = new CreateOrderCommand("Ahmed", new List<OrderItemsDto> { new OrderItemsDto("Mouse", itemprice, 3) });


            var validator = new CreateOrderCommandValidator();


            var result = validator.Validate(command);


            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == "Items[0].price");
        }
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WhenItemQuantityIsNegativeOrZero_ReturnError(int itemQuantity)
        {

            var command = new CreateOrderCommand("Ahmed", new List<OrderItemsDto> { new OrderItemsDto("Mouse", 100M, itemQuantity) });


            var validator = new CreateOrderCommandValidator();


            var result = validator.Validate(command);


            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == "Items[0].Quantity");
        }




        #endregion



        #region CreateOrderHandler

        [Fact]
        public async Task Handle_WhenCommandIsValid_CreatesOrderAndReturnsId()
        {
            // Arrange
            var orders = new List<Order>();

            var mockOrders = new Mock<DbSet<Order>>();

            mockOrders
    .Setup(x => x.Add(It.IsAny<Order>()))
    .Callback<Order>(order =>
    {
        orders.Add(order);
    });



            var mockContext = new Mock<IApplicationDbContext>();

            mockContext
                .Setup(x => x.Orders)
                .Returns(mockOrders.Object);

            mockContext
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var mockLogger = new Mock<ILogger<CreateOrderHandler>>();

            var handler = new CreateOrderHandler(
                mockContext.Object,
                mockLogger.Object);

            var command = new CreateOrderCommand(
                "Ahmed",
                new List<OrderItemsDto>
                {
                new OrderItemsDto("Mouse", 100M, 2)
                });

            // Act
            var result = await handler.Handle(
                command,
                CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);

            var order = Assert.Single(orders);

            Assert.Equal(result, order.Id);
            Assert.Equal("Ahmed", order.CustomerName);
            Assert.Equal(200M, order.TotalPrice);

            mockContext.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);


        }


        [Fact]
        public async Task Handle_WhenCreateMultipleOrders_Returns()
        {
            //Arrange 

            var mockcontext = new Mock<IApplicationDbContext>();
            var mockOrder = new Mock<DbSet<Order>>();
            var mockIlogger = new Mock<ILogger<CreateOrderHandler>>();


            var Orders = new List<Order>();

            mockOrder.
                Setup(x => x.Add(It.IsAny<Order>())).Callback<Order>(Orders.Add);
            mockcontext.Setup(x => x.Orders).Returns(mockOrder.Object);

            mockcontext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            //Act 

            var command = new CreateOrderCommand("Ahmed", new List<OrderItemsDto>
            { new OrderItemsDto("Mouse" , 100M , 3),
             new OrderItemsDto("Keyboard" , 200M , 3),
             new OrderItemsDto("Mouse" , 500M , 2)
            });

            var handler = new CreateOrderHandler(mockcontext.Object, mockIlogger.Object);

            var result = await handler.Handle(command, CancellationToken.None);


            //Assert 
            Assert.NotEqual(Guid.Empty, result);

            var order = Assert.Single<Order>(Orders);

            Assert.Equal(3, order.Items.Count);

            Assert.Equal(1900, order.TotalPrice);

            mockOrder.Verify(
                          x => x.Add(It.IsAny<Order>()),
                           Times.Once);

            mockcontext.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);





        }
        #endregion


        #region CompleteOrderHandler

        [Fact]
        public async Task Handle_WhenOrderExists_CompleteOrder()
        {
            //Arrange 

            var mockcontext = new Mock<IApplicationDbContext>();
            var mockOrder = new Mock<DbSet<Order>>();
            var mockIlogger = new Mock<ILogger<CompleteOrderHandler>>();




            var order = new Order("Ahmed");



            mockcontext.Setup(x => x.Orders).Returns(mockOrder.Object);

            mockcontext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            mockOrder.Setup(x => x.FindAsync(It.IsAny<Object[]>(), It.IsAny<CancellationToken>())).
                Returns(new ValueTask<Order?>(order));



            var command = new CompleteOrderCommand(order.Id);

            var handler = new CompleteOrderHandler(mockcontext.Object, mockIlogger.Object);


            /*var result =*/
            await handler.Handle(command, CancellationToken.None);


            //  var order = Assert.Single(Orders);

            Assert.Equal(OrderStatus.Completed, order.orderStatus);

            Assert.NotEqual(Guid.Empty, order.Id);

            mockcontext.Verify(
    x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
    Times.Once);

            mockOrder.Verify(x => x.FindAsync(It.IsAny<Object[]>(), It.IsAny<CancellationToken>()), Times.Once);


        }


        [Fact]
        public async Task Handle_WhenOrderDoesNotExist_ThrowsKeyNotFoundException()
        {
            var mockcontext = new Mock<IApplicationDbContext>();
            var mockOrder = new Mock<DbSet<Order>>();
            var mockIlogger = new Mock<ILogger<CompleteOrderHandler>>();




            //        var order = new Order("Ahmed");

            Random random = new Random();

            mockcontext.Setup(x => x.Orders).Returns(mockOrder.Object);

            mockcontext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            mockOrder.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>())).
                Returns(new ValueTask<Order?>((Order?)null));



            var command = new CompleteOrderCommand(Guid.NewGuid());

            var handler = new CompleteOrderHandler(mockcontext.Object, mockIlogger.Object);





            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await handler.Handle(command, CancellationToken.None));




            mockcontext.Verify(
    x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
    Times.Never);


            mockOrder.Verify(
                x => x.FindAsync(
                    It.IsAny<object[]>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }


        #endregion




    }





    public class CancelOrderhandlerTest : SqliteTestBase
    {

        private readonly CancelOrderHandler _handler;

        public CancelOrderhandlerTest()
        {
            var logger = new Mock<ILogger<CancelOrderHandler>>();
            _handler = new CancelOrderHandler(Context, logger.Object);
        }


        //First Case Order Is Null [ Non Exist ] .
        [Fact]
        public async Task Handel_WhenOrderIsNull_returnFalse()
        {
            var command = new CancelOrderCommand(Guid.NewGuid());

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.False(result);


        }

        //Second Case Order Is Exist But Can Not Be Canceled Because It's Completed Or Already Canceled

        [Theory]
        [InlineData(OrderStatus.Completed)]
        [InlineData(OrderStatus.Canceled)]
        public async Task Handel_WhenOrderStatusIsCompletedOrOrAlreadyCanceled_returnFalse(OrderStatus orderStatus)
        {
            var order = new Order("Ahmed");
            order.AddItem("Mouse", 100M, 3);
            order.orderStatus = orderStatus;
            Context.Add(order);
            await Context.SaveChangesAsync();
            Context.ChangeTracker.Clear();
            var command = new CancelOrderCommand(order.Id);
            var result = await _handler.Handle(command, CancellationToken.None);


            Context.ChangeTracker.Clear();
            Assert.False(result);

            var save = await Context.Orders.FindAsync(order.Id);

            Assert.Equal(orderStatus, save!.orderStatus);
        }
        [Fact]
        public async Task Handle_WhenOrderCanBeCancelled_CancelsAndPersists()
        {
            // Arrange
            var order = new Order("Ahmed");
            order.AddItem("Mouse", 100M, 2);
            Context.Orders.Add(order);
            await Context.SaveChangesAsync();
            Context.ChangeTracker.Clear();

            // Act
            var result = await _handler.Handle(new CancelOrderCommand(order.Id), CancellationToken.None);

            // Assert
            Assert.True(result);

            Context.ChangeTracker.Clear();
            var saved = await Context.Orders.FindAsync(order.Id);
            Assert.Equal(OrderStatus.Canceled, saved!.orderStatus);
        }

    }

}
