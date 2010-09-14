using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using Suteki.Common.Models;
using Suteki.Common.Repositories;
using System.Web.Mvc.Html;

namespace Suteki.Common.HtmlHelpers
{
    public interface IComboFor<TEntity, TModel> where TEntity : INamedEntity
    {
        string BoundTo(Expression<Func<TModel, int>> propertyExpression, Expression<Func<TEntity, bool>> whereClause);
        string BoundTo(Expression<Func<TModel, int>> propertyExpression, string propertyNamePrefix);
        string BoundTo(Expression<Func<TModel, int>> propertyExpression);
        string BoundTo(Expression<Func<TModel, int>> propertyExpression, Expression<Func<TEntity, bool>> whereClause, string propertyNamePrefix);
    }

    public class ComboFor<TEntity, TModel> : IComboFor<TEntity, TModel>, IRequireHtmlHelper<TModel> where TEntity : class, INamedEntity
    {
        readonly IRepository<TEntity> repository;
        protected Expression<Func<TEntity, bool>> WhereClause { get; set; }
        protected string PropertyNamePrefix { get; set; }

        public ComboFor(IRepository<TEntity> repository)
        {
            this.repository = repository;
        }
        public string BoundTo(Expression<Func<TModel, int>> propertyExpression, Expression<Func<TEntity, bool>> whereClause, string propertyNamePrefix)
        {
            WhereClause = whereClause;
            PropertyNamePrefix = propertyNamePrefix;
            return BoundTo(propertyExpression);
        }

        public string BoundTo(Expression<Func<TModel, int>> propertyExpression, Expression<Func<TEntity, bool>> whereClause)
        {
            WhereClause = whereClause;
            return BoundTo(propertyExpression);
        }

        public string BoundTo(Expression<Func<TModel, int>> propertyExpression, string propertyNamePrefix)
        {
            PropertyNamePrefix = propertyNamePrefix;
            return BoundTo(propertyExpression);
        }

        public string BoundTo(Expression<Func<TModel, int>> propertyExpression)
        {
            var getPropertyValue = propertyExpression.Compile();
            var propertyName = (!String.IsNullOrEmpty(PropertyNamePrefix) ? PropertyNamePrefix : "") + Utils.ExpressionHelper.GetPropertyFromExpression(propertyExpression).Name;

            var viewDataModelIsNull = (!typeof(TModel).IsValueType) && HtmlHelper.ViewData.Model == null;
            var selectedId = viewDataModelIsNull ? 0 : getPropertyValue(HtmlHelper.ViewData.Model);
            return BuildCombo(selectedId, propertyName);
        }


        public override string ToString()
        {
            return BuildCombo(0, typeof(TEntity).Name + "Id");
        }

        protected virtual string BuildCombo(int selectedId, string htmlId)
        {
            if (HtmlHelper == null)
            {
                throw new SutekiCommonException("HtmlHelper is null");
            }
            return HtmlHelper.DropDownList(htmlId, GetSelectListItems(selectedId)).ToString();
        }

        protected IEnumerable<SelectListItem> GetSelectListItems(int selectedId)
        {
            var queryable = repository.GetAll();
            if (WhereClause != null)
            {
                queryable = queryable.Where(WhereClause);
            }
            var items = queryable.AsEnumerable()
                .OrderBy(e => e.Name)
                .Select(e => new SelectListItem { Selected = e.Id == selectedId, Text = e.Name, Value = e.Id.ToString() });

            var initialItem = new[]
            {
                new SelectListItem {Selected = selectedId == 0, Text = "<Please Select>", Value = "0"}
            };

            return initialItem.Union(items);
        }

        public HtmlHelper<TModel> HtmlHelper { get; set; }
    }
}