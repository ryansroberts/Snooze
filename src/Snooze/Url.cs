#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Web.Routing;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Snooze.Routing;

#endregion

namespace Snooze
{
    /// <summary>
    ///   Base class for strongly-typed URLs parameters.
    /// </summary>
    public abstract class Url : IXmlSerializable
    {
        static readonly Dictionary<Type, IEnumerable<Action<Url, RouteValueDictionary>>> s_propertyPushersCache =
            new Dictionary<Type, IEnumerable<Action<Url, RouteValueDictionary>>>();

        readonly IEnumerable<Action<Url, RouteValueDictionary>> _propertyPushers;

        readonly static IList<Func<PropertyInfo, bool>> _preventMapping = new List<Func<PropertyInfo, bool>>();


        public Url()
        {
            _propertyPushers = GetOrCreatePropertyPushers(GetType());
        }

        #region IXmlSerializable Members

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteString(ToString());
        }

        #endregion

        public static void DoNotMapToUrlWhere(Func<PropertyInfo, bool> predicate)
        {
            _preventMapping.Add(predicate);
        }

        static IEnumerable<Action<Url, RouteValueDictionary>> GetOrCreatePropertyPushers(Type urlType)
        {
            IEnumerable<Action<Url, RouteValueDictionary>> propertyGetters = null;
            if (s_propertyPushersCache.TryGetValue(urlType, out propertyGetters) == false)
            {
                lock (s_propertyPushersCache)
                {
                    if (s_propertyPushersCache.TryGetValue(urlType, out propertyGetters) == false)
                    {
                        propertyGetters = CreatePropertyPushers(urlType).ToArray();
                        s_propertyPushersCache[urlType] = propertyGetters;
                    }
                }
            }
            return propertyGetters;
        }

        public static Func<PropertyInfo, bool> And<T>(IEnumerable<Func<PropertyInfo, bool>> predicates)
        {
            return item => predicates.All(predicate => predicate(item));
        }

        static IEnumerable<Action<Url, RouteValueDictionary>> CreatePropertyPushers(Type urlType)
        {
            //mm: removed the 'declared only' binding flag and added a filter to exclude properties of type Url ( for sub urls ) 
            var properties = urlType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => !p.PropertyType.IsSubclassOf(typeof(Url)));

            //properties = properties.Where(p => !_preventMapping.Any(v => v(p)));

            properties = _preventMapping.Aggregate(properties, (current, exclude) => current.Where(p => !exclude(p)));
            var indexer = typeof (RouteValueDictionary).GetProperty("this");
            var addMethod = typeof(RouteValueDictionary).GetMethod("Add");

            var setMethod = typeof(RouteValueDictionary).GetProperty("Item").GetSetMethod();

            return properties.Select(property => CreatePropertyPusher(urlType, setMethod, property)).Select(lambda => lambda.Compile());
        }

        static Expression<Action<Url, RouteValueDictionary>> CreatePropertyPusher(Type urlType, MethodInfo setMethod, PropertyInfo property)
        {
            var url = Expression.Parameter(typeof (Url), "url");
            var values = Expression.Parameter(typeof(RouteValueDictionary), "values");

            var typedUrl = Expression.TypeAs(url, urlType);
            Expression getPropertyValue = Expression.Property(typedUrl, property);
            if (property.PropertyType.IsValueType)
            {
                // need to box the value type.
                getPropertyValue = Expression.TypeAs(getPropertyValue, typeof (object));
            }

            var addValue = Expression.Call(values, setMethod, Expression.Constant(property.Name, typeof (string)), getPropertyValue);

            return Expression.Lambda<Action<Url, RouteValueDictionary>>(addValue, url, values);
        }

        public override string ToString()
        {
            return ToString(CreateMinimalRequestContext());
        }

        public string ToString(RequestContext requestContext)
        {
            var values = new RouteValueDictionary();
            FillRouteValueDictionary(values);
            var name = ResourceRoute.GetRouteNameFromUrlType(GetType());

            VirtualPathData vp;
            try
            {
                foreach (var value in values.ToList().Where(value => value.Value is string[]))
                {
                    values[value.Key] = String.Join(",", (string[])value.Value);
                }

                vp = RouteTable.Routes.GetVirtualPath(requestContext, name, values);
            }
            catch (ArgumentException)
            {
                return GetType().Name + "-NotConfigured";
            }
			if(vp == null)
			{
				return "No route for " + GetType().Name + " name: " + name + " values: " + string.Join(",", values.Keys.ToArray());
			}

            return vp.VirtualPath;
        }

        protected internal virtual void FillRouteValueDictionary(RouteValueDictionary values)
        {
            foreach (var pushPropertyValue in _propertyPushers)
            {
                pushPropertyValue(this, values);
            }
        }

        static RequestContext CreateMinimalRequestContext()
        {
            if (HttpContext.Current != null)
            {
                return new RequestContext(new HttpContextWrapper(HttpContext.Current), new RouteData());
            }
            else
            {
                return new RequestContext(new FakeHttpContext(), new RouteData());
            }
        }
    }
}