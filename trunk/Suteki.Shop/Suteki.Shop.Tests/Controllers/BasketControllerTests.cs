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
    public class BasketControllerTests
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            userService = MockRepository.GenerateStub<IUserService>();
            postageService = MockRepository.GenerateStub<IPostageService>();
            countryRepository = MockRepository.GenerateStub<IRepository<Country>>();
            basketService = new BasketService(countryRepository);

            basketController = new BasketController(
                userService,
                postageService,
                countryRepository,
                basketService);

            testContext = new ControllerTestContext(basketController);

			user = new User { Baskets = { new Basket { Id = 4, Country = new Country() } } };
			userService.Expect(x => x.CurrentUser).Return(user);
        }

        #endregion

        private User user;

        private BasketController basketController;
        private ControllerTestContext testContext;

        private IUserService userService;
        IBasketService basketService;
        private IPostageService postageService;
        private IRepository<Country> countryRepository;

        private static BasketItem CreateBasketItem()
        {
            var product = new Product { Name = "Denim Jacket", UrlName = "denim_jacket", Weight = 10 };
            var size = new Size
            {
                Id = 5,
                Name = "S",
                IsInStock = true,
                IsActive = true,
                Product = product
            };
            product.Sizes.Add(size);

			return new BasketItem { Size = size, Quantity = 2 };
        }

        [Test]
        public void Index_ShouldShowIndexViewWithCurrentBasket()
        {
            testContext.TestContext.Context.User = user;
			countryRepository.Expect(x => x.GetAll()).Return(new List<Country>().AsQueryable());


			basketController.Index()
				.ReturnsViewResult()
				.ForView("Index")
				.WithModel<ShopViewData>()
				.AssertAreSame(user.Baskets[0], vd => vd.Basket)
				.AssertNotNull(x => x.Countries);
        }

    	[Test]
    	public void GoToCheckout_UpdatesCountry()
    	{
    	    var country = new Country{ Id = 5 };

            basketController.GoToCheckout(country);
            basketService.GetCurrentBasketFor(userService.CurrentUser).Country.ShouldBeTheSameAs(country);
    	}

    	[Test]
    	public void GoToCheckout_RedirectsToCheckout()
    	{
            basketController.GoToCheckout(new Country { Id = 5 })
				.ReturnsRedirectToRouteResult()
				.ToController("Checkout")
				.ToAction("Index")
                .WithRouteValue("id", basketService.GetCurrentBasketFor(user).Id.ToString());
    	}

    	[Test]
    	public void UpdateCountry_UpdatesCountry()
    	{
            var country = new Country { Id = 5 };
            basketController.UpdateCountry(country);
            basketService.GetCurrentBasketFor(userService.CurrentUser).Country.ShouldBeTheSameAs(country);
    	}

    	[Test]
    	public void UpdateCountry_RedirectsToIndex()
    	{
            basketController.UpdateCountry(new Country { Id = 5 }).ReturnsRedirectToRouteResult().ToAction("Index");
    	}

    	[Test]
        public void Remove_ShouldRemoveItemFromBasket()
        {
            const int basketItemIdToRemove = 3;

            var basketItem = new BasketItem
            {
                Id = basketItemIdToRemove,
                Quantity = 1,
                Size = new Size
                {
                    Product = new Product {Weight = 100}
                }
            };
            user.Baskets[0].BasketItems.Add(basketItem);
            testContext.TestContext.Context.User = user;

            basketController.Remove(basketItemIdToRemove)
				.ReturnsRedirectToRouteResult()
				.ToAction("Index");

    	    user.Baskets[0].BasketItems.Count.ShouldEqual(0);
        }

        [Test]
        public void Update_ShouldAddBasketLineToCurrentBasket()
        {
            var basketItem = CreateBasketItem();
            var basket = new Basket();

            basketController.Update(basket, basketItem);

            Assert.AreEqual(1, basket.BasketItems.Count, "expected BasketItem is missing");
            Assert.AreEqual(5, basket.BasketItems[0].Size.Id);
            Assert.AreEqual(2, basket.BasketItems[0].Quantity);
        }

        [Test]
        public void Update_ShouldShowErrorMessageIfItemIsOutOfStock()
        {
            var basketItem = CreateBasketItem();
            basketItem.Size.IsInStock = false;

            const string expectedMessage = "Sorry, Denim Jacket, Size S is out of stock.";

            basketController.Update(basketService.GetCurrentBasketFor(user), basketItem)
				.ReturnsRedirectToRouteResult()
				.ToController("Product")
				.ToAction("Item")
                .WithRouteValue("urlName", basketItem.Size.Product.UrlName);

			basketController.Message.ShouldEqual(expectedMessage);

            Assert.AreEqual(0, user.Baskets[0].BasketItems.Count, "should not be any basket items");
        }
    }
}