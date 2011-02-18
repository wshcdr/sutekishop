using System;
using System.Collections.Generic;
using Suteki.Common.Models;

namespace Suteki.Shop.StockControl.AddIn.Models
{
    public class StockItem : IEntity, IActivatable
    {
        public virtual int Id { get; set; }
        public virtual string ProductName { get; protected set; }
        public virtual string SizeName { get; protected set; }
        public virtual int Level { get; protected set; }
        public virtual bool IsActive { get; set; }

        public virtual IList<StockItemHistoryBase> History { get; protected set; }

        public virtual bool IsInStock
        {
            get { return Level > 0; }
        }

        protected StockItem()
        {
        }

        protected StockItem(string productName, string sizeName)
        {
            History = new List<StockItemHistoryBase>();
            ProductName = productName;
            SizeName = sizeName;
            Level = 0;
            IsActive = true;
        }

        public static StockItem Create(string productName, string sizeName, DateTime dateCreated, string user)
        {
            var stockitem = new StockItem(productName, sizeName);
            stockitem.History.Add(new StockItemCreated(dateCreated, user, stockitem, 0));
            return stockitem;
        }

        public virtual ReceivedStock ReceiveStock(int numberOfItems, DateTime dateReceived, string user)
        {
            Level += numberOfItems;
            var history = new ReceivedStock(numberOfItems, dateReceived, user, this, Level);
            History.Add(history);
            return history;
        }

        public virtual DispatchedStock Dispatch(int numberOfItems, int orderNumber, DateTime dateDispactched, string user)
        {
            Level -= numberOfItems;
            var history = new DispatchedStock(dateDispactched, numberOfItems, orderNumber, user, this, Level);
            History.Add(history);
            return history;
        }

        public virtual StockAdjustment AdjustStockLevel(int newLevel, DateTime dateAdjusted, string user)
        {
            Level = newLevel;
            var history = new StockAdjustment(dateAdjusted, newLevel, user, this, Level);
            History.Add(history);
            return history;
        }

        public virtual StockItemDeactivated Deactivate(DateTime dateDeactivated, string user)
        {
            IsActive = false;
            var history = new StockItemDeactivated(dateDeactivated, user, this, Level);
            History.Add(history);
            return history;
        }

        public virtual StockItemProductNameChanged ChangeProductName(string oldProductName, string newProductName, DateTime dateChanged, string currentUser)
        {
            if (ProductName != oldProductName)
            {
                throw new ArgumentException(string.Format("oldProductName '{0}' does not match ProductName '{1}'.",
                                                          oldProductName, ProductName));
            }
            ProductName = newProductName;
            var history = new StockItemProductNameChanged(dateChanged, currentUser, this, Level, oldProductName, newProductName);
            History.Add(history);
            return history;
        }
    }
}