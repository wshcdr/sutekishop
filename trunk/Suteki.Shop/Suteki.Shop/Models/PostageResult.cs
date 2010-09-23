using Suteki.Common.Models;

namespace Suteki.Shop
{
    public class PostageResult
    {
        public virtual Money Price { get; set; }
        public virtual bool Phone { get; set; }
        public virtual string Description { get; set; }

        public static PostageResult WithPhone { get { return new PostageResult { Phone = true }; } }
        
        public static PostageResult WithPrice(Money price, string description)
        {
            return new PostageResult { Phone = false, Price = price, Description = description };
        }

        public static PostageResult WithDefault(PostZone postZone)
        {
            if (postZone.AskIfMaxWeight)
            {
                return WithPhone;
            }
            else
            {
                return WithPrice(postZone.FlatRate, string.Empty);
            }
        }
    }
}
