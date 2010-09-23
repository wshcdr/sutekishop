using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Suteki.Common.Models;
using Suteki.Shop.Models.CustomDataAnnotations;
using Suteki.Common.Extensions;

namespace Suteki.Shop
{
    public class Order : IEntity
    {
        public virtual int Id { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [Email(ErrorMessage = "Not a valid email address")]
        [StringLength(250, ErrorMessage = "Email must not be longer than 250 characters")]
        public virtual string Email { get; set; }

        public virtual string AdditionalInformation { get; set; }
        public virtual bool UseCardHolderContact { get; set; }
        public virtual bool PayByTelephone { get; set; }
        public virtual DateTime CreatedDate { get; set; }
        public virtual DateTime DispatchedDate { get; set; }

        [StringLength(1000, ErrorMessage = "Note must not be longer than 1000 characters")]
        public virtual string Note { get; set; }

        public virtual bool ContactMe { get; set; }
        public virtual Card Card { get; set; }
        public virtual Contact CardContact { get; set; }
        public virtual Contact DeliveryContact { get; set; }
        public virtual OrderStatus OrderStatus { get; set; }
        public virtual User User { get; set; }
        public virtual Basket Basket { get; set; }

        private IList<OrderLine> orderLines = new List<OrderLine>();

        public virtual IList<OrderLine> OrderLines
        {
            get { return orderLines; }
            protected set { orderLines = value; }
        }

        public virtual Contact PostalContact
        {
            get
            {
                return UseCardHolderContact ? CardContact : DeliveryContact;
            }
        }

        public virtual string DispatchedDateAsString
        {
            get
            {
                if (IsDispatched) return DispatchedDate.ToShortDateString();
                return "&nbsp;";
            }
        }

        public virtual string UserAsString
        {
            get
            {
                if (User != null)
                {
                    return User.Email;
                }
                return "&nbsp;";
            }
        }

        public virtual bool IsCreated { get { return OrderStatus.Id == OrderStatus.CreatedId; } }
        public virtual bool IsDispatched { get { return OrderStatus.Id == OrderStatus.DispatchedId; } }
        public virtual bool IsRejected { get { return OrderStatus.Id == OrderStatus.RejectedId; } }

        // TODO: replace ids with object refs
        public virtual void UpdateBasket()
        {
            if (PostalContact.Country == null)
            {
                if (PostalContact.Country == null)
                {
                    throw new ApplicationException("PostalContact.Country is null");
                }
                Basket.Country.Id = PostalContact.Country.Id;
            }
            else
            {
                Basket.Country = PostalContact.Country;
            }
        }

        // TODO: replace ids with object refs
        public virtual int CardContactCountryId
        {
            get
            {
                return GetContactCountryId(CardContact);
            }
        }

        // TODO: replace ids with object refs
        public virtual int DeliveryContactCountryId
        {
            get
            {
                return GetContactCountryId(DeliveryContact);
            }
        }

        public virtual Money Total
        {
            get { return orderLines.Select(line => line.Total).Sum(); }
        }

        public virtual PostageResult Postage { get; set; }

        public virtual string PostageTotal
        {
            get
            {
                if (Postage == null) return " - ";
                if (Postage.Phone) return "Phone";
                return Postage.Price.ToStringWithSymbol();
            }
        }

        public virtual string TotalWithPostage
        {
            get
            {
                if (Postage == null) return " - ";
                if (Postage.Phone) return "Phone";
                return (Postage.Price + Total).ToStringWithSymbol();
            }
        }

        public virtual string PostageDescription
        {
            get
            {
                if (Postage == null) return "No postage calculated";
                return Postage.Description;
            }
        }

        private int GetContactCountryId(Contact contact)
        {
            if (contact == null || contact.Country == null || contact.Country.Id == 0)
            {
                if (Basket == null || Basket.Country == null) return 0;
                return Basket.Country.Id;
            }
            return contact.Country.Id;
        }

        public virtual void AddLine(string productName, int quantity, Money price)
        {
            if (productName == null)
            {
                throw new ArgumentNullException("productName");
            }
            if (quantity == 0)
            {
                throw new ArgumentException("quantity can not be zero");
            }

            var orderLine = new OrderLine
            {
                ProductName = productName,
                Quantity = quantity,
                Price = price,
                Order = this
            };

            OrderLines.Add(orderLine);
        }
    }
}
