namespace Suteki.Common.Models
{
    public class Money
    {
        public decimal Amount { get; private set; }

        public Money(decimal amount)
        {
            Amount = amount;
        }

        public override string ToString()
        {
            return Amount.ToString("0.00");
        }
    }
}