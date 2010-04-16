using System;
using System.Globalization;
using System.Web.Mvc;
using Suteki.Common.Binders;
using Suteki.Common.Repositories;
using Suteki.Common.Validation;
using Suteki.Shop.Services;

namespace Suteki.Shop.Binders
{
	public class OrderBinder : IModelBinder
	{
		readonly IModelBinder defaultBinder;
		readonly IEncryptionService encryptionService;
		readonly IRepository<Basket> basketRepository;
	    readonly IRepository<OrderStatus> orderStatusRepository;

        public OrderBinder(IModelBinder defaultBinder, IEncryptionService encryptionService, IRepository<Basket> basketRepository, IRepository<OrderStatus> orderStatusRepository)
		{
			this.defaultBinder = defaultBinder;
            this.orderStatusRepository = orderStatusRepository;
            this.basketRepository = basketRepository;
			this.encryptionService = encryptionService;

            SwitchOffModelBinderFetch(defaultBinder);
		}

	    static void SwitchOffModelBinderFetch(IModelBinder modelBinder)
	    {
	        var acceptsAttribute = modelBinder as IAcceptsAttribute;
            if(acceptsAttribute != null)
            {
                acceptsAttribute.Accept(new EntityBindAttribute{ Fetch = false });
            }
	    }

	    public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			var order = new Order
			{
                OrderStatus = orderStatusRepository.GetById(OrderStatus.PendingId),
				CreatedDate = DateTime.Now,
				DispatchedDate = DateTime.Now
			};

		    UpdateOrder(order, controllerContext, bindingContext);
		    UpdateCardContact(order, controllerContext, bindingContext);
		    UpdateDeliveryContact(order, controllerContext, bindingContext);
		    UpdateCard(order, controllerContext, bindingContext);
		
			EnsureBasketCountryId(order);

			return order;
		}

	    void UpdateOrder(Order order, ControllerContext controllerContext, ModelBindingContext bindingContext)
	    {
	        defaultBinder.BindModel(controllerContext, BuildBindingContext(bindingContext, order, "order"));
	        var confirmEmail = bindingContext.ValueProvider.GetValue("emailconfirm").AttemptedValue;
	        if (order.Email != confirmEmail)
	        {
	            //hackery...
	            bindingContext.ModelState.AddModelError("order.email", "Email and Confirm Email do not match");
	            bindingContext.ModelState.SetModelValue("order.email", new ValueProviderResult(order.Email, order.Email, CultureInfo.CurrentCulture));
	        }
	    }

	    void UpdateCardContact(Order order, ControllerContext controllerContext, ModelBindingContext bindingContext)
	    {
	        var cardContact = new Contact();
	        order.CardContact = cardContact;
	        defaultBinder.BindModel(controllerContext, BuildBindingContext(bindingContext, cardContact, "cardcontact"));
	    }

	    void UpdateDeliveryContact(Order order, ControllerContext controllerContext, ModelBindingContext bindingContext)
	    {
	        if (order.UseCardHolderContact) return;

	        var deliveryContact = new Contact();
	        order.DeliveryContact = deliveryContact;
	        defaultBinder.BindModel(controllerContext, BuildBindingContext(bindingContext, deliveryContact, "deliverycontact"));
	    }

	    public void UpdateCard(Order order, ControllerContext controllerContext, ModelBindingContext bindingContext)
	    {
	        if (order.PayByTelephone) return;

	        var card = new Card();
	        order.Card = card;
	        defaultBinder.BindModel(controllerContext, BuildBindingContext(bindingContext, card, "card"));

            // don't attempt to encrypt card if there are any model binding errors.
	        if (!bindingContext.ModelState.IsValid)
	        {
                return;
	        }

	        var validator = new Validator
	        {
                () => encryptionService.EncryptCard(card)
	        };
	        validator.Validate(bindingContext.ModelState);
	    }

	    void EnsureBasketCountryId(Order order)
		{
			var basket = basketRepository.GetById(order.Basket.Id);

            // TODO: this code expects that equality works for entities
			if (order.DeliveryContact != null && basket.Country != order.DeliveryContact.Country) //delivery contact
			{
				basket.Country = order.DeliveryContact.Country;
			}
			//No Delivery contact specified - resort to CardContact. 
			else if(order.CardContact != null && basket.Country != order.CardContact.Country)
			{
				basket.Country = order.CardContact.Country;
			}
		}

	    static ModelBindingContext BuildBindingContext<T>(ModelBindingContext context, T entity, string prefix)
	    {
	        var metaData = new ModelMetadata(
	            new DataAnnotationsModelMetadataProvider(),
	            null,
	            () => entity,
	            typeof(T),
	            null);

	        return new ModelBindingContext
	        {
                ModelName = prefix,
	            ModelMetadata = metaData,
	            ValueProvider = context.ValueProvider,
                ModelState = context.ModelState
	        };
	    }
	}
}