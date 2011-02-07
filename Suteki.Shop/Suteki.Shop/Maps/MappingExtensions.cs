using FluentNHibernate.Mapping.Builders;
using Suteki.Common.NHibernate;

namespace Suteki.Shop.Maps
{
    public static class MappingExtensions
    {
        public static PropertyBuilder Text(this PropertyBuilder propertyBuilder)
        {
            propertyBuilder.Length(10000);
            return propertyBuilder;
        }

        public static PropertyBuilder Money(this PropertyBuilder propertyBuilder)
        {
            propertyBuilder.CustomType<MoneyUserType>();
            return propertyBuilder;
        }
    }
}