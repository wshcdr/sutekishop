using System;
using System.Linq;
using System.Web.Mvc;
using MvcContrib;
using Suteki.Common.Binders;
using Suteki.Common.Filters;
using Suteki.Common.Repositories;
using Suteki.Common.Services;
using Suteki.Shop.Filters;
using Suteki.Shop.Repositories;
using Suteki.Shop.ViewData;

namespace Suteki.Shop.Controllers
{
	[ValidateInput(false)] //Html must be allowed in the Save actions. 
	public class CmsController : ControllerBase
	{
		private readonly IRepository<Content> contentRepository;
        private readonly IOrderableService<Content> contentOrderableService;

	    public CmsController(
            IRepository<Content> contentRepository, IOrderableService<Content> contentOrderableService)
		{
			this.contentRepository = contentRepository;
			this.contentOrderableService = contentOrderableService;
		}

		public override string GetControllerName()
		{
			return "";
		}

		//TODO: Possibly look at slimming down this action.
        [HttpGet, UnitOfWork]
        public ActionResult Index(string urlName)
		{
		    Content content;

		    try
		    {
		        content = string.IsNullOrEmpty(urlName)
		                      ? contentRepository.GetAll().DefaultText(null)
		                      : contentRepository.GetAll().WithUrlName(urlName);
		    }
		    catch (UrlNameNotFoundException)
		    {
		        return View("NotFound");
		    }

			if (content is Menu)
			{
				content = contentRepository.GetAll()
					.WithParent(content)
					.DefaultText(content as Menu);
			}

			if (content is ActionContent)
			{
				var actionContent = content as ActionContent;
				return RedirectToAction(actionContent.Action, actionContent.Controller);
			}

			AppendTitle(content.Name);

			if (content is TopContent)
			{
				return View("TopPage", CmsView.Data.WithContent(content));
			}

			return View("SubPage", CmsView.Data.WithContent(content));
		}

		[AdministratorsOnly]
        [HttpGet, UnitOfWork]
        public ActionResult Add(int id)
		{
		    var viewData = GetEditViewData(0);
			var parentContent = contentRepository.GetById(id);
			var textContent = TextContent.DefaultTextContent(parentContent, contentOrderableService.NextPosition);
			return View("Edit", viewData.WithContent(textContent));
		}

		[AdministratorsOnly, HttpPost, UnitOfWork]
		public ActionResult Add([EntityBind(Fetch = false)] TextContent content)
		{
			if(ModelState.IsValid)
			{
				contentRepository.SaveOrUpdate(content);
				Message = "Changes have been saved.";
				return this.RedirectToAction<MenuController>(c => c.List(content.ParentContent.Id));
			}

			return View("Edit", GetEditViewData(content.Id).WithContent(content));
		}

		[AdministratorsOnly]
        [HttpGet, UnitOfWork]
        public ActionResult EditText(int id)
		{
		    return EditContent(id);
		}

	    ActionResult EditContent(int id)
	    {
	        var viewData = GetEditViewData(id);
	        var content = contentRepository.GetById(id);
	        return View("Edit", viewData.WithContent(content));
	    }

	    [AdministratorsOnly, UnitOfWork, AcceptVerbs(HttpVerbs.Post)]
		public ActionResult EditText(TextContent content)
		{
		    return EditContent(content, "EditText");
		}

        ActionResult EditContent(Content content, string errorView)
	    {
	        if (ModelState.IsValid)
	        {
	            Message = "Changes have been saved.";
	            return this.RedirectToAction<MenuController>(c => c.List(content.ParentContent.Id));
	        }

	        //Error
            return View(errorView, GetEditViewData(content.Id).WithContent(content));
	    }

	    CmsViewData GetEditViewData(int contentId)
		{
			var menus = contentRepository.GetAll().NotIncluding(contentId).Menus().ToList();
			return CmsView.Data.WithMenus(menus);
		}

		[AdministratorsOnly, UnitOfWork]
		public ActionResult MoveUp(int id)
		{
			var content = contentRepository.GetById(id);

			contentOrderableService
				.MoveItemAtPosition(content.Position)
                .ConstrainedBy(c => c.ParentContent.Id == content.ParentContent.Id)
				.UpOne();

            return this.RedirectToAction<MenuController>(c => c.List(content.ParentContent.Id));
		}

		[AdministratorsOnly, UnitOfWork]
		public ActionResult MoveDown(int id)
		{
			var content = contentRepository.GetById(id);

			contentOrderableService
				.MoveItemAtPosition(content.Position)
                .ConstrainedBy(c => c.ParentContent.Id == content.ParentContent.Id)
				.DownOne();

            return this.RedirectToAction<MenuController>(c => c.List(content.ParentContent.Id));
		}

        [AdministratorsOnly]
        [HttpGet, UnitOfWork]
        public ActionResult EditTop(int id)
        {
            return EditContent(id);
        }

        [AdministratorsOnly, UnitOfWork, AcceptVerbs(HttpVerbs.Post)]
        public ActionResult EditTop(TopContent content)
	    {
            return EditContent(content, "EditTop");
        }
	}
}