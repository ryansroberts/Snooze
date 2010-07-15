#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

#endregion

namespace Snooze
{
    /// <summary>
    ///   Find actions methods that match the HTTP method and has the requested Url type as the first parameter.
    /// </summary>
    public class ResourceActionInvoker : ControllerActionInvoker
    {
        static readonly Dictionary<string, MethodInfo> s_actionMethodCache = new Dictionary<string, MethodInfo>();

        protected override ActionDescriptor FindAction(ControllerContext controllerContext,
                                                       ControllerDescriptor controllerDescriptor, string actionName)
        {
            var urlType = GetUrlType(controllerContext);
            var httpMethod = GetHttpMethod(controllerContext);

            CheckChildAction(controllerContext.IsChildAction,controllerContext.Controller.GetType(),controllerContext.RequestContext.HttpContext.Request.Url.ToString());

            var methodInfo = GetMethodInfo(controllerContext.Controller.GetType(), urlType, httpMethod);
            if (methodInfo == null) return null;

            // Fix up the "action" name to be the Url type name (minus the "Url" suffix).
            // This makes for decent View names e.g. AuthorUrl => controller=Book, action=Author => /Views/Book/Author.aspx
            controllerContext.RouteData.Values["action"] = urlType.Name.Substring(0, urlType.Name.Length - 3);

            return new ReflectedActionDescriptor(methodInfo, httpMethod, controllerDescriptor);
        }

        void CheckChildAction(bool isChildaction,Type contollerType,string url)
        {
            if(!isChildaction && contollerType.Name.ToLower().StartsWith("Partial"))
                throw new HttpException(502,"This is a partial controller cannot execute a non partial request " + url);        
         }

        protected override ActionResult CreateActionResult(ControllerContext controllerContext,
                                                           ActionDescriptor actionDescriptor, object actionReturnValue)
        {
            if (!(actionReturnValue is ActionResult))
            {
                return new ResourceResult(200, actionReturnValue);
            }
            return base.CreateActionResult(controllerContext, actionDescriptor, actionReturnValue);
        }

        static MethodInfo GetMethodInfo(Type controllerType, Type urlType, string httpMethod)
        {
            var key = urlType.FullName + "!" + httpMethod;

            MethodInfo methodInfo = null;
            if (!s_actionMethodCache.TryGetValue(key, out methodInfo))
            {
                lock (s_actionMethodCache)
                {
                    if (!s_actionMethodCache.TryGetValue(key, out methodInfo))
                    {
                        methodInfo = FindActionMethod(controllerType, urlType, httpMethod);
                        if (methodInfo != null) s_actionMethodCache[key] = methodInfo;
                    }
                }
            }
            return methodInfo;
        }

        static MethodInfo FindActionMethod(Type controllerType, Type urlType, string httpMethod)
        {
            var methods =
                from m in controllerType.GetMethods()
                where m.Name.Equals(httpMethod, StringComparison.OrdinalIgnoreCase)
                let parameters = m.GetParameters()
                where parameters.Length > 0
                      && parameters[0].ParameterType.Equals(urlType)
                select m;

            return methods.FirstOrDefault();
        }

        static Type GetUrlType(ControllerContext controllerContext)
        {
            return controllerContext.RouteData.Route.GetType().GetGenericArguments()[0];
        }

        static string GetHttpMethod(ControllerContext controllerContext)
        {
            var methodInForm = controllerContext.HttpContext.Request.Form["_method"];
            var methodInHeader = controllerContext.HttpContext.Request.Headers["X-HTTP-Method-Override"];
            var methodInRequest = controllerContext.HttpContext.Request.HttpMethod;

            return methodInForm ?? methodInHeader ?? methodInRequest;
        }
    }
}