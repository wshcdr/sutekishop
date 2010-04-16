using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Suteki.Common.Repositories;
using Suteki.Common.TestHelpers;
using Suteki.Shop.Controllers;
using Suteki.Shop.Services;
using Suteki.Shop.ViewData;
using System.Collections.Generic;

namespace Suteki.Shop.Tests.Controllers
{
	[TestFixture]
	public class CheckoutControllerTester
	{
		CheckoutController controller;
		IRepository<Basket> basketRepository;
		IUserService userService;
	    IBasketService basketService;
		IPostageService postageService;
		IRepository<Country> countryRepository;
		IRepository<CardType> cardTypeRepository;
		IRepository<Order> orderRepository;
		IUnitOfWorkManager unitOfWorkManager;
		IEmailService emailService;
		IRepository<MailingListSubscription> subscriptionRepository;

		[SetUp]
		public void Setup()
		{
			basketRepository = MockRepository.GenerateStub<IRepository<Basket>>();
			unitOfWorkManager = MockRepository.GenerateStub<IUnitOfWorkManager>();

			userService = MockRepository.GenerateStub<IUserService>();
		    basketService = MockRepository.GenerateStub<IBasketService>();
			postageService = MockRepository.GenerateStub<IPostageService>();
			countryRepository = MockRepository.GenerateStub<IRepository<Country>>();
			cardTypeRepository = MockRepository.GenerateStub<IRepository<CardType>>();
			orderRepository = MockRepository.GenerateStub<IRepository<Order>>();
			subscriptionRepository = MockRepository.GenerateStub<IRepository<MailingListSubscription>>();
			emailService = MockRepository.GenerateStub<IEmailService>();

			var mocks = new MockRepository(); //TODO: No need to partial mock once email sending is fixed
			controller = new CheckoutController(
				basketRepository,
				userService,
				postageService,
				countryRepository,
				cardTypeRepository,
				orderRepository,
				unitOfWorkManager,
				emailService,
				subscriptionRepository,
                basketService
			);
			mocks.ReplayAll();
			userService.Expect(us => us.CurrentUser).Return(new User { Id = 4, Role = Role.Administrator });
		}

		[Test]
		public void Index_ShouldDisplayCheckoutForm() {
			const int basketId = 6;

			var basket = new Basket { Id = basketId };
			var countries = new List<Country> { new Country() }.AsQueryable();
			var cardTypes = new List<CardType> { new CardType() }.AsQueryable();

			// stubs
			basketRepository.Stub(br => br.GetById(basketId)).Return(basket);
			countryRepository.Stub(cr => cr.GetAll()).Return(countries);
			cardTypeRepository.Stub(ctr => ctr.GetAll()).Return(cardTypes);

			// exercise Checkout action
			controller.Index(basketId)
				.ReturnsViewResult()
				.WithModel<ShopViewData>()
				.AssertNotNull(vd => vd.Order)
				.AssertNotNull(vd => vd.Order.CardContact)
				.AssertNotNull(vd => vd.Order.DeliveryContact)
				.AssertNotNull(vd => vd.Order.Card)
				.AssertAreSame(basket, vd => vd.Order.Basket)
				.AssertNotNull(vd => vd.Countries)
				.AssertAreSame(cardTypes, vd => vd.CardTypes);
		}

		[Test]
		public void Index_should_load_order_from_tempdata()
		{
			var order = new Order();
			controller.TempData["order"] = order;

			controller.Index(4)
				.ReturnsViewResult()
				.WithModel<ShopViewData>()
				.AssertAreSame(order, vd => vd.Order);
		}

		[Test]
		public void IndexWithPost_ShouldCreateANewOrder() {
			var order = new Order { Id = 4 };

			//controller.Expect(x => x.EmailOrder(order));

			controller.Index(order)
				.ReturnsRedirectToRouteResult()
				.ToController("Checkout")
				.ToAction("Confirm")
				.WithRouteValue("id", "4");

//			emailService.AssertWasCalled(x => x.SendOrderConfirmation(order));
			orderRepository.AssertWasCalled(x => x.SaveOrUpdate(order));
//			unitOfWorkManager.AssertWasCalled(x => x.Commit());
		}

		[Test]
		public void IndexWithPost_ShouldRenderViewOnError()
		{
			controller.ModelState.AddModelError("foo", "bar");
			var order = new Order { Basket = new Basket { Id = 6} };
			controller.Index(order)
				.ReturnsViewResult()
				.WithModel<ShopViewData>()
				.AssertAreEqual(order, x => x.Order);
		}

		[Test]
		public void Confirm_DisplaysConfirm()
		{
			var order = new Order();
			orderRepository.Expect(x => x.GetById(5)).Return(order);
			controller.Confirm(5)
				.ReturnsViewResult()
				.WithModel<ShopViewData>()
				.AssertAreSame(order, x => x.Order);
		}

		[Test]
		public void ConfirmWithPost_UpdatesOrderStatus()
		{
			var order = new Order { Id = 5 };

			controller.Confirm(order)
				.ReturnsRedirectToRouteResult()
				.ToController("Order")
				.ToAction("Item")
				.WithRouteValue("id", "5");

			emailService.AssertWasCalled(x => x.SendOrderConfirmation(order));
		}

		[Test]
		public void ConfirmWithPost_CreatesMailingListSubscriptionForDeliveryContact()
		{
			var order = new Order { DeliveryContact = new Contact(), ContactMe = true, Email = "foo@bar.com"};
			MailingListSubscription subscription = null;

			subscriptionRepository.Expect(x => x.SaveOrUpdate(Arg<MailingListSubscription>.Is.Anything))
				.Do(new Action<MailingListSubscription>(x => subscription = x));

			controller.Confirm(order);

			subscription.ShouldNotBeNull();
			subscription.Contact.ShouldBeTheSameAs(order.DeliveryContact);
			subscription.Email.ShouldEqual("foo@bar.com");
		}

		[Test]
		public void ConfirmWithPost_CreatesMailingListSubscriptionForCardContact()
		{
			var order = new Order
			{
			    CardContact = new Contact(), 
                ContactMe = true, 
                Email = "foo@bar.com", 
                UseCardHolderContact = true
			};

			MailingListSubscription subscription = null;

			subscriptionRepository.Expect(x => x.SaveOrUpdate(Arg<MailingListSubscription>.Is.Anything))
				.Do(new Action<MailingListSubscription>(x => subscription = x));

			controller.Confirm(order);

			subscription.ShouldNotBeNull();
			subscription.Contact.ShouldBeTheSameAs(order.CardContact);
			subscription.Email.ShouldEqual("foo@bar.com");
		}
	}
}