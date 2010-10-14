// ReSharper disable InconsistentNaming
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Suteki.Common.Repositories;
using Suteki.Common.TestHelpers;
using Suteki.Shop.ActionResults;
using Suteki.Shop.Controllers;
using Suteki.Shop.Tests.Repositories;
using Suteki.Shop.ViewData;

namespace Suteki.Shop.Tests.Controllers
{
	[TestFixture]
	public class ReviewsControllerTests
	{
		ReviewsController controller;
		IRepository<Review> repository;
		IRepository<Product> productRepository;
	    Product product;

		[SetUp]
		public void Setup()
		{
            product = new Product();
			repository = MockRepositoryBuilder.CreateReviewRepository();
            productRepository = new FakeRepository<Product>(id => 
            { 
                product.Id = id;
                return product; 
            });
            controller = new ReviewsController(repository, productRepository);
		}

		[Test]
		public void Show_should_get_reviews_for_product()
		{
			controller.Show(1)
				.ReturnsViewResult()
				.WithModel<ReviewViewData>()
				.AssertAreSame(productRepository.GetById(1), x => x.Product)
				.AssertAreEqual(2, x => x.Reviews.Count());
		}

		[Test]
		public void New_renders_view()
		{
			controller.New(5)
				.ReturnsViewResult()
				.WithModel<ReviewViewData>()
				.AssertAreSame(product, x => x.Product);
		}

		[Test]
		public void NewWithPost_saves_review()
		{
			var review = new Review();
			
			controller.New(5, review)
				.ReturnsRedirectToRouteResult()
				.ToAction("Submitted");

			review.Product.Id.ShouldEqual(5);

			repository.AssertWasCalled(x => x.SaveOrUpdate(review));
		}

		[Test]
		public void NewWithPost_renders_view_on_error()
		{
			controller.ModelState.AddModelError("foo", "bar");
			var review = new Review();

			controller.New(5, review)
				.ReturnsViewResult()
				.WithModel<ReviewViewData>()
				.AssertAreSame(review, x => x.Review)
				.AssertAreSame(product, x => x.Product);
		}

		[Test]
		public void Submitted_renders_view()
		{
			controller.Submitted(1)
				.ReturnsViewResult()
				.WithModel<ReviewViewData>()
				.AssertAreSame(product, x => x.Product);
		}

		[Test]
		public void Index_displays_unapproved_reviews()
		{
			controller.Index()
				.ReturnsViewResult()
				.WithModel<ReviewViewData>()
				.AssertAreEqual(2, x => x.Reviews.Count());
		}

		[Test]
		public void Approve_approves_review()
		{
			var review = new Review();
			repository.Expect(x => x.GetById(5)).Return(review);

			controller.Approve(5)
				.ReturnsRedirectToRouteResult()
				.ToAction("Index");

			review.Approved.ShouldBeTrue();
		}

		[Test]
		public void Delete_deletes_review()
		{
			var review = new Review();
			repository.Expect(x => x.GetById(5)).Return(review);

			controller.Delete(5)
				.ReturnsResult<RedirectToReferrerResult>();

			repository.AssertWasCalled(x => x.DeleteOnSubmit(review));
		}

	    [Test]
	    public void AllApproved_shows_all_reviews()
	    {
	        controller.AllApproved()
	            .ReturnsViewResult()
	            .ForView("AllApproved")
	            .WithModel<IEnumerable<Review>>()
	            .AssertAreEqual(3, vd => vd.Count());
	    }
	}
}
// ReSharper restore InconsistentNaming
