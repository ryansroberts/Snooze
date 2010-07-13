#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Linq.Expressions;
using Snooze.Routing;

#endregion

namespace Snooze
{
    public static class ParialRequestExtensions
    {
        public static string PartialFor<TUrl,TEnum>(this HtmlHelper htmlHelper,IEnumerable<TEnum> items,Func<TEnum,TUrl> f) where TUrl : Url
        {
            return items.Aggregate(new StringBuilder(),(b,i) => b.Append(htmlHelper.PartialFor(f(i))))
                .ToString();
        }

        public static string PartialFor<TUrl>(this HtmlHelper htmlHelper, TUrl url) where TUrl : Url
        {
            var controllerType = ResourceControllerTypes.FindTypeForUrl<TUrl>();

            if (controllerType == null)
                throw new InvalidOperationException(
                    "Cannot find Controller for Route - ensure all configured Routes have matching Action defined.");

            var routeValues = GetRouteValues(controllerType, url);

            var routeData = CreateRouteData(ResourceRoute<TUrl>.Route(), routeValues, routeValues,
                                            htmlHelper.ViewContext);
            var httpContext = htmlHelper.ViewContext.HttpContext;
            var requestContext = new RequestContext(httpContext, routeData);

            var writer = new StringWriter();
            ExecuteRequest(httpContext, requestContext, writer);

            return writer.GetStringBuilder().ToString();
        }

        static RouteValueDictionary GetRouteValues<TUrl>(Type controllerType, TUrl url)
        {
            var desc = new ReflectedControllerDescriptor(controllerType);
            var controllerName = desc.ControllerName;

            var routeValues = new RouteValueDictionary(url);
            routeValues.Add("controller", controllerName);
            routeValues.Add("action", "dummy_value_to_please_mvc_implementation");
            return routeValues;
        }

        static void ExecuteRequest(HttpContextBase httpContext, RequestContext requestContext, TextWriter textWriter)
        {
            var handler = new MvcHandler(requestContext);
            httpContext.Server.Execute(HttpHandlerUtil.WrapForServerExecute(handler), textWriter, true);
        }

        static RouteData CreateRouteData(RouteBase route, RouteValueDictionary routeValues,
                                         RouteValueDictionary dataTokens, ViewContext parentViewContext)
        {
            var routeData = new RouteData();

            foreach (var kvp in routeValues)
            {
                routeData.Values.Add(kvp.Key, kvp.Value);
            }

            foreach (var kvp in dataTokens)
            {
                routeData.DataTokens.Add(kvp.Key, kvp.Value);
            }

            routeData.Route = route;
            routeData.DataTokens["ParentActionViewContext"] = parentViewContext;
            return routeData;
        }
    }
}