using Castle.Core.Configuration;
using Castle.Facilities.NHibernateIntegration;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions.Helpers;
using NHibernate.Cfg;
using Suteki.Shop.Maps;

namespace Suteki.Shop.Repositories
{
    public class FluentNHibernateConfigurationBuilder : IConfigurationBuilder
    {
        public Configuration GetConfiguration(IConfiguration facilityConfiguration)
        {
            return Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2005.ConnectionString(c => c
                    .Server(@"localhost\sqlexpress")
                    .Database("SutekiShop")
                    .TrustedConnection()))
                .Mappings(ConfigureMappings)
                .BuildConfiguration();
        }

        public static void ConfigureMappings(MappingConfiguration mappingConfiguration)
        {
            mappingConfiguration.FluentMappings
                .AddFromAssembly(typeof (ProductMap).Assembly)
                .Conventions.Add(
                    ForeignKey.EndsWith("Id"),
                    PrimaryKey.Name.Is(x => x.EntityType.Name + "Id"),
                    DefaultCascade.None());
        }
    }
}