using System.Collections.Generic;
using Suteki.Common.Models;

namespace Suteki.Shop
{
    public class ContentType : IEntity
    {
        public const int MenuId = 1;
        public const int TextContentId = 2;
        public const int ActionContentId = 3;
        public const int TopContentId = 4;

        public virtual int Id { get; set; }
        public virtual string Name { get; set; }

        IList<Content> contents = new List<Content>();

        public virtual IList<Content> Contents
        {
            get { return contents; }
            set { contents = value; }
        }

        // TODO: is this really going to work with NH?
        public static ContentType Menu
        {
            get
            {
                return new ContentType
                {
                    Id = MenuId
                };
            }
        }

        // TODO: is this really going to work with NH?
        public static ContentType TextContent
        {
            get
            {
                return new ContentType
                {
                    Id = TextContentId
                };
            }
        }
    }
}
