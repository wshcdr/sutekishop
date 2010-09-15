using System.ComponentModel.DataAnnotations;
using Suteki.Common.ViewData;

namespace Suteki.Shop.ViewData
{
    public class CheckoutViewData : ViewDataBase
    {
        public int OrderId { get; set; }
        public int BasketId { get; set; }

        // card contact

        [Required]
        public string CardContactFirstName { get; set; }

        [Required]
        public string CardContactLastName { get; set; }

        [Required]
        public string CardContactAddress1 { get; set; }

        public string CardContactAddress2 { get; set; }
        public string CardContactAddress3 { get; set; }
        public string CardContactTown { get; set; }
        public string CardContactCounty { get; set; }
        public string CardContactPostcode { get; set; }
        public Country CardContactCountry { get; set; }
        public string CardContactTelephone { get; set; }

        // email

        public string Email { get; set; }
        public string EmailConfirm { get; set; }

        // delivery contact

        public bool UseCardholderContact { get; set; }

        public string DeliveryContactFirstName { get; set; }
        public string DeliveryContactLastName { get; set; }
        public string DeliveryContactAddress1 { get; set; }
        public string DeliveryContactAddress2 { get; set; }
        public string DeliveryContactAddress3 { get; set; }
        public string DeliveryContactTown { get; set; }
        public string DeliveryContactCounty { get; set; }
        public string DeliveryContactPostcode { get; set; }
        public Country DeliveryContactCountry { get; set; }
        public string DeliveryContactTelephone { get; set; }

        // additional

        public string AdditionalInformation { get; set; }

        // card details

        public CardType CardCardType { get; set; }
        public string CardHolder { get; set; }
        public string CardNumber { get; set; }
        public string CardExpiryMonth { get; set; }
        public string CardExpiryYear { get; set; }
        public string CardStartMonth { get; set; }
        public string CardStartYear { get; set; }
        public string CardIssueNumber { get; set; }
        public string CardSecurityCode { get; set; }

        public bool PayByTelephone { get; set; }
    }
}