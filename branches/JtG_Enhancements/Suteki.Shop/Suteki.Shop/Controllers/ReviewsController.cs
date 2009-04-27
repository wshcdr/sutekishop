using System;
using System.Linq;
using System.Web.Mvc;
using Suteki.Common.Filters;
using Suteki.Common.Repositories;
using Suteki.Shop.Repositories;
using Suteki.Shop.ViewData;
using MvcContrib;
namespace Suteki.Shop.Controllers
{
	public class ReviewsController : ControllerBase
	{
		readonly IRepository<Review> reviewRepository;
		readonly IRepository<Product> productRepository;

		public ReviewsController(IRepository<Review> repository, IRepository<Product> productRepository)
		{
			this.reviewRepository = repository;
			this.productRepository = productRepository;
		}

		public ActionResult Show(int id)
		{
			var reviews = reviewRepository.GetAll().ForProduct(id).Approved().ToList();

			return View(new ReviewViewData 
			{
				ProductId = id,
                Reviews = reviews
			});
		}

		public ActionResult New(int id)
		{
			return View(new ReviewViewData
			{
				ProductId = id
			});
		}

		[AcceptVerbs(HttpVerbs.Post), UnitOfWork]
		public ActionResult New(int id, Review review)
		{
			review.ProductId = id;
			reviewRepository.InsertOnSubmit(review);
			return this.RedirectToAction(x => x.Submitted(id));
		}

		public ActionResult Submitted(int id)
		{
			return View(new ReviewViewData
			{
				Product = productRepository.GetById(id)
			});
		}
	}
}