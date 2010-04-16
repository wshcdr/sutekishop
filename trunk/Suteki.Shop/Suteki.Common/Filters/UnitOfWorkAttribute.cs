using System.Web.Mvc;
using Castle.Facilities.NHibernateIntegration;
using NHibernate;

namespace Suteki.Common.Filters
{
	public class UnitOfWorkAttribute : FilterUsingAttribute
	{
		public UnitOfWorkAttribute() : base(typeof (UnitOfWorkFilter))
		{
		}
	}

	public class UnitOfWorkFilter : IActionFilter
	{
	    readonly ISessionManager sessionManager;
	    ITransaction transaction;

	    public UnitOfWorkFilter(ISessionManager sessionManager)
	    {
	        this.sessionManager = sessionManager;
	    }

	    public void OnActionExecuting(ActionExecutingContext filterContext)
		{
	        transaction = sessionManager.OpenSession().BeginTransaction();
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{
		    var thereWereNoExceptions = filterContext.Exception == null || filterContext.ExceptionHandled;
            if (filterContext.Controller.ViewData.ModelState.IsValid && thereWereNoExceptions)
			{
				transaction.Commit();
			}
			else
			{
			    transaction.Rollback();
			}
		}
	}
}