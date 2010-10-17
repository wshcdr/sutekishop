namespace Suteki.Shop
{
    public class Comment : IComment
    {
        public virtual int Id { get; set; }
        public virtual bool Approved { get; set; }
        public virtual string Text { get; set; }
        public virtual string Reviewer { get; set; }
    }
}