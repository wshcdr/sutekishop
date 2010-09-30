using System;
using NUnit.Framework;
using Rhino.Mocks;
using Suteki.Common.Events;
using Suteki.Common.Repositories;
using Suteki.Shop.Controllers;
using Suteki.Shop.Services;

namespace Suteki.Shop.Tests.Controllers
{
	[TestFixture]
	public class OrderStatusControllerTests
	{
		OrderStatusController controller;
		IUserService userService;
		IRepository<Order> repository;

        [SetUp]
		public void Setup()
		{
			userService = MockRepository.GenerateStub<IUserService>();
			repository = MockRepository.GenerateStub<IRepository<Order>>();
			controller = new OrderStatusController(repository, userService);

			userService.Expect(x => x.CurrentUser).Return(new User { Id = 4 });
		}


		[Test]
		public void Dispatch_ShouldChangeOrderStatusAndDispatchedDate()
		{
            using (DomainEvent.TurnOff())
            {
                const int orderId = 44;
                var order = new Order
                {
                    Id = orderId,
                    OrderStatus = OrderStatus.Created
                };

                repository.Expect(or => or.GetById(orderId)).Return(order);

                controller.Dispatch(orderId);

                order.IsDispatched.ShouldBeTrue();
            }
		}

	    [Test]
	    public void Reject_should_change_order_status_to_rejected()
	    {
	        const int orderId = 32;
	        var order = new Order {OrderStatus = OrderStatus.Created};
	        repository.Stub(r => r.GetById(orderId)).Return(order);
	        controller.Reject(orderId);
            order.IsRejected.ShouldBeTrue();
	    }

		[Test]
		public void UndoStatus_ShouldChangeOrderStatusToCreated()
		{
			const int orderId = 44;
			var order = new Order
			{
				Id = orderId,
				OrderStatus = OrderStatus.Dispatched,
				DispatchedDate = DateTime.Now
			};

			repository.Expect(or => or.GetById(orderId)).Return(order);

			controller.UndoStatus(orderId);
			order.IsCreated.ShouldBeTrue();
		}
	}
}