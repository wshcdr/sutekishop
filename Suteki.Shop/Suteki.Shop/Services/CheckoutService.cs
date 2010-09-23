using System;
using System.Web.Mvc;
using Suteki.Common.Repositories;
using Suteki.Common.Validation;
using Suteki.Shop.ViewData;

namespace Suteki.Shop.Services
{
    public interface ICheckoutService
    {
        Order OrderFromCheckoutViewData(CheckoutViewData checkoutViewData, ModelStateDictionary modelState);
    }

    public class CheckoutService : ICheckoutService
	{
	    readonly IRepository<Basket> basketRepository;
        readonly IEncryptionService encryptionService;
        readonly IPostageService postageService;

	    public CheckoutService(
            IRepository<Basket> basketRepository, 
            IEncryptionService encryptionService, 
            IPostageService postageService)
	    {
	        this.basketRepository = basketRepository;
	        this.postageService = postageService;
	        this.encryptionService = encryptionService;
	    }

	    public Order OrderFromCheckoutViewData(CheckoutViewData checkoutViewData, ModelStateDictionary modelState)
	    {
            if (EmailAddressesDoNotMatch(checkoutViewData, modelState)) return null;

            var basket = basketRepository.GetById(checkoutViewData.BasketId);
            var order = new Order
            {
                Basket = basket,
                Email = checkoutViewData.Email,
                AdditionalInformation = checkoutViewData.AdditionalInformation,
                ContactMe = checkoutViewData.ContactMe,
                Card = GetCardFromViewData(checkoutViewData, modelState),
                CardContact = GetCardContactFromViewData(checkoutViewData, modelState),
                CreatedDate = DateTime.Now,
                DeliveryContact = GetDeliveryContactFromViewData(checkoutViewData, modelState),
                DispatchedDate = DateTime.Now,
                OrderStatus = OrderStatus.Pending,
                UseCardHolderContact = checkoutViewData.UseCardholderContact,
                PayByTelephone = checkoutViewData.PayByTelephone
            };
            EnsureBasketCountry(order);
            AddOrderLinesFromBasket(order, basket);
	        CalcuatePostage(order, basket);
            return order;
        }

        private void CalcuatePostage(Order order, Basket basket)
        {
            order.Postage = postageService.CalculatePostageFor(basket);
        }

        private static void AddOrderLinesFromBasket(Order order, Basket basket)
        {
            foreach (var basketItem in basket.BasketItems)
            {
                var productName = basketItem.Size.Product.Name + " - " + basketItem.Size.Name;
                order.AddLine(
                    productName,
                    basketItem.Quantity,
                    basketItem.Size.Product.Price);
            }
        }

        private static bool EmailAddressesDoNotMatch(CheckoutViewData checkoutViewData, ModelStateDictionary modelState)
        {
            if (checkoutViewData.Email != checkoutViewData.EmailConfirm)
            {
                modelState.AddModelError("EmailConfirm", "Email Addresses do not match");
                return true;
            }
            return false;
        }

        private static Contact GetCardContactFromViewData(CheckoutViewData checkoutViewData, ModelStateDictionary modelState)
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
                .AndUpdate(modelState);

            return cardContact;
        }

        private static Contact GetDeliveryContactFromViewData(CheckoutViewData checkoutViewData, ModelStateDictionary modelState)
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
                .AndUpdate(modelState);

            return deliveryContact;
        }

        private Card GetCardFromViewData(CheckoutViewData checkoutViewData, ModelStateDictionary modelState)
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
                .AndUpdate(modelState);

            // don't attempt to encrypt card if there are any model binding errors.
            if (modelState.IsValid)
            {
                var validator = new Validator
	            {
                    () => encryptionService.EncryptCard(card)
	            };
                validator.Validate(modelState);
            }

            return card;
        }

        static void EnsureBasketCountry(Order order)
        {
            order.Basket.Country = order.UseCardHolderContact ?
                order.CardContact.Country :
                order.DeliveryContact.Country;
        }

	}
}