namespace Suteki.Shop.Services
{
    public interface IBasketService
    {
        Basket GetCurrentBasketFor(User user);
        Basket CreateNewBasketFor(User user);
    }
}