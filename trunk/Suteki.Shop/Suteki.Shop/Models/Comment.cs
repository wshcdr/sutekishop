using System.ComponentModel.DataAnnotations;

namespace Suteki.Shop
{
    public class Comment : IComment
    {
        public virtual int Id { get; set; }
        public virtual bool Approved { get; set; }

        [Required(ErrorMessage = "Comment is required")]
        public virtual string Text { get; set; }

        [Required(ErrorMessage = "Your Name is required")]
        [StringLength(255, ErrorMessage = "Maximum length for your name is 255 characters.")]
        public virtual string Reviewer { get; set; }
    }
}