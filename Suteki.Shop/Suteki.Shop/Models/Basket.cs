using System;
using System.Collections.Generic;
using System.Linq;
using Suteki.Common.Models;

namespace Suteki.Shop
{
    public class Basket : IEntity
    {
        public virtual int Id { get; set; }
        public virtual DateTime OrderDate { get; set; }
        public virtual Country Country { get; set; }
        public virtual User User { get; set; }

        IList<BasketItem> basketItems = new List<BasketItem>();
        public virtual IList<BasketItem> BasketItems
        {
            get { return basketItems; }
            set { basketItems = value; }
        }

        IList<Order> orders = new List<Order>();
        public virtual IList<Order> Orders
        {
            get { return orders; }
            set { orders = value; }
        }

        
        private PostageResult postageTotal;

        public virtual bool IsEmpty
        {
            get
            {
                return !BasketItems.Any();
            }
        }

        public virtual decimal Total
        {
            get
            {
                return BasketItems.Sum(item => item.Total);
            }
        }

        public virtual string PostageTotal
        {
            get
            {
                if (postageTotal == null) return " - ";
                if (postageTotal.Phone) return "Phone";
                return postageTotal.Price.ToString("£0.00");
            }
        }

        public virtual string TotalWithPostage
        {
            get
            {
                if (postageTotal == null) return " - ";
                if (postageTotal.Phone) return "Phone";
                return (Total + postageTotal.Price).ToString("£0.00");
            }
        }

        public virtual PostageResult CalculatePostage(IQueryable<Postage> postages)
        {
            if (postages == null)
            {
                throw new ArgumentNullException("postages");
            }

            var postZone = Country.PostZone;

            var totalWeight = (int)BasketItems
                .Sum(bi => bi.TotalWeight);

            var postageToApply = postages
                .Where(p => totalWeight <= p.MaxWeight && p.IsActive)
                .OrderBy(p => p.MaxWeight)
                .FirstOrDefault();

            if (postageToApply == null) return postageTotal = PostageResult.WithDefault(postZone);

            var multiplier = postZone.Multiplier;
            var total = Math.Round(postageToApply.Price * multiplier, 2, MidpointRounding.AwayFromZero);

            return postageTotal = PostageResult.WithPrice(total);
        }

        public virtual void AddBasketItem(BasketItem basketItem)
        {
            basketItem.Basket = this;
            basketItems.Add(basketItem);
        }

        public virtual void RemoveBasketItem(BasketItem basketItem)
        {
            basketItem.Basket = null;
            basketItems.Remove(basketItem);
        }
    }
}
