using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Xunit.Sdk;

namespace OrderFlowII.Tests
{
    public class OrderTesting
    {
        [Fact]
        public void CompleteOrder_WhenStatusIsPending_ChangesStatusToCompleted()
        {
            //Arrange
            Order order = new Order("Samy");

            // order.AddItem("Mouse", 100M, 5);
            //Act
            order.CompleteOrder();
            //Assert
            Assert.True(order.orderStatus == OrderStatus.Completed);
        }



        [Fact]
        public void CompleteOrder_WhenStatusIsCanceled_ThrowsInvalidOperationException()
        {
            Order order = new Order("Semsem");
            order.orderStatus = OrderStatus.Canceled;



            Assert.Throws<InvalidOperationException>(() => order.CompleteOrder());

        }
        [Fact]
        public void CompleteOrder_WhenStatusIsCompleted_ThrowsInvalidOperationException()
        {
            Order order = new Order("Semsem");
            order.orderStatus = (OrderStatus)2;



            Assert.Throws<InvalidOperationException>(() => order.CompleteOrder());

        }

        [Fact]
        public void CancelOrder_whenStatusIsPending_ChangeStatusToCanceled()
        {
            Order order = new Order("saad");

            var result = order.CancelOrder();
            Assert.True(result);
            Assert.Equal(OrderStatus.Canceled, order.orderStatus);
        }

        [Fact]
        public void CancelOrder_WhenStatusIsCompleted_ReturnsFalseAndKeepsStatusCompleted()
        {
            // Arrange
            Order order = new Order("Semsem");
            order.orderStatus = OrderStatus.Completed;

            // Act
            bool result = order.CancelOrder();

            // Assert
            Assert.False(result);
            Assert.Equal(OrderStatus.Completed, order.orderStatus);
        }

        [Fact]
        public void CancelOrder_WhenStatusIsCompleted_ReturnsFalseAndKeepsStatusCanceled()
        {
            // Arrange
            Order order = new Order("Semsem");
            order.orderStatus = OrderStatus.Canceled;

            // Act
            bool result = order.CancelOrder();

            // Assert
            Assert.False(result);
            Assert.Equal(OrderStatus.Canceled, order.orderStatus);
        }

        [Fact]
        public void AddItem_WhenItemsCountZero_ItemCountOneAndTotalPriceIsChecked()
        {
            Order order = new Order("Saeed");

            order.AddItem("Mouse", 100M, 5);


            Assert.Equal(500, order.TotalPrice);
            Assert.Equal(1, order.Items.Count);
        }
        [Fact]
        public void AddItem_WhenMultipleItemsAreAdded_UpdatesTotalPriceAndItemCount()
        {
            Order order = new Order("Saeed");

            order.AddItem("Mouse", 100M, 5);
            order.AddItem("Keyboard", 200M, 2);


            Assert.Equal(900M, order.TotalPrice);
            Assert.Equal(2, order.Items.Count);
        }

        [Fact]
        public void CheckOrderNameValidation_WhenNameIsNull_ThrowArgumentException()
        {
           

            Assert.Throws<ArgumentException>(() => new Order(null));
        }
        //[Fact]
        //public void CheckOrderItemNameValidation_WhenNameIsWhiteSpace_ThrowArgumentException()
        //{
        //    Order order = new Order("Samer");

            

        //    Assert.Throws<ArgumentException>(() => order.AddItem(" ", 105.5M, 5));
        //}
        //[Fact]
        //public void CheckOrderItemPriceValidation_WhenPriceIsNegative_ThrowArgumentException()
        //{
        //    Order order = new Order("Samer");

            

        //    Assert.Throws<ArgumentException>(() => order.AddItem("Mouse", -105.5M, 5));
        //}
        //[Fact]
        //public void CheckOrderItemQuantityValidation_WhenQuantityIsNegativeOrZero_ThrowArgumentException()
        //{
        //    Order order = new Order("Samer");

            

        //    Assert.Throws<ArgumentException>(() => order.AddItem("Mouse", 105.5M, 0));
        //    Assert.Throws<ArgumentException>(() => order.AddItem("Mouse", 105.5M, -5));
        //}

        //[Theory]
        //[InlineData(0)]
        //[InlineData(-5)]
        //public void CheckOrderItemQuantityValidation_WhenQuantityIsNegativeOrZero_ThrowArgumentException2(int qaunatity)
        //{
        //    Order order = new Order("Samer");

            

        //    Assert.Throws<ArgumentException>(() => order.AddItem("Mouse", 105.5M, qaunatity));
            
        //}

        [Fact]
        public void Order_WhenCreated_InitializesDefaultValues()
        {
            Order order = new Order("Sema");

            Assert.NotEqual(Guid.Empty, order.Id);
            Assert.Equal(order.orderStatus, OrderStatus.Pending);
            Assert.Equal("Sema", order.CustomerName);
            Assert.Equal(0, order.TotalPrice);
            
        }




    }
}
