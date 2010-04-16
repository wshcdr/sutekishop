using System.Web;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using Suteki.Common.Binders;
using Suteki.Common.Repositories;
using Suteki.Common.Services;
using Suteki.Common.Validation;
using Suteki.Shop.Binders;
using System.Collections.Generic;
using System.Linq;
using Suteki.Shop.Services;

namespace Suteki.Shop.Tests.Binders
{
	[TestFixture]
	public class ProductBinderTester
	{
		ProductBinder productBinder;
		IModelBinder defaultBinder;
        FakeRepository<Product> productRepository;
	    IRepository<Category> categoryRepository;
		ControllerContext controllerContext;
		IHttpFileService fileService;
		IOrderableService<ProductImage> imageOrderableService;
		ModelBindingContext bindingContext;
		List<Image> images;
		ISizeService sizeService;
		static FakeValueProvider valueProvider;

		[SetUp]
		public void Setup()
		{
			images = new List<Image>();

		    productRepository = new FakeRepository<Product>();
		    categoryRepository = new FakeRepository<Category>(id => new Category {Id = id});

			fileService = MockRepository.GenerateStub<IHttpFileService>();
            imageOrderableService = MockRepository.GenerateStub<IOrderableService<ProductImage>>();
			sizeService = MockRepository.GenerateStub<ISizeService>();
			
			controllerContext = new ControllerContext
			{
				HttpContext = MockRepository.GenerateStub<HttpContextBase>()
			};

			controllerContext.HttpContext.Stub(x => x.Request).Return(MockRepository.GenerateStub<HttpRequestBase>());
			sizeService.Expect(x => x.WithValues(controllerContext.HttpContext.Request.Form)).Return(sizeService);

			valueProvider = new FakeValueProvider();
			bindingContext = new ModelBindingContext
			{
				ModelState = new ModelStateDictionary(),
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(Product)),
				ModelName = "product",
				ValueProvider = valueProvider
			};

            defaultBinder = MockRepository.GenerateStub<IModelBinder>();
            defaultBinder.Stub(b => b.BindModel(controllerContext, bindingContext))
                .Return(new Product { Id = 0, Name = "foo" }).Repeat.Any();

            productBinder = new ProductBinder(defaultBinder, productRepository, categoryRepository, fileService, imageOrderableService, sizeService);
        }

		[Test]
		public void ShouldAddErrorToModelstateWhenNameNotUnique()
		{
            fileService.Stub(x => x.GetUploadedImages(null)).IgnoreArguments().Return(images);

            productRepository.EntitesToReturnFromGetAll.Add(new Product { Id = 5, Name = "foo" });

			var product = productBinder.BindModel(controllerContext, bindingContext);

		    product.ShouldNotBeNull("Product is null");
			bindingContext.ModelState["product.ProductId"].ShouldNotBeNull("Expected model state error");
		}

		[Test]
		public void ShouldUpdateImages()
		{
            fileService.Stub(x => x.GetUploadedImages(null)).IgnoreArguments().Return(images);

			images.Add(new Image());
			images.Add(new Image());

			var product = (Product)productBinder.BindModel(controllerContext, bindingContext);

            product.ShouldNotBeNull("Product is null");
			product.ProductImages.Count.ShouldEqual(2);
			product.ProductImages.First().Image.ShouldBeTheSameAs(images[0]);
			product.ProductImages.Last().Image.ShouldBeTheSameAs(images[1]);
			product.ProductImages.First().Position.ShouldEqual(0);
			product.ProductImages.Last().Position.ShouldEqual(1);
		}

	    [Test]
	    public void Validation_errors_thrown_by_file_service_should_be_added_to_model_state()
	    {
	        fileService.Stub(f => f.GetUploadedImages(null)).IgnoreArguments().Throw(
	            new ValidationException("Image upload failed"));
            productBinder.BindModel(controllerContext, bindingContext);

            bindingContext.ModelState["validation_error_0"].Errors[0].ErrorMessage.ShouldEqual("Image upload failed");

            fileService.AssertWasCalled(f => f.GetUploadedImages(Arg<HttpRequestBase>.Is.Anything, Arg<string[]>.Is.Anything));
	    }

		[Test]
		public void ShouldUpdateSizes()
		{
            fileService.Stub(x => x.GetUploadedImages(null)).IgnoreArguments().Return(images);

			productBinder.BindModel(controllerContext, bindingContext);
			sizeService.AssertWasCalled(x => x.Update(Arg<Product>.Is.Anything));
		}

		[Test]
		public void ShouldUpdateCategories()
		{
            fileService.Stub(x => x.GetUploadedImages(null)).IgnoreArguments().Return(images);

			valueProvider.AddValue("categories", new[] { 1, 2 }, "1,2");
			var product = (Product)productBinder.BindModel(controllerContext, bindingContext);

			product.ProductCategories.Count().ShouldEqual(2);
			product.ProductCategories.First().Category.Id.ShouldEqual(1);
            product.ProductCategories.Last().Category.Id.ShouldEqual(2);
		}


		[Test]
		public void AddsModelStateErrorWhenNoCategories()
		{
            fileService.Stub(x => x.GetUploadedImages(null)).IgnoreArguments().Return(images);

			productBinder.BindModel(controllerContext, bindingContext);
			bindingContext.ModelState["categories"].Errors.Count.ShouldEqual(1);
		}
	}
}