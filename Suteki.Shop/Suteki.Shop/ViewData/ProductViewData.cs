using System.Collections.Generic;
using Suteki.Common.Models;

namespace Suteki.Shop.ViewData
{
    public class ProductViewData
    {
        public ProductViewData()
        {
            CategoryIds = new List<int>();
            Sizes = new List<string>();
            ProductImages = new List<ProductImage>();
        }

        public int ProductId { get; set; }
        public int Position { get; set; }
        public string Name { get; set; }
        public IList<int> CategoryIds { get; set; }
        public int Weight { get; set; }
        public Money Price { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
        public IList<string> Sizes { get; set; }
        public IList<ProductImage> ProductImages { get; set; }
    }
}