using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using Suteki.Common.Binders;
using Suteki.Common.Repositories;
using Suteki.Common.Validation;
using Suteki.Shop.Binders;
using Suteki.Shop.Services;

namespace Suteki.Shop.Tests.Binders
{
	[TestFixture]
	public class OrderBinderTester
	{
		OrderBinder binder;
		ControllerContext context;
		IEncryptionService encryptionService;
		IRepository<Basket> basketRepository;
	    IRepository<OrderStatus> orderStatusRepository;
	    Basket basket;

		[SetUp]
		public void Setup()
		{
		    var country = new Country { Name = "United Kingdom" };
            basket = new Basket { Country = country };
			encryptionService = MockRepository.GenerateStub<IEncryptionService>();
            basketRepository = new FakeRepository<Basket>(id => 
            { 
                basket.Id = id;
                return basket; 
            });
		    orderStatusRepository = new FakeRepository<OrderStatus>(id => new OrderStatus{ Id = id });

		    var repositoryResolver = MockRepository.GenerateStub<IRepositoryResolver>();
            repositoryResolver.Stub(r => r.GetRepository(typeof(Country))).Return(new FakeRepository(id => country));
            repositoryResolver.Stub(r => r.GetRepository(typeof(Basket))).Return(new FakeRepository(id => basket));
            repositoryResolver.Stub(r => r.GetRepository(typeof(CardType))).Return(new FakeRepository(id => new CardType()));

            var innerModelBinder = new EntityModelBinder(repositoryResolver);
            var modelBinderDictionary = new ModelBinderDictionary { DefaultBinder = innerModelBinder };
		    var defaultModelBinder = new EntityModelBinder(repositoryResolver);
		    defaultModelBinder.SetModelBinderDictionary(modelBinderDictionary);

			binder = new OrderBinder(
                defaultModelBinder, 
				encryptionService,
				basketRepository,
                orderStatusRepository
			);

			context = new ControllerContext
			{
				HttpContext = MockRepository.GenerateStub<HttpContextBase>()
			};
			context.HttpContext.Expect(x => x.Request).Return(MockRepository.GenerateStub<HttpRequestBase>());
		}

		[Test]
		public void Should_Create_order() {

            // mock the request form
			var form = BuildPlaceOrderRequest(true);
            var bindingContext = BuildBindingContext(form);

		    var order = (Order)binder.BindModel(context, bindingContext);

			// Order
			Assert.AreEqual(10, order.Id, "Order Id is incorrect");
			Assert.AreEqual(form["order.email"], order.Email, "Email is incorrect");
			Assert.AreEqual(form["order.additionalinformation"], order.AdditionalInformation, "AdditionalInformation is incorrect");
			Assert.IsFalse(order.UseCardHolderContact, "UseCardHolderContact is incorrect");
			Assert.IsFalse(order.PayByTelephone, "PayByTelephone is incorrect");

			Assert.AreEqual(DateTime.Now.ToShortDateString(), order.CreatedDate.ToShortDateString(), "CreatedDate is incorrect");

			// Card Contact
			var cardContact = order.CardContact;
			AssertContactIsCorrect(form, cardContact, "cardcontact");

			// Delivery Contact
			var deliveryContact = order.DeliveryContact;
			AssertContactIsCorrect(form, deliveryContact, "deliverycontact");

			// Card
			var card = order.Card;
			Assert.IsNotNull(card, "card is null");
			Assert.AreEqual(form["card.cardtype.id"], card.CardType.Id.ToString());
			Assert.AreEqual(form["card.holder"], card.Holder);
			Assert.AreEqual(form["card.number"], card.Number);
			Assert.AreEqual(form["card.expirymonth"], card.ExpiryMonth.ToString());
			Assert.AreEqual(form["card.expiryyear"], card.ExpiryYear.ToString());
			Assert.AreEqual(form["card.startmonth"], card.StartMonth.ToString());
			Assert.AreEqual(form["card.startyear"], card.StartYear.ToString());
			Assert.AreEqual(form["card.issuenumber"], card.IssueNumber);
			Assert.AreEqual(form["card.securitycode"], card.SecurityCode);

            // TODO: Test is failing because the OrderBinder is trying to bind Country.Name which isn't given
		    OutputBindingErrors(bindingContext.ModelState);
            bindingContext.ModelState.IsValid.ShouldBeTrue("ModelState is invalid");

			encryptionService.AssertWasCalled(es => es.EncryptCard(Arg<Card>.Is.Anything));
		}

	    static void OutputBindingErrors(ModelStateDictionary modelState)
	    {
	        foreach (var result in modelState.Keys
                .Select(key => modelState[key])
                .Where(result => result.Errors.Count > 0))
	        {
	            Console.WriteLine("{0}, {1}", result.Errors[0].ErrorMessage, result.Value);
	        }
	    }

	    static ModelBindingContext BuildBindingContext(NameValueCollection form)
	    {
	        var modelMetadata = new ModelMetadata(
	            new DataAnnotationsModelMetadataProvider(),
	            null,
	            null,
	            typeof(Order),
	            null);

	        return new ModelBindingContext
	        {
	            ValueProvider = new NameValueCollectionValueProvider(form, CultureInfo.GetCultureInfo("EN-GB")),
	            ModelMetadata = modelMetadata
	        };
	    }

	    [Test]
		public void Updates_country()
		{
            var bindingContext = BuildBindingContext(BuildPlaceOrderRequest(true));

            binder.BindModel(context, bindingContext);

			basket.Country.Id.ShouldEqual(6);
		}


		[Test]
		public void Updates_country_when_there_is_no_delivery_contact()
		{
            var bindingContext = BuildBindingContext(BuildPlaceOrderRequest(false));

			binder.BindModel(context, bindingContext);

			basket.Country.Id.ShouldEqual(3);
		}

	    [Test]
	    public void Encryption_service_validation_errors_should_be_added_to_model_binder()
	    {
	        encryptionService.Stub(s => s.EncryptCard(Arg<Card>.Is.Anything)).Throw(new ValidationException("No way!"));

            var bindingContext = BuildBindingContext(BuildPlaceOrderRequest(true));
            binder.UpdateCard(new Order(), context, bindingContext);

            bindingContext.ModelState["validation_error_0"].Errors[0].ErrorMessage.ShouldEqual("No way!");
	    }

		private static void AssertContactIsCorrect(NameValueCollection form, Contact contact, string prefix) {
			Assert.IsNotNull(contact, prefix + " is null");
			Assert.AreEqual(form[prefix + ".firstname"], contact.Firstname, prefix + " Firstname is incorrect");
			Assert.AreEqual(form[prefix + ".lastname"], contact.Lastname, prefix + " Lastname is incorrect");
			Assert.AreEqual(form[prefix + ".address1"], contact.Address1, prefix + " Address1 is incorrect");
			Assert.AreEqual(form[prefix + ".address2"], contact.Address2, prefix + " Address2 is incorrect");
			Assert.AreEqual(form[prefix + ".address3"], contact.Address3, prefix + " Address3 is incorrect");
			Assert.AreEqual(form[prefix + ".town"], contact.Town, prefix + " Town is incorrect");
			Assert.AreEqual(form[prefix + ".county"], contact.County, prefix + " County is incorrect");
			Assert.AreEqual(form[prefix + ".postcode"], contact.Postcode, prefix + " Postcode is incorrect");
			//Assert.AreEqual(form[prefix + ".countryid"], contact.CountryId.ToString(), prefix + " CountryId is incorrect");
			Assert.AreEqual(form[prefix + ".telephone"], contact.Telephone, prefix + " Telephone is incorrect");
		}

		private static FormCollection BuildPlaceOrderRequest(bool includeDeliveryContact) {
			var form = new FormCollection
            {
                {"order.id", "10"},
                {"order.basket.id", "22"}, // TODO: change view to new value
                {"cardcontact.firstname", "Mike"},
                {"cardcontact.lastname", "Hadlow"},
                {"cardcontact.address1", "23 The Street"},
                {"cardcontact.address2", "The Manor"},
                {"cardcontact.address3", "Near Somewhere"},
                {"cardcontact.town", "Hove"},
                {"cardcontact.county", "East Sussex"},
                {"cardcontact.postcode", "BN6 2EE"},
                {"cardcontact.country.id", "3"},
                {"cardcontact.telephone", "01273 234234"},
                {"order.email", "mike@mike.com"},
                {"emailconfirm", "mike@mike.com"},
                {"order.usecardholdercontact", (!includeDeliveryContact).ToString()},
                {"order.additionalinformation", "some more info"},
                {"card.cardtype.id", "1"}, // TODO: change view
                {"card.holder", "MR M HADLOW"},
                {"card.number", "1111111111111117"},
                {"card.expirymonth", "3"},
                {"card.expiryyear", "2009"},
                {"card.startmonth", "2"},
                {"card.startyear", "2003"},
                {"card.issuenumber", "3"},
                {"card.securitycode", "235"},
                {"order.paybytelephone", "False"}
            };

			if(includeDeliveryContact)
			{
				form.Add("deliverycontact.firstname", "Mike");
                form.Add("deliverycontact.lastname", "Hadlow");
                form.Add("deliverycontact.address1", "23 The Street");
                form.Add("deliverycontact.address2", "The Manor");
                form.Add("deliverycontact.address3", "Near Somewhere");
                form.Add("deliverycontact.town", "Hove");
                form.Add("deliverycontact.county", "East Sussex");
                form.Add("deliverycontact.postcode", "BN6 2EE");
                form.Add("deliverycontact.country.id", "6");
				form.Add("deliverycontact.telephone", "01273 234234");
			}

			return form;
		}
	}
}