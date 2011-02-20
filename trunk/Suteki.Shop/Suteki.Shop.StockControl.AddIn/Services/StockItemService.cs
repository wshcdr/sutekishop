using System.Linq;
using System.Collections.Generic;
using Suteki.Common.Extensions;
using Suteki.Common.Repositories;
using Suteki.Shop.StockControl.AddIn.Models;

namespace Suteki.Shop.StockControl.AddIn.Services
{
    public interface IStockItemService
    {
        StockItem GetById(int stockItemId);
        IEnumerable<StockItem> GetAllForProduct(string productName);
    }

    public class StockItemService : IStockItemService
    {
        private readonly IRepository<StockItem> stockItemRepository;

        public StockItemService(IRepository<StockItem> stockItemRepository)
        {
            this.stockItemRepository = stockItemRepository;
        }

        public StockItem GetById(int stockItemId)
        {
            return stockItemRepository.GetById(stockItemId);
        }

        public IEnumerable<StockItem> GetAllForProduct(string productName)
        {
            var stockItems = stockItemRepository
                .GetAll()
                .Where(x => x.ProductName == productName && x.IsActive).AsEnumerable();

            if (stockItems.Count() > 0) return stockItems;

            var defaultItem = stockItemRepository
                .GetAll()
                .Where(x => x.ProductName == productName && x.SizeName == "-")
                .SingleOrDefault();

            if (defaultItem == null)
            {
                throw new StockControlException(
                    "No default StockItem (size named '-') exists for product named '{0}'", productName);
            }

            return defaultItem.ToEnumerable();
        }
    }
}