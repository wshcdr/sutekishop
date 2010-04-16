using System.Linq;
using Suteki.Common.Repositories;

namespace Suteki.Shop.Repositories
{
    public static class ContentRepositoryExtensions
    {
/*
        public static Menu GetMainMenu(this IRepository<Content> contentRepository)
        {
            // the static data setup should create a main menu content item with contentId = 1
            Menu mainMenu = contentRepository.GetById(1) as Menu;
            if (mainMenu == null)
            {
                throw new ApplicationException(
                    "Expected Content with ContentId = 1 to be ContentType menu. Check static data.");
            }
            return mainMenu; 
        }
*/

        public static IQueryable<Content> WithParent(this IQueryable<Content> contents, Content parent)
        {
            return contents.WithParent(parent.Id);
        }

        public static IQueryable<Content> WithParent(this IQueryable<Content> contents, int parentContentId)
        {
            return contents.Where(c => c.ParentContent.Id == parentContentId);
        }

        public static IQueryable<Content> WithAnyParent(this IQueryable<Content> contents)
        {
            return contents.Where(c => c.ParentContent != null);
        }

        public static Content DefaultText(this IQueryable<Content> contents, Menu menu)
        {
            return contents.Where(c => c is TextContent || c is TopContent).InOrder().FirstOrDefault() ?? new TextContent
            {
                ContentType = ContentType.TextContent,
                Name = "Default",
                Text = "No content has been created yet",
                IsActive = true,
                ParentContent = menu
            };
        }

        public static IQueryable<Menu> Menus(this IQueryable<Content> contents)
        {
            return contents.OfType<Menu>();
        }
    }
}
