using System.Linq;
using Suteki.Common.Events;
using Suteki.Common.Repositories;
using Suteki.Shop.Exports.Events;

namespace Suteki.Shop.Handlers
{
    public class ChangeInStockStatusOnStockItemInStockChanged : IHandle<StockItemInStockChanged>
    {
        private readonly IRepository<Size> sizeRepository;

        public ChangeInStockStatusOnStockItemInStockChanged(IRepository<Size> sizeRepository)
        {
            this.sizeRepository = sizeRepository;
        }

        public void Handle(StockItemInStockChanged @event)
        {
            var sizeName = @event.SizeName;
            var productName = @event.ProductName;

            var size = sizeRepository.GetAll().Where(x =>
                x.Name == sizeName &&
                x.Product.UrlName == productName &&
                x.IsActive)
                .SingleOrDefault();

            size.IsInStock = @event.IsInStock;
        }
    }
}