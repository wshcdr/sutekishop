using System;
using System.Web.Mvc;
using MvcContrib;
using Suteki.Common.Filters;
using Suteki.Common.Repositories;
using Suteki.Shop.Filters;
using Suteki.Shop.Services;

namespace Suteki.Shop.Controllers
{
	[AdministratorsOnly]
	public class OrderStatusController : ControllerBase
	{
		readonly IRepository<Order> orderRepository;
		readonly IUserService userService;

		public OrderStatusController(IRepository<Order> orderRepository, IUserService userService)
		{
			this.orderRepository = orderRepository;
			this.userService = userService;
		}

		[UnitOfWork]
		public ActionResult Dispatch(int id)
		{
			var order = orderRepository.GetById(id);

			if (order.IsCreated)
			{
                order.Dispatch(userService.CurrentUser);
			}

			return this.RedirectToAction<OrderController>(c => c.Item(order.Id));
		}

		[UnitOfWork]
		public ActionResult Reject(int id)
		{
			var order = orderRepository.GetById(id);

			if (order.IsCreated)
			{
                order.Reject(userService.CurrentUser);
			}

			return this.RedirectToAction<OrderController>(c => c.Item(order.Id));
		}

		[UnitOfWork]
		public ActionResult UndoStatus(int id)
		{
			var order = orderRepository.GetById(id);

			if (order.IsDispatched || order.IsRejected)
			{
				order.ResetStatus();
			}

			return this.RedirectToAction<OrderController>(c => c.Item(order.Id));
		}
	}
}