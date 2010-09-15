using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Suteki.Common.Models;

namespace Suteki.Common.Extensions
{
    public static class TypeExtensions
    {
        public static PropertyInfo GetPrimaryKey(this Type entityType)
        {
            if(!entityType.IsEntity())
            {
                throw new ApplicationException(string.Format("type {0} does not implement IEntity", entityType.Name));
            }
            return entityType.GetProperty("Id");
        }

        public static bool IsEntity(this Type type)
        {
            return typeof(IEntity).IsAssignableFrom(type);
        }

        public static IEnumerable<PropertyInfo> PropertiesWithAttributeOf(this Type type, Type attributeType)
        {
            return type.GetProperties().Where(property => property.HasAttributeOf(attributeType));
        }

        public static TAttribute GetAttributeOf<TAttribute>(this Type type)
        {
            var attributes = type.GetCustomAttributes(typeof (TAttribute), true);
            if(attributes.Length == 0) return default(TAttribute);
            return (TAttribute)attributes[0];
        }

        public static bool IsEnumerable(this Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        public static bool IsOrderable(this Type type)
        {
            return typeof (IOrderable).IsAssignableFrom(type);
        }

        public static bool IsProxy(this Type type)
        {
            return type.AssemblyQualifiedName.Contains("DynamicProxyGenAssembly2");
        }
    }
}