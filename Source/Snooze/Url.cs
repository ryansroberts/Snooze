#region

using System;
using System.Collections.Generic;
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

        static IEnumerable<Action<Url, RouteValueDictionary>> CreatePropertyPushers(Type urlType)
        {
            var properties =
                urlType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            var addMethod = typeof (RouteValueDictionary).GetMethod("Add");
            foreach (var property in properties)
            {
                var lambda = CreatePropertyPusher(urlType, addMethod, property);
                yield return lambda.Compile();
            }
        }

        static Expression<Action<Url, RouteValueDictionary>> CreatePropertyPusher(Type urlType, MethodInfo addMethod,
                                                                                  PropertyInfo property)
        {
            var url = Expression.Parameter(typeof (Url), "url");
            var values = Expression.Parameter(typeof (RouteValueDictionary), "values");

            var typedUrl = Expression.TypeAs(url, urlType);
            Expression getPropertyValue = Expression.Property(typedUrl, property);
            if (property.PropertyType.IsValueType)
            {
                // need to box the value type.
                getPropertyValue = Expression.TypeAs(getPropertyValue, typeof (object));
            }

            var addValue = Expression.Call(values, addMethod, Expression.Constant(property.Name, typeof (string)),
                                           getPropertyValue);

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
            var vp = RouteTable.Routes.GetVirtualPath(requestContext, name, values);
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