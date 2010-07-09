using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using Snooze.Routing;

namespace Snooze
{

    static class HttpHandlerUtil
    {

        // Since Server.Execute() doesn't propagate HttpExceptions where the status code is
        // anything other than 500, we need to wrap these exceptions ourselves.
        public static IHttpHandler WrapForServerExecute(IHttpHandler httpHandler)
        {
            IHttpAsyncHandler asyncHandler = httpHandler as IHttpAsyncHandler;
            return (asyncHandler != null) ? new ServerExecuteHttpHandlerAsyncWrapper(asyncHandler) : new ServerExecuteHttpHandlerWrapper(httpHandler);
        }

        // Server.Execute() requires that the provided IHttpHandler subclass Page.
        internal class ServerExecuteHttpHandlerWrapper : Page
        {
            private readonly IHttpHandler _httpHandler;

            public ServerExecuteHttpHandlerWrapper(IHttpHandler httpHandler)
            {
                _httpHandler = httpHandler;
            }

            internal IHttpHandler InnerHandler
            {
                get
                {
                    return _httpHandler;
                }
            }

            public override void ProcessRequest(HttpContext context)
            {
                Wrap(() => _httpHandler.ProcessRequest(context));
            }

            protected static void Wrap(Action action)
            {
                Wrap(delegate
                {
                    action();
                    return (object)null;
                });
            }

            protected static TResult Wrap<TResult>(Func<TResult> func)
            {
                try
                {
                    return func();
                }
                catch (HttpException he)
                {
                    if (he.GetHttpCode() == 500)
                    {
                        throw; // doesn't need to be wrapped
                    }
                    throw new HttpException(500, "An exception occured on the execution of a partial request, see inner exception for details", he);
                }
            }
        }

        private sealed class ServerExecuteHttpHandlerAsyncWrapper : ServerExecuteHttpHandlerWrapper, IHttpAsyncHandler
        {
            private readonly IHttpAsyncHandler _httpHandler;

            public ServerExecuteHttpHandlerAsyncWrapper(IHttpAsyncHandler httpHandler)
                : base(httpHandler)
            {
                _httpHandler = httpHandler;
            }

            public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
            {
                return Wrap(() => _httpHandler.BeginProcessRequest(context, cb, extraData));
            }

            public void EndProcessRequest(IAsyncResult result)
            {
                Wrap(() => _httpHandler.EndProcessRequest(result));
            }
        }

    }

    public static class ParialRequestExtensions
    {
        public static string Render<TUrl>(this HtmlHelper htmlHelper,TUrl url) where TUrl : Url
        {
            var controllerType = ResourceControllerTypes.FindTypeForUrl<TUrl>();

            if (controllerType == null)
                throw new InvalidOperationException("Cannot find Controller for Route - ensure all configured Routes have matching Action defined.");

            var routeValues = GetRouteValues(controllerType, url);

            var routeData = CreateRouteData(ResourceRoute<TUrl>.Route(), routeValues,routeValues, htmlHelper.ViewContext);
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
            routeValues.Add("controller",controllerName);
            routeValues.Add("action", "dummy_value_to_please_mvc_implementation");
            return routeValues;
        }

        static void ExecuteRequest(HttpContextBase httpContext, RequestContext requestContext, TextWriter textWriter)
        {
            var handler = new MvcHandler(requestContext);
            httpContext.Server.Execute(HttpHandlerUtil.WrapForServerExecute(handler), textWriter, true);
        }

        private static RouteData CreateRouteData(RouteBase route, RouteValueDictionary routeValues, RouteValueDictionary dataTokens, ViewContext parentViewContext)
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