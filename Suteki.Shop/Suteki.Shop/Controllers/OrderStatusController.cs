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
		readonly IEmailService emailService;

		public OrderStatusController(IRepository<Order> orderRepository, IUserService userService, IEmailService emailService)
		{
			this.orderRepository = orderRepository;
			this.emailService = emailService;
			this.userService = userService;
		}

		[UnitOfWork]
		public ActionResult Dispatch(int id)
		{
			var order = orderRepository.GetById(id);

			if (order.IsCreated)
			{
				order.OrderStatus = OrderStatus.Dispatched;
				order.DispatchedDate = DateTime.Now;
				order.User = userService.CurrentUser;

				emailService.SendDispatchNotification(order);
			}

			return this.RedirectToAction<OrderController>(c => c.Item(order.Id));
		}

		[UnitOfWork]
		public ActionResult Reject(int id)
		{
			var order = orderRepository.GetById(id);

			if (order.IsCreated)
			{
				order.OrderStatus = OrderStatus.Rejected;
				order.User = userService.CurrentUser;
			}

			return this.RedirectToAction<OrderController>(c => c.Item(order.Id));
		}

		[UnitOfWork]
		public ActionResult UndoStatus(int id)
		{
			var order = orderRepository.GetById(id);

			if (order.IsDispatched || order.IsRejected)
			{
				order.OrderStatus = OrderStatus.Created;
				order.User = null;
			}

			return this.RedirectToAction<OrderController>(c => c.Item(order.Id));
		}
	}
}