using System;
using NUnit.Framework;
using Suteki.Common.Models;

// ReSharper disable InconsistentNaming

namespace Suteki.Shop.Tests.Models
{
    [TestFixture]
    public class OrderTests
    {
        [Test]
        public void UserAsString_should_return_user_email()
        {
            var order = new Order
            {
                User = new User
                {
                    Email = "mike@mike.com"
                }
            };

            order.UserAsString.ShouldEqual("mike@mike.com");
        }

        [Test]
        public void Total_should_return_sum_of_order_line_totals()
        {
            var price1 = new Money(3.40M);
            var price2 = new Money(6.23M);
            var price3 = new Money(10.44M);

            var order = new Order();
            order.AddLine("line1", 2, price1);
            order.AddLine("line2", 1, price2);
            order.AddLine("line3", 3, price3);

            var expectedTotal = (2*price1) + (1*price2) + (3*price3);

            order.Total.ShouldEqual(expectedTotal);
        }

        [Test]
        public void TotalWithPostage_should_return_postageResult_postage()
        {
            var postageCost = new Money(5.66M);
            var itemPrice = new Money(101.43M);

            var order = new Order
            {
                Postage = PostageResult.WithPrice(postageCost)
            };
            order.AddLine("line1", 1, itemPrice);

            var expectedTotalWithPostage = postageCost + itemPrice;

            order.TotalWithPostage.ShouldEqual(expectedTotalWithPostage.ToStringWithSymbol());
        }

        [Test]
        public void PostageDescription_should_return_Postage_Description()
        {
            var order = new Order
            {
                Postage = new PostageResult {Description = "some postage description"}
            };

            order.PostageDescription.ShouldEqual("some postage description");
        }

        public static Order Create350GramOrder()
        {
            var order = new Order
            {
                Basket = BasketTests.Create350GramBasket(),
                UseCardHolderContact = true,
                CardContact = new Contact { Country = new Country
                    {
                       PostZone = new PostZone { Multiplier = 2.5M, FlatRate = new Money(10.00M), AskIfMaxWeight = false }
                    } },
                Email = "mike@mike.com",
                CreatedDate = new DateTime(2008, 10, 18),
                OrderStatus = new OrderStatus { Name = "Dispatched" }
            };
            return order;
        }

        public static Order Create450GramOrder()
        {
            var order = Create350GramOrder();

            // add one more item to make max weight band (weight now 450)
            order.Basket.BasketItems.Add(new BasketItem
            {
                Quantity = 1,
                Size = new Size
                {
                    Product = new Product { Weight = 100, Price = new Money(0M) }
                }
            });
            return order;
        }
    }
}
// ReSharper restore InconsistentNaming
