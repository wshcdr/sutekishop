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
                Postage = PostageResult.WithPrice(postageCost, "postage desc")
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
    }
}
// ReSharper restore InconsistentNaming
