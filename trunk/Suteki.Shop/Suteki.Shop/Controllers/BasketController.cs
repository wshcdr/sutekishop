using System;
using System.Linq;
using System.Web.Mvc;
using Suteki.Common.Binders;
using Suteki.Common.Extensions;
using Suteki.Common.Filters;
using Suteki.Common.Repositories;
using Suteki.Shop.Binders;
using Suteki.Shop.Filters;
using Suteki.Shop.Repositories;
using Suteki.Shop.ViewData;
using Suteki.Shop.Services;
using MvcContrib;
namespace Suteki.Shop.Controllers
{
    public class BasketController : ControllerBase
    {
        readonly IUserService userService;
        readonly IBasketService basketService;
        readonly IPostageService postageService;
        readonly IRepository<Country> countryRepository;

    	public BasketController(
            IUserService userService, 
            IPostageService postageService, 
            IRepository<Country> countryRepository, 
            IBasketService basketService)
        {
    	    this.basketService = basketService;
    	    this.userService = userService;
            this.postageService = postageService;
            this.countryRepository = countryRepository;
        }

		[FilterUsing(typeof(EnsureSsl))]
        public ActionResult Index()
        {
            var user = userService.CurrentUser;
            return  View("Index", IndexViewData(basketService.GetCurrentBasketFor(user)));
        }

        [UnitOfWork, AcceptVerbs(HttpVerbs.Post)]
		public ActionResult Update([CurrentBasket] Basket basket, [EntityBind(Fetch = false)] BasketItem basketItem)
        {
            if (!basketItem.Size.IsInStock)
            {
                Message = RenderIndexViewWithError(basketItem.Size);
                return this.RedirectToAction<ProductController>(c => c.Item(basketItem.Size.Product.UrlName));
            }

            basket.AddBasketItem(basketItem);
			return this.RedirectToAction(c => c.Index());
        }

        private string RenderIndexViewWithError(Size size)
        {
        	if (size.Product.HasSize)
            {
                return "Sorry, {0}, Size {1} is out of stock.".With(size.Product.Name, size.Name);
            }

        	return "Sorry, {0} is out of stock.".With(size.Product.Name);
        }

        private ShopViewData IndexViewData(Basket basket)
        {
            if (basket.Country == null)
            {
                throw new ApplicationException("Country has not been lazy loaded for this basket");
            }

			var countries = countryRepository.GetAll().Active().InOrder();

            return ShopView.Data.WithBasket(basket)
				.WithCountries(countries)
                .WithTotalPostage(postageService.CalculatePostageFor(basket));
        }

		[UnitOfWork]
        public ActionResult Remove(int id)
        {
            var basket = basketService.GetCurrentBasketFor(userService.CurrentUser);
            var basketItem = basket.BasketItems.Where(item => item.Id == id).SingleOrDefault();

            if (basketItem != null)
            {
                basket.RemoveBasketItem(basketItem);
            }

			return this.RedirectToAction(c => c.Index());
        }

		[AcceptVerbs(HttpVerbs.Post), UnitOfWork]
		public ActionResult UpdateCountry(Country country)
		{
			if (country.Id != 0) 
			{
				basketService.GetCurrentBasketFor(userService.CurrentUser).Country = country;
			}

			return this.RedirectToAction(c => c.Index());
		}

		[AcceptVerbs(HttpVerbs.Post), UnitOfWork]
        public ActionResult GoToCheckout(Country country)
		{
		    var currentBasket = basketService.GetCurrentBasketFor(userService.CurrentUser);
            if (country.Id != 0)
			{
                currentBasket.Country = country;
            }

            return this.RedirectToAction<CheckoutController>(c => c.Index(currentBasket.Id));
		}
    }
}
