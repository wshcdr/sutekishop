using System.Linq;

namespace Suteki.Shop.Repositories
{
	public static class ReviewRepositoryExtensions
	{
		public static IQueryable<Review> ForProduct(this IQueryable<Review> reviews, int productId)
		{
			return reviews.Where(x => x.Product.Id == productId);
		}

		public static IQueryable<Review> Approved(this IQueryable<Review> reviews)
		{
			return reviews.Where(x => x.Approved);
		}

		public static IQueryable<Review> Unapproved(this IQueryable<Review> reviews)
		{
			return reviews.Where(x => x.Approved == false);
		}
	}
}