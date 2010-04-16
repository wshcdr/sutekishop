using System;
using System.Linq;
using Suteki.Common.Repositories;
using Suteki.Shop.Repositories;

namespace Suteki.Shop.Services
{
    public class BasketService : IBasketService
    {
        const string defaultCountryName = "United Kingdom";

        readonly IRepository<Country> countryRepository;

        public BasketService(IRepository<Country> countryRepository)
        {
            this.countryRepository = countryRepository;
        }

        public Basket GetCurrentBasketFor(User user)
        {
            return user.Baskets.Count == 0 ? 
                CreateNewBasketFor(user) : 
                user.Baskets.CurrentBasket();
        }

        public Basket CreateNewBasketFor(User user)
        {
            var country = countryRepository.GetAll().SingleOrDefault(c => c.Name == defaultCountryName);
            if (country == null)
            {
                throw new ApplicationException(string.Format(
                    "There is no country with Name == '{0}'. (this name is coded in BasketService.defaultCountryName)",
                    defaultCountryName));
            }
            var basket = new Basket
            {
                OrderDate = DateTime.Now,
                Country = country
            };
            user.AddBasket(basket);
            return basket;
        }
    }
}