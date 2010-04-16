using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Suteki.Common.Repositories;
using Suteki.Shop.Services;

namespace Suteki.Shop.Tests.Services
{
    [TestFixture]
    public class BasketServiceTests
    {
        IBasketService basketService;
        IRepository<Country> countryRepository;

        [SetUp]
        public void SetUp()
        {
            countryRepository = MockRepository.GenerateStub<IRepository<Country>>();
            basketService = new BasketService(countryRepository);
        }

        [Test]
        public void GetCurrentBasket_should_return_the_basket_collections_current_basket()
        {
            var basket = new Basket();
            var user = new User
            {
                Baskets =
                    {
                        basket
                    }
            };

            var currentBasket = basketService.GetCurrentBasketFor(user);

            currentBasket.ShouldBeTheSameAs(basket);
        }

        [Test]
        public void GetCurrentBasket_should_return_a_new_basket_when_the_user_has_no_baskets()
        {
            var country = new Country { Name = "United Kingdom" }; // expect the default country to be United Kingdom.
            countryRepository.Stub(r => r.GetAll()).Return(new[] {country}.AsQueryable());

            var user = new User();

            var currentBasket = basketService.GetCurrentBasketFor(user);

            currentBasket.Country.ShouldBeTheSameAs(country);
        }

        [Test, ExpectedException(typeof(ApplicationException))]
        public void GetCurrentBasket_should_throw_when_the_default_country_is_not_in_the_database()
        {
            var country = new Country { Name = "France" }; // expect the default country to be UK.
            countryRepository.Stub(r => r.GetAll()).Return(new[] { country }.AsQueryable());

            var user = new User();

            basketService.GetCurrentBasketFor(user);
        }
    }
}