using System.Linq;
using NHibernate.Linq;
using NUnit.Framework;
using Suteki.Shop.Repositories;

namespace Suteki.Shop.Tests.Maps
{
    [TestFixture]
    public class ContentMapTests : MapTestBase
    {
        [SetUp]
        public void SetUp()
        {
            var menu1 = new Menu
            {
                Name = "menu1",
                IsActive = true,
                Position = 1
            };

            var text = new TextContent
            {
                Name = "text1",
                Text = "some text",
                IsActive = true,
                Position = 2
            };

            var action = new ActionContent
            {
                Name = "My Action",
                Controller = "controller",
                Action = "action",
                IsActive = true,
                Position = 3
            };

            var menu2 = new Menu
            {
                Name = "menu2",
                IsActive = true,
                Position = 4
            };

            InSession(session =>
            {
                session.Save(menu1);
                session.Save(text);
                session.Save(action);
                session.Save(menu2);
            });
        }

        [Test]
        public void Should_be_able_to_get_only_menus()
        {
            InSession(session =>
            {
                var menus = session.Query<Content>().Menus().AsEnumerable();
                menus.Count().ShouldEqual(2);
            });
        }
    }
}