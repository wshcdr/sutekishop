using NHibernate.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Suteki.Shop.Tests.Maps;
using Suteki.Shop.Repositories;

namespace Suteki.Shop.Tests.Repositories
{
    [TestFixture]
    public class ContentRepositoryExtensionsTests : MapTestBase
    {
        Menu menu;

        [SetUp]
        public void SetUp()
        {
            var menuType = new ContentType { Id = 1, Name = "Menu"};
            var textType = new ContentType { Id = 2, Name = "TextContent"};
            var actionType = new ContentType { Id = 3, Name = "Action"};
            var topType = new ContentType { Id = 4, Name = "TopContent"};

            InSession(session =>
            {
                session.Save(menuType);
                session.Save(textType);
                session.Save(actionType);
                session.Save(topType);
            });

            menu = new Menu
            {
                ContentType = menuType, 
                Name = "Main", 
                IsActive = true, 
                Position = 1
            };
            var textContent = new TextContent
            {
                ContentType = textType, 
                Name = "Text", 
                Text = "Some text", 
                IsActive = true, 
                Position = 2,
                ParentContent = menu
            };
            var actionContent = new ActionContent
            {
                ContentType = actionType,
                Name = "Action",
                Controller = "HomeController",
                Action = "Index",
                IsActive = true,
                Position = 3,
                ParentContent = menu
            };
            var topContent = new TopContent
            {
                ContentType = topType,
                Name = "Top content",
                Text = "Some more text",
                IsActive = true,
                Position = 4,
                ParentContent = menu
            };

            InSession(session =>
            {
                session.Save(menu);
                session.Save(textContent);
                session.Save(actionContent);
                session.Save(topContent);
            });
        }

        [Test]
        public void DefaultText_should_return_textContent()
        {
            Content defaultText = null;
            InSession(session =>
            {
                defaultText = session.Query<Content>().DefaultText(menu);
            });

            defaultText.ShouldNotBeNull();
        }
    }
}