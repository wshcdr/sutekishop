using Suteki.Common.Models;

namespace Suteki.Shop
{
    public class OrderLine : IEntity
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public Order Order { get; set; }
    }
}