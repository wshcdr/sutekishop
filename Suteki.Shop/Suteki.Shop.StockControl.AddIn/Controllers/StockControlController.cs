using System;
using System.Web.Mvc;
using Suteki.Common.AddIns;
using Suteki.Common.Filters;
using Suteki.Shop.StockControl.AddIn.Services;
using Suteki.Shop.StockControl.AddIn.ViewData;

namespace Suteki.Shop.StockControl.AddIn.Controllers
{
    public class StockControlController : AddinControllerBase
    {
        private readonly IStockItemService stockItemService;
        private readonly Now now;
        private readonly CurrentUser currentUser;

        public StockControlController(IStockItemService stockItemService, Now now, CurrentUser currentUser)
        {
            this.stockItemService = stockItemService;
            this.now = now;
            this.currentUser = currentUser;
        }

        [UnitOfWork]
        public ActionResult List(string id) // id is productName
        {
            var viewData = stockItemService.GetAllForProduct(id);
            return View("List", viewData);
        }

        [UnitOfWork]
        public ActionResult History(int id)
        {
            var stockItem = stockItemService.GetById(id);
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
                    var stockItem = stockItemService.GetById(updateItem.StockItemId);
                    stockItem.ReceiveStock(updateItem.GetReceivedValue(), now(), currentUser())
                        .SetComment(stockUpdateViewData.Comment);
                }
                if (updateItem.HasAdjustedValue())
                {
                    var stockItem = stockItemService.GetById(updateItem.StockItemId);
                    stockItem.AdjustStockLevel(updateItem.GetAdjustmentValue(), now(), currentUser())
                        .SetComment(stockUpdateViewData.Comment);
                }
            }

            return Redirect(stockUpdateViewData.ReturnUrl);
        }
    }
}