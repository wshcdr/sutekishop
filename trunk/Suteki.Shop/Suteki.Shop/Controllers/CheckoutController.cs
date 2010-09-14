using System;
using MvcContrib;
using System.Web.Mvc;
using Suteki.Common.Binders;
using Suteki.Common.Filters;
using Suteki.Common.Repositories;
using Suteki.Shop.Binders;
using Suteki.Shop.Repositories;
using Suteki.Shop.Services;
using Suteki.Shop.ViewData;

namespace Suteki.Shop.Controllers
{
	public class CheckoutController : ControllerBase
	{
		readonly IRepository<Basket> basketRepository;
		readonly IUserService userService;
	    readonly IBasketService basketService;
		readonly IPostageService postageService;
		readonly IRepository<Country> countryRepository;
		readonly IRepository<CardType> cardTypeRepository;
		readonly IRepository<Order> orderRepository;
		readonly IUnitOfWorkManager unitOfWork;
		readonly IEmailService emailService;
		readonly IRepository<MailingListSubscription> mailingListRepository;

		public CheckoutController(
            IRepository<Basket> basketRepository, 
            IUserService userService, 
            IPostageService postageService, 
            IRepository<Country> countryRepository, 
            IRepository<CardType> cardTypeRepository, 
            IRepository<Order> orderRepository, 
            IUnitOfWorkManager unitOfWork, 
            IEmailService emailService, 
            IRepository<MailingListSubscription> mailingListRepository, 
            IBasketService basketService)
		{
			this.basketRepository = basketRepository;
		    this.basketService = basketService;
		    this.emailService = emailService;
			this.mailingListRepository = mailingListRepository;
			this.unitOfWork = unitOfWork;
			this.orderRepository = orderRepository;
			this.cardTypeRepository = cardTypeRepository;
			this.countryRepository = countryRepository;
			this.postageService = postageService;
			this.userService = userService;
		}

        [HttpGet, UnitOfWork]
        public ActionResult Index(int id)
		{
			// create a default order
			var order = CurrentOrder ?? new Order {UseCardHolderContact = true};

			PopulateOrderForView(order, id);

			return View(CheckoutViewData(order));
		}

		void PopulateOrderForView(Order order, int basketId)
		{
            var basket = basketRepository.GetById(basketId);
            order.Basket = basket;
			if (order.CardContact == null) order.CardContact = new Contact();
			if (order.DeliveryContact == null) order.DeliveryContact = new Contact();
			if (order.Card == null) order.Card = new Card { CardType = new CardType { Id = CardType.VisaDeltaElectronId }};
		}

		ShopViewData CheckoutViewData(Order order)
		{
			userService.CurrentUser.EnsureCanViewOrder(order);
			postageService.CalculatePostageFor(order);

			return ShopView.Data
				.WithCountries(countryRepository.GetAll().Active().InOrder())
				.WithCardTypes(cardTypeRepository.GetAll())
				.WithOrder(order);
		}

		[HttpPost, UnitOfWork]
		public ActionResult Index([BindUsing(typeof(OrderBinder))] Order order)
		{
			if (ModelState.IsValid)
			{
				orderRepository.SaveOrUpdate(order);
				//we need an explicit Commit in order to obtain the db-generated Order Id
				unitOfWork.Commit();
				return this.RedirectToAction(c => c.Confirm(order.Id));
			}

            PopulateOrderForView(order, order.Basket.Id);
			return View(CheckoutViewData(order));
		}

		private void EmailOrder(Order order)
		{
			userService.CurrentUser.EnsureCanViewOrder(order);
			postageService.CalculatePostageFor(order);
			emailService.SendOrderConfirmation(order);
		}

        [HttpGet, UnitOfWork]
		public ActionResult Confirm(int id)
		{
			var order = orderRepository.GetById(id);
			userService.CurrentUser.EnsureCanViewOrder(order);
			postageService.CalculatePostageFor(order);
			return View(ShopView.Data.WithOrder(order));
		}

		[HttpPost, UnitOfWork]
		public ActionResult Confirm(Order order)
		{
			order.OrderStatus = OrderStatus.Created;

			if(order.ContactMe)
			{
				var mailingListSubscription = new MailingListSubscription
				{
					Contact = order.PostalContact,
					Email = order.Email,
                    DateSubscribed = DateTime.Now
				};

				mailingListRepository.SaveOrUpdate(mailingListSubscription);
			}

			EmailOrder(order);
		    basketService.CreateNewBasketFor(userService.CurrentUser);
            
			return this.RedirectToAction<OrderController>(c => c.Item(order.Id));
		}

		[UnitOfWork]
		public ActionResult UpdateCountry(int id, int countryId, [BindUsing(typeof(OrderBinder))] Order order)
		{
			//Ignore any errors - if there are any errors in modelstate then the UnitOfWork will not commit.
			ModelState.Clear(); 

			var basket = basketRepository.GetById(id);
		    var country = countryRepository.GetById(countryId);
			basket.Country = country;
			CurrentOrder = order;
			return this.RedirectToAction(c => c.Index(id));
		}

		private Order CurrentOrder
		{
			get { return TempData["order"] as Order; }
			set { TempData["order"] = value; }
		}
	}
}