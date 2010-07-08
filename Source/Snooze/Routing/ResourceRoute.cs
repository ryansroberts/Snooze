using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Routing;
using Snooze.ExpressionManipulation;

namespace Snooze.Routing
{
    class ResourceRoute<TUrl> : Route
        where TUrl : Url
    {
        public ResourceRoute(Expression<Func<TUrl, string>> routeExpression)
            : base(GetUrl(routeExpression), new MvcRouteHandler())
        {
            RouteExpression = routeExpression;

            var controllerType = ResourceControllerTypes.FindTypeForUrl<TUrl>();

            // NICE / SH - Check if controller type is null. If it is it means a route has been defined
            // but matching Action has not been defined in a Controller (and Controller contains no other snooze routes)
            // Throw more meaningful Exception
            if (controllerType == null)
                throw new ApplicationException("Cannot find Controller for Route - ensure all configured Routes have matching Action defined.");
            // NICE / SH - End

            var desc = new ReflectedControllerDescriptor(controllerType);
            var controllerName = desc.ControllerName;
                        
            var defaultsProvider = new RouteDefaultsProvider<TUrl>(routeExpression);
            Defaults = new RouteValueDictionary(defaultsProvider.Defaults);
            Defaults.Add("controller", controllerName);
            Defaults.Add("action", "dummy_value_to_please_mvc_implementation");
        }

        static string GetUrl(Expression<Func<TUrl, string>> routeExpression)
        {
            var factory = new RouteUrlProvider<TUrl>(routeExpression);
            return factory.Url;
        }

        protected internal virtual Expression<Func<TUrl, string>> RouteExpression { get; set; }

    }

    static class ResourceRoute
    {
        public static string GetRouteNameFromUrlType(Type type)
        {
            Type resourceRouteType;
            if (type.IsDefined(typeof(SubUrlAttribute), true))
            {
                resourceRouteType = GetSubResourceRouteType(type);
            }
            else
            {
                resourceRouteType = typeof(ResourceRoute<>).MakeGenericType(type);
            }
            return resourceRouteType.FullName;
        }

        static Type GetSubResourceRouteType(Type urlType)
        {
            var parentUrlType = urlType.BaseType.GetGenericArguments()[0];
            return typeof(SubResourceRoute<,>).MakeGenericType(urlType, parentUrlType);
        }
    }
}
