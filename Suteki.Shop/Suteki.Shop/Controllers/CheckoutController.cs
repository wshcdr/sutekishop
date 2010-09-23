using System;
using MvcContrib;
using System.Web.Mvc;
using Suteki.Common.Filters;
using Suteki.Common.Repositories;
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
	    readonly IRepository<CardType> cardTypeRepository;
		readonly IRepository<Order> orderRepository;
		readonly IUnitOfWorkManager unitOfWork;
		readonly IEmailService emailService;
		readonly IRepository<MailingListSubscription> mailingListRepository;
	    readonly ICheckoutService checkoutService;

		public CheckoutController(
            IRepository<Basket> basketRepository, 
            IUserService userService, 
            IPostageService postageService, 
            IRepository<CardType> cardTypeRepository, 
            IRepository<Order> orderRepository, 
            IUnitOfWorkManager unitOfWork, 
            IEmailService emailService, 
            IRepository<MailingListSubscription> mailingListRepository, 
            IBasketService basketService, 
            ICheckoutService checkoutService)
		{
			this.basketRepository = basketRepository;
		    this.checkoutService = checkoutService;
		    this.basketService = basketService;
		    this.emailService = emailService;
			this.mailingListRepository = mailingListRepository;
			this.unitOfWork = unitOfWork;
			this.orderRepository = orderRepository;
			this.cardTypeRepository = cardTypeRepository;
		    this.postageService = postageService;
			this.userService = userService;
		}

        [HttpGet, UnitOfWork]
        public ActionResult Index(int id)
		{
            var viewData = CurrentOrder ?? CreateCheckoutViewData(basketRepository.GetById(id));
            return View("Index", viewData);
		}

		[HttpPost, UnitOfWork]
		public ActionResult Index(CheckoutViewData checkoutViewData)
		{
		    var order = checkoutService.OrderFromCheckoutViewData(checkoutViewData, ModelState);

			if (ModelState.IsValid)
			{
				orderRepository.SaveOrUpdate(order);
				//we need an explicit Commit in order to obtain the db-generated Order Id
				unitOfWork.Commit();
				return this.RedirectToAction(c => c.Confirm(order.Id));
			}

		    return View("Index", checkoutViewData);
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
		    basketService.CreateNewBasketForCurrentUser();
            
			return this.RedirectToAction<OrderController>(c => c.Item(order.Id));
		}

        [HttpPost, UnitOfWork]
		public ActionResult UpdateCountry(CheckoutViewData checkoutViewData)
		{
			//Ignore any errors - if there are any errors in modelstate then the UnitOfWork will not commit.
			ModelState.Clear(); 

			var basket = basketRepository.GetById(checkoutViewData.BasketId);

		    var country = checkoutViewData.UseCardholderContact
		                      ? checkoutViewData.CardContactCountry
		                      : checkoutViewData.DeliveryContactCountry;
			
            basket.Country = country;
            CurrentOrder = checkoutViewData;
            return this.RedirectToAction(c => c.Index(checkoutViewData.BasketId));
		}

        private CheckoutViewData CurrentOrder
		{
            get { return TempData["CheckoutViewData"] as CheckoutViewData; }
            set { TempData["CheckoutViewData"] = value; }
		}

        [NonAction]
	    public CheckoutViewData CreateCheckoutViewData(Basket basket)
	    {
	        var cardType = cardTypeRepository.GetById(CardType.VisaDeltaElectronId);

	        return new CheckoutViewData
	        {
	            BasketId = basket.Id,
	            CardCardType = cardType,
	            CardContactCountry = basket.Country,
	            DeliveryContactCountry = basket.Country,
	            UseCardholderContact = true
	        };
	    }
	}
}