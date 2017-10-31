#region

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Routing;
using Snooze.ExpressionManipulation;

#endregion

namespace Snooze.Routing
{
    internal class ResourceRoute<TUrl> : Route
        where TUrl : Url
    {
        static readonly IDictionary<Type, Route> _registeredRoutes = new Dictionary<Type, Route>();

        public ResourceRoute(Expression<Func<TUrl, string>> routeExpression)
            : base(GetUrl(routeExpression), new MvcRouteHandler())
        {
            RouteExpression = routeExpression;

            var controllerType = ResourceControllerTypes.FindTypeForUrl<TUrl>();

            if (controllerType == null)
                throw new InvalidOperationException(
                    "Cannot find Controller for Route - ensure all configured Routes have matching Action defined.");

            var desc = new ReflectedControllerDescriptor(controllerType);
            var controllerName = desc.ControllerName;

            var defaultsProvider = new RouteDefaultsProvider<TUrl>(routeExpression);
            Defaults = new RouteValueDictionary(defaultsProvider.Defaults);
            Defaults.Add("controller", controllerName);
            Defaults.Add("action", "dummy_value_to_please_mvc_implementation");

            _registeredRoutes[typeof (TUrl)] = this;
        }

        protected internal virtual Expression<Func<TUrl, string>> RouteExpression { get; set; }

        public static Route Route()
        {
            if (!_registeredRoutes.ContainsKey(typeof (TUrl)))
                throw new InvalidOperationException("Route for " + typeof (TUrl).Name + " not configured");
            return _registeredRoutes[typeof (TUrl)];
        }

        static string GetUrl(Expression<Func<TUrl, string>> routeExpression)
        {
            var factory = new RouteUrlProvider<TUrl>(routeExpression);
            return factory.Url;
        }
    }

    internal static class ResourceRoute
    {
        public static string GetRouteNameFromUrlType(Type type)
        {
            Type resourceRouteType;
            if (type.IsDefined(typeof (SubUrlAttribute), true) && type.BaseType.IsGenericType)
            {
                resourceRouteType = GetSubResourceRouteType(type);
            }
            else
            {
                resourceRouteType = typeof (ResourceRoute<>).MakeGenericType(type);
            }
            return resourceRouteType.FullName;
        }

        static Type GetSubResourceRouteType(Type urlType)
        {
            var parentUrlType = urlType.BaseType.GetGenericArguments()[0];
            return typeof (SubResourceRoute<,>).MakeGenericType(urlType, parentUrlType);
        }
    }
}