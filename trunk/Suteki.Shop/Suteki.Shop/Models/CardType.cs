using System.Collections.Generic;
using Suteki.Common.Models;

namespace Suteki.Shop
{
    public class CardType : IEntity
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual bool RequiredIssueNumber { get; set; }

        IList<Card> cards = new List<Card>();
        public virtual IList<Card> Cards
        {
            get { return cards; }
            set { cards = value; }
        }

        // these constants should match database table CardType.CardTypeId
        public const int VisaDeltaElectronId = 1;
        public const int MasterCardEuroCardId = 2;
        public const int AmericanExpressId = 3;
        public const int SwitchSoloMaestro = 4;
    }
}
