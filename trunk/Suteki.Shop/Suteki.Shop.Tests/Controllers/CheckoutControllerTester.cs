using System;
using NUnit.Framework;
using Rhino.Mocks;
using Suteki.Common.Repositories;
using Suteki.Common.TestHelpers;
using Suteki.Shop.Controllers;
using Suteki.Shop.Services;
using Suteki.Shop.ViewData;

// ReSharper disable InconsistentNaming
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
		IRepository<CardType> cardTypeRepository;
		FakeRepository<Order> orderRepository;
		IUnitOfWorkManager unitOfWorkManager;
		IEmailService emailService;
		IRepository<MailingListSubscription> subscriptionRepository;
	    IEncryptionService encryptionService;

		[SetUp]
		public void Setup()
		{
			basketRepository = MockRepository.GenerateStub<IRepository<Basket>>();
			unitOfWorkManager = MockRepository.GenerateStub<IUnitOfWorkManager>();

			userService = MockRepository.GenerateStub<IUserService>();
		    basketService = MockRepository.GenerateStub<IBasketService>();
			postageService = MockRepository.GenerateStub<IPostageService>();
			cardTypeRepository = MockRepository.GenerateStub<IRepository<CardType>>();
			orderRepository = new FakeRepository<Order>();
			subscriptionRepository = MockRepository.GenerateStub<IRepository<MailingListSubscription>>();
			emailService = MockRepository.GenerateStub<IEmailService>();
		    encryptionService = MockRepository.GenerateStub<IEncryptionService>();

			var mocks = new MockRepository(); //TODO: No need to partial mock once email sending is fixed
			controller = new CheckoutController(
				basketRepository,
				userService,
				postageService,
				cardTypeRepository,
				orderRepository,
				unitOfWorkManager,
				emailService,
				subscriptionRepository,
                basketService,
                encryptionService
			);
			mocks.ReplayAll();
			userService.Expect(us => us.CurrentUser).Return(new User { Id = 4, Role = Role.Administrator });
		}

		[Test]
		public void Index_ShouldDisplayCheckoutForm() {
			const int basketId = 6;
		    var basket = new Basket
		    {
                Country = new Country(),
                Id = basketId
		    };
		    basketRepository.Stub(b => b.GetById(basketId)).Return(basket);

		    var visa = new CardType();
			cardTypeRepository.Stub(ctr => ctr.GetById(CardType.VisaDeltaElectronId))
                .Return(visa);

			// exercise Checkout action
		    controller.Index(basketId)
		        .ReturnsViewResult()
		        .WithModel<CheckoutViewData>()
                .AssertAreSame(basket.Country, vd => vd.CardContactCountry)
                .AssertAreSame(basket.Country, vd => vd.DeliveryContactCountry)
		        .AssertAreSame(visa, vd => vd.CardCardType)
		        .AssertAreEqual(basketId, vd => vd.BasketId)
                .AssertIsTrue(vd => vd.UseCardholderContact);
		}

		[Test]
		public void Index_should_load_order_from_tempdata()
		{
		    var checkoutViewData = new CheckoutViewData();
            controller.TempData["CheckoutViewData"] = checkoutViewData;

			controller.Index(4)
				.ReturnsViewResult()
                .WithModel<CheckoutViewData>()
                .AssertAreSame(checkoutViewData, vd => vd);
		}

		[Test]
		public void IndexWithPost_ShouldCreateANewOrder()
		{
		    var checkoutViewData = GetCheckoutViewData();
		    basketRepository.Stub(r => r.GetById(7)).Return(
		        new Basket { Id = 7 }
		        );

		    Order order = null;
		    orderRepository.SaveOrUpdateDelegate = entity =>
		    {
		        order = entity;
		        order.Id = 4;
		    };

		    controller.Index(checkoutViewData)
				.ReturnsRedirectToRouteResult()
				.ToController("Checkout")
				.ToAction("Confirm")
				.WithRouteValue("id", "4");

            VerifyOrderMatchesCheckoutViewData(order, checkoutViewData);
		}

	    private static CheckoutViewData GetCheckoutViewData()
	    {
	        return new CheckoutViewData
	        {
                OrderId = 0,
                BasketId = 7,
                
                CardContactFirstName = "Jon",
                CardContactLastName = "Anderson",
                CardContactAddress1 = "5 Yes Avenue",
                CardContactAddress2 = "Close to the Edge",
                CardContactAddress3 = "Near Fragile",
                CardContactTown = "Brighton",
                CardContactCounty = "Sussex",
                CardContactPostcode = "BN3 6TT",
                CardContactCountry = new Country(),
                CardContactTelephone = "01273999555",

                Email = "Jon@yes.com",
                EmailConfirm = "Jon@yes.com",

                UseCardholderContact = false,

                DeliveryContactFirstName = "Jonx",
                DeliveryContactLastName = "Andersonx",
                DeliveryContactAddress1 = "5 Yes Avenuex",
                DeliveryContactAddress2 = "Close to the Edgx",
                DeliveryContactAddress3 = "Near Fragilex",
                DeliveryContactTown = "Brightonx",
                DeliveryContactCounty = "Sussexx",
                DeliveryContactPostcode = "BN3 6TTx",
                DeliveryContactCountry = new Country(),
                DeliveryContactTelephone = "01273999555x",

                AdditionalInformation = "some additional info",

                CardCardType = new CardType(),
                CardHolder = "Jon Anderson",
                CardNumber = "1111111111111117",
                CardExpiryMonth = 3,
                CardExpiryYear = 2012,
                CardStartMonth = 2,
                CardStartYear = 2009,
                CardIssueNumber = "3",
                CardSecurityCode = "123",

                PayByTelephone = false,
                ContactMe = true
            };
	    }

	    private static void VerifyOrderMatchesCheckoutViewData(Order order, CheckoutViewData checkoutViewData)
	    {
	        order.Basket.Id.ShouldEqual(checkoutViewData.BasketId);

	        order.CardContact.Firstname.ShouldEqual(checkoutViewData.CardContactFirstName);
	        order.CardContact.Lastname.ShouldEqual(checkoutViewData.CardContactLastName);
	        order.CardContact.Address1.ShouldEqual(checkoutViewData.CardContactAddress1);
	        order.CardContact.Address2.ShouldEqual(checkoutViewData.CardContactAddress2);
	        order.CardContact.Address3.ShouldEqual(checkoutViewData.CardContactAddress3);
	        order.CardContact.Town.ShouldEqual(checkoutViewData.CardContactTown);
	        order.CardContact.County.ShouldEqual(checkoutViewData.CardContactCounty);
	        order.CardContact.Postcode.ShouldEqual(checkoutViewData.CardContactPostcode);
	        order.CardContact.Country.ShouldEqual(checkoutViewData.CardContactCountry);
	        order.CardContact.Telephone.ShouldEqual(checkoutViewData.CardContactTelephone);

	        order.Email.ShouldEqual(checkoutViewData.Email);

	        order.UseCardHolderContact.ShouldEqual(checkoutViewData.UseCardholderContact);

            order.DeliveryContact.Firstname.ShouldEqual(checkoutViewData.DeliveryContactFirstName);
            order.DeliveryContact.Lastname.ShouldEqual(checkoutViewData.DeliveryContactLastName);
            order.DeliveryContact.Address1.ShouldEqual(checkoutViewData.DeliveryContactAddress1);
            order.DeliveryContact.Address2.ShouldEqual(checkoutViewData.DeliveryContactAddress2);
            order.DeliveryContact.Address3.ShouldEqual(checkoutViewData.DeliveryContactAddress3);
            order.DeliveryContact.Town.ShouldEqual(checkoutViewData.DeliveryContactTown);
            order.DeliveryContact.County.ShouldEqual(checkoutViewData.DeliveryContactCounty);
            order.DeliveryContact.Postcode.ShouldEqual(checkoutViewData.DeliveryContactPostcode);
            order.DeliveryContact.Country.ShouldEqual(checkoutViewData.DeliveryContactCountry);
            order.DeliveryContact.Telephone.ShouldEqual(checkoutViewData.DeliveryContactTelephone);

	        order.AdditionalInformation.ShouldEqual(checkoutViewData.AdditionalInformation);

	        order.Card.CardType.ShouldEqual(checkoutViewData.CardCardType);
            order.Card.Holder.ShouldEqual(checkoutViewData.CardHolder);
            order.Card.Number.ShouldEqual(checkoutViewData.CardNumber);
            order.Card.ExpiryMonth.ShouldEqual(checkoutViewData.CardExpiryMonth);
            order.Card.ExpiryYear.ShouldEqual(checkoutViewData.CardExpiryYear);
            order.Card.StartMonth.ShouldEqual(checkoutViewData.CardStartMonth);
            order.Card.StartYear.ShouldEqual(checkoutViewData.CardStartYear);
            order.Card.IssueNumber.ShouldEqual(checkoutViewData.CardIssueNumber);
            order.Card.SecurityCode.ShouldEqual(checkoutViewData.CardSecurityCode);

	        order.PayByTelephone.ShouldEqual(checkoutViewData.PayByTelephone);
	        order.ContactMe.ShouldEqual(checkoutViewData.ContactMe);
	    }

	    [Test]
		public void IndexWithPost_ShouldRenderViewOnError()
		{
            const int basketId = 7;
            var basket = new Basket { Id = basketId, Country = new Country() };
            basketRepository.Stub(br => br.GetById(basketId)).Return(basket);

			controller.ModelState.AddModelError("foo", "bar");
	        var checkoutViewData = GetCheckoutViewData();
            controller.Index(checkoutViewData)
				.ReturnsViewResult()
                .WithModel<CheckoutViewData>();
		}

		[Test]
		public void Confirm_DisplaysConfirm()
		{
			var order = new Order();
		    orderRepository.EntityFactory = id => order;
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

	    [Test]
	    public void UpdateCountry_should_update_the_country()
	    {
	        const int basketId = 94;
	        var basket = new Basket();
	        basketRepository.Stub(b => b.GetById(basketId)).Return(basket);

	        var checkoutViewData = new CheckoutViewData
	        {
	            BasketId = basketId,
	            UseCardholderContact = true,
	            CardContactCountry = new Country(),
                DeliveryContactCountry = new Country()
	        };

	        controller.UpdateCountry(checkoutViewData)
	            .ReturnsRedirectToRouteResult()
	            .WithRouteValue("action", "Index");

            basket.Country.ShouldBeTheSameAs(checkoutViewData.CardContactCountry);

            controller.TempData["CheckoutViewData"].ShouldBeTheSameAs(checkoutViewData);
	    }
	}
}
// ReSharper restore InconsistentNaming
