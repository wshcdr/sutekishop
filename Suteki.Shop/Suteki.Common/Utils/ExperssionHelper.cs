using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Suteki.Common.Utils
{
    public class ExpressionHelper
    {
        const string expressionNotProperty = "propertyExpression must be a property accessor. e.g: 'x => x.MyProperty'";

        public static PropertyInfo GetPropertyFromExpression<TModel>(Expression<Func<TModel, int>> propertyExpression)
        {
            return GetProperty(propertyExpression.Body);
        }

        public static PropertyInfo GetPropertyFromExpression<TModel>(Expression<Func<TModel, object>> propertyExpression)
        {
            return GetProperty(propertyExpression.Body);
        }

        static PropertyInfo GetProperty(Expression expression)
        {
            var memberExpression = expression as MemberExpression;
            if (memberExpression == null)
            {
                throw new ArgumentException(expressionNotProperty);
            }
            var property = memberExpression.Member as PropertyInfo;
            if (property == null)
            {
                throw new ArgumentException(expressionNotProperty);
            }
            return property;
        }
    }
}