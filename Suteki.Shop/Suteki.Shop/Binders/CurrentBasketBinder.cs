using System.Web.Mvc;
using Suteki.Common.Binders;
using Suteki.Shop.Services;

namespace Suteki.Shop.Binders
{
	public class CurrentBasketAttribute : BindUsingAttribute
	{
		public CurrentBasketAttribute() : base(typeof(CurrentBasketBinder))
		{
		}
	}

	public class CurrentBasketBinder : IModelBinder
	{
		readonly IUserService userService;
	    readonly IBasketService basketService;

		public CurrentBasketBinder(IUserService userService, IBasketService basketService)
		{
		    this.userService = userService;
		    this.basketService = basketService;
		}

	    public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			var user = userService.CurrentUser;

			if(UserIsGuest(user))
			{
				user = PromoteGuestToCustomer();
			}

	        return basketService.GetCurrentBasketFor(user);
		}

		private User PromoteGuestToCustomer()
		{
			var user = userService.CreateNewCustomer();
			userService.SetAuthenticationCookie(user.Email);
			userService.SetContextUserTo(user);
			return user;
		}

		private static bool UserIsGuest(User user)
		{
			return user.Role.Id == Role.GuestId;
		}
	}
}