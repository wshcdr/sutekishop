using FluentNHibernate.Mapping;

namespace Suteki.Shop.Maps
{
    public static class MappingExtensions
    {
        public static PropertyPart Text(this PropertyPart propertyPart)
        {
            propertyPart.Length(10000);
            return propertyPart;
        }
    }
}