using System;
using MvcContrib;
using System.Web.Mvc;
using Suteki.Common.Filters;
using Suteki.Common.Repositories;
using Suteki.Common.Validation;
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
	    readonly IEncryptionService encryptionService;

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
            IEncryptionService encryptionService)
		{
			this.basketRepository = basketRepository;
		    this.encryptionService = encryptionService;
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
		    var order = OrderFromCheckoutViewData(checkoutViewData);

			if (ModelState.IsValid)
			{
				orderRepository.SaveOrUpdate(order);
				//we need an explicit Commit in order to obtain the db-generated Order Id
				unitOfWork.Commit();
				return this.RedirectToAction(c => c.Confirm(order.Id));
			}

		    return View("Index", checkoutViewData);
		}

	    [NonAction]
	    public Order OrderFromCheckoutViewData(CheckoutViewData checkoutViewData)
	    {
            if(EmailAddressesDoNotMatch(checkoutViewData)) return null;

	        var basket = basketRepository.GetById(checkoutViewData.BasketId);
	        var order = new Order
	        {
	            Basket = basket,
	            Email = checkoutViewData.Email,
	            AdditionalInformation = checkoutViewData.AdditionalInformation,
	            ContactMe = checkoutViewData.ContactMe,
	            Card = GetCardFromViewData(checkoutViewData),
	            CardContact = GetCardContactFromViewData(checkoutViewData),
	            CreatedDate = DateTime.Now,
	            DeliveryContact = GetDeliveryContactFromViewData(checkoutViewData),
	            DispatchedDate = DateTime.Now,
                OrderStatus = OrderStatus.Pending,
                UseCardHolderContact = checkoutViewData.UseCardholderContact,
                PayByTelephone = checkoutViewData.PayByTelephone
	        };
	        EnsureBasketCountry(order);
	        return order;
	    }

	    private bool EmailAddressesDoNotMatch(CheckoutViewData checkoutViewData)
	    {
	        if (checkoutViewData.Email != checkoutViewData.EmailConfirm)
	        {
                ModelState.AddModelError("EmailConfirm", "Email Addresses do not match");
	            return true;
	        }
	        return false;
	    }

	    private Contact GetCardContactFromViewData(CheckoutViewData checkoutViewData)
	    {
	        var cardContact = new Contact
	        {
	            Address1 = checkoutViewData.CardContactAddress1,
	            Address2 = checkoutViewData.CardContactAddress2,
	            Address3 = checkoutViewData.CardContactAddress3,
	            Country = checkoutViewData.CardContactCountry,
	            County = checkoutViewData.CardContactCounty,
	            Firstname = checkoutViewData.CardContactFirstName,
	            Lastname = checkoutViewData.CardContactLastName,
	            Town = checkoutViewData.CardContactTown,
	            Telephone = checkoutViewData.CardContactTelephone,
	            Postcode = checkoutViewData.CardContactPostcode
	        };
            
            DataAnnotationsValidator
                .Validate(cardContact)
                .WithPropertyPrefix("CardContact")
                .AndUpdate(ModelState);
	        
            return cardContact;
	    }

	    private Contact GetDeliveryContactFromViewData(CheckoutViewData checkoutViewData)
	    {
            if (checkoutViewData.UseCardholderContact) return null;

            var deliveryContact = new Contact
            {
                Address1 = checkoutViewData.DeliveryContactAddress1,
                Address2 = checkoutViewData.DeliveryContactAddress2,
                Address3 = checkoutViewData.DeliveryContactAddress3,
                Country = checkoutViewData.DeliveryContactCountry,
                County = checkoutViewData.DeliveryContactCounty,
                Firstname = checkoutViewData.DeliveryContactFirstName,
                Lastname = checkoutViewData.DeliveryContactLastName,
                Town = checkoutViewData.DeliveryContactTown,
                Telephone = checkoutViewData.DeliveryContactTelephone,
                Postcode = checkoutViewData.DeliveryContactPostcode
            };

            DataAnnotationsValidator
                .Validate(deliveryContact)
                .WithPropertyPrefix("DeliveryContact")
                .AndUpdate(ModelState);
	        
            return deliveryContact;
	    }

	    private Card GetCardFromViewData(CheckoutViewData checkoutViewData)
	    {
            if (checkoutViewData.PayByTelephone) return null;

	        var card = new Card
	        {
                Holder = checkoutViewData.CardHolder,
                Number = checkoutViewData.CardNumber,
                ExpiryMonth = checkoutViewData.CardExpiryMonth,
                ExpiryYear = checkoutViewData.CardExpiryYear,
                StartMonth = checkoutViewData.CardStartMonth,
                StartYear = checkoutViewData.CardStartYear,
                IssueNumber = checkoutViewData.CardIssueNumber,
                SecurityCode = checkoutViewData.CardSecurityCode,
                CardType = checkoutViewData.CardCardType
	        };
            
            DataAnnotationsValidator
                .Validate(card)
                .WithPropertyPrefix("Card")
                .AndUpdate(ModelState);

            // don't attempt to encrypt card if there are any model binding errors.
            if (ModelState.IsValid)
            {
                var validator = new Validator
	            {
                    () => encryptionService.EncryptCard(card)
	            };
                validator.Validate(ModelState);
            }

	        return card;
	    }

	    static void EnsureBasketCountry(Order order)
	    {
	        order.Basket.Country = order.UseCardHolderContact ? 
                order.CardContact.Country : 
                order.DeliveryContact.Country;
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