using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Suteki.Common.Extensions;
using Suteki.Common.Repositories;
using Suteki.Common.Services;
using Suteki.Common.Validation;
using Suteki.Shop.Services;

namespace Suteki.Shop.Binders
{
	public class ProductBinder : IModelBinder
	{
	    readonly IModelBinder defaultBinder;
	    readonly IRepository<Product> productRepository;
	    readonly IRepository<Category> categoryRepository;
	    readonly IHttpFileService httpFileService;
		readonly IOrderableService<ProductImage> orderableService;
		readonly ISizeService sizeService;

		public ProductBinder(
            IModelBinder defaultBinder, 
            IRepository<Product> productRepository, 
            IRepository<Category> categoryRepository, 
            IHttpFileService httpFileService, 
            IOrderableService<ProductImage> orderableService, 
            ISizeService sizeService)
		{
		    this.defaultBinder = defaultBinder;
		    this.productRepository = productRepository;
		    this.categoryRepository = categoryRepository;
		    this.httpFileService = httpFileService;
			this.orderableService = orderableService;
			this.sizeService = sizeService;
		}

		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
		    if (bindingContext.ModelType != typeof (Product))
		    {
		        throw new ApplicationException(string.Format(
                    "ProductBinder can only be used to bind Product. Attempting to bind a {0}", 
                    bindingContext.ModelType.Name));
		    }

            var product = defaultBinder.BindModel(controllerContext, bindingContext) as Product;

			if(product != null)
			{
				UpdateImages(product, controllerContext.HttpContext.Request, bindingContext);
				CheckForDuplicateNames(product, bindingContext);
				UpdateSizes(controllerContext.HttpContext.Request.Form, product);
				UpdateCategories(product, bindingContext);
			}
	
			return product;
		}

        void UpdateImages(Product product, HttpRequestBase request, ModelBindingContext bindingContext)
	    {
            IEnumerable<Image> images = null;
            if(Validator.ValidateFails(bindingContext.ModelState, ()=> 
                images = httpFileService.GetUploadedImages(request, ImageDefinition.ProductImage, ImageDefinition.ProductThumbnail)
            )) return;

            var position = orderableService.NextPosition;
            foreach (var image in images)
            {
                product.AddProductImage(image, position);
                position++;
            }
	    }

	    void CheckForDuplicateNames(Product product, ModelBindingContext bindingContext)
	    {
	        if (string.IsNullOrEmpty(product.Name)) return;
	        
            var productWithNameAlreadyExists =
	            productRepository.GetAll().Any(x => x.Id != product.Id && x.Name == product.Name);

	        if (!productWithNameAlreadyExists) return;

	        var key = bindingContext.ModelName + ".ProductId";
	        bindingContext.ModelState.AddModelError(key, "Product names must be unique and there is already a product called '{0}'".With(product.Name));
	    }

	    void UpdateSizes(NameValueCollection form, Product product)
	    {
	        sizeService.WithValues(form).Update(product);
	    }

	    void UpdateCategories(Product product, ModelBindingContext context)
		{
			product.ProductCategories.Clear();

			var valueProviderResult = context.ValueProvider.GetValue("categories");
			
			if(valueProviderResult != null)
			{
				var categoryIds = valueProviderResult.ConvertTo(typeof(int[])) as int[];

				if(categoryIds != null)
				{
					foreach (var category in categoryIds.Select(categoryId => categoryRepository.GetById(categoryId)))
					{
					    product.AddCategory(category);
					}
				}
			}

			if(product.ProductCategories.Count == 0)
			{
				context.ModelState.AddModelError("categories", "Please select at least 1 category for the product.");
			}
		}
	}
}