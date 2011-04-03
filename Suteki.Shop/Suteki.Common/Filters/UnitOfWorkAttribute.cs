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
	    private readonly IPerActionTransactionStore perActionTransactionStore;

	    public UnitOfWorkFilter(ISessionManager sessionManager, IPerActionTransactionStore perActionTransactionStore)
	    {
	        this.sessionManager = sessionManager;
	        this.perActionTransactionStore = perActionTransactionStore;
	    }

	    public void OnActionExecuting(ActionExecutingContext filterContext)
	    {
            var sesion = sessionManager.OpenSession();
            perActionTransactionStore.StoreTransaction(filterContext, sesion.BeginTransaction());
	    }

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{
		    var transaction = perActionTransactionStore.RetrieveTransaction(filterContext);
            if (transaction == null) return;

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

    public interface IPerActionTransactionStore
    {
        void StoreTransaction(ActionExecutingContext filterContext, ITransaction transaction);
        ITransaction RetrieveTransaction(ActionExecutedContext filterContext);
    }

    public class PerActionTransactionStore : IPerActionTransactionStore
    {
        private const string transactionToken = "__transaction__";

        public void StoreTransaction(ActionExecutingContext filterContext, ITransaction transaction)
        {
            var controllerActionName =
                transactionToken +
                filterContext.Controller.GetType().Name +
                "." +
                filterContext.ActionDescriptor.ActionName;
            filterContext.RequestContext.HttpContext.Items[controllerActionName] = transaction;
        }

        public ITransaction RetrieveTransaction(ActionExecutedContext filterContext)
        {
            var controllerActionName =
                transactionToken +
                filterContext.Controller.GetType().Name +
                "." +
                filterContext.ActionDescriptor.ActionName;

            return filterContext.RequestContext.HttpContext.Items[controllerActionName] as ITransaction;
        }
    }
}