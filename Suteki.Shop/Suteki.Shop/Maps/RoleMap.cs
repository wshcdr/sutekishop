using FluentNHibernate.Mapping;

namespace Suteki.Shop.Maps
{
    public class RoleMap : ClassMap<Role>
    {
        public RoleMap()
        {
            Id(x => x.Id);
            Map(x => x.Name);
            HasMany(x => x.Users);
        }
    }
}