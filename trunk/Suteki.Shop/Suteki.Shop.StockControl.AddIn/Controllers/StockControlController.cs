using System;
using System.Linq;
using System.Web.Mvc;
using Suteki.Common.AddIns;
using Suteki.Common.Filters;
using Suteki.Common.Repositories;
using Suteki.Shop.StockControl.AddIn.Models;
using Suteki.Shop.StockControl.AddIn.ViewData;

namespace Suteki.Shop.StockControl.AddIn.Controllers
{
    public class StockControlController : AddinControllerBase
    {
        private readonly IRepository<StockItem> stockItemRepository;
        private readonly Now now;
        private readonly CurrentUser currentUser;

        public StockControlController(IRepository<StockItem> stockItemRepository, Now now, CurrentUser currentUser)
        {
            this.stockItemRepository = stockItemRepository;
            this.now = now;
            this.currentUser = currentUser;
        }

        [UnitOfWork]
        public ActionResult List(string id) // id is productName
        {
            var viewData = stockItemRepository.GetAll().Where(x => x.ProductName == id).ToList();
            return View("List", viewData);
        }

        [UnitOfWork]
        public ActionResult History(int id)
        {
            var stockItem = stockItemRepository.GetById(id);
            return View("History", stockItem);
        }

        [HttpPost, UnitOfWork]
        public ActionResult Update(StockUpdateViewData stockUpdateViewData)
        {
            if (stockUpdateViewData == null)
            {
                throw new ArgumentNullException("stockUpdateViewData");
            }

            foreach (var updateItem in stockUpdateViewData.UpdateItems)
            {
                if (updateItem.HasReceivedValue())
                {
                    var stockItem = stockItemRepository.GetById(updateItem.StockItemId);
                    stockItem.ReceiveStock(updateItem.GetReceivedValue(), now(), currentUser())
                        .SetComment(stockUpdateViewData.Comment);
                }
                if (updateItem.HasAdjustedValue())
                {
                    var stockItem = stockItemRepository.GetById(updateItem.StockItemId);
                    stockItem.AdjustStockLevel(updateItem.GetAdjustmentValue(), now(), currentUser())
                        .SetComment(stockUpdateViewData.Comment);
                }
            }

            return Redirect(stockUpdateViewData.ReturnUrl);
        }
    }
}