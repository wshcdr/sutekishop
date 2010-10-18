using FluentNHibernate.Mapping;

namespace Suteki.Shop.Maps
{
    public class OrderAdjustmentMap : ClassMap<OrderAdjustment>
    {
        public OrderAdjustmentMap()
        {
            Id(x => x.Id);
            Map(x => x.Amount);
            Map(x => x.Description).Text();

            References(x => x.Order);
        }
    }
}