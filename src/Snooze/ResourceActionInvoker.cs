#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

#endregion

namespace Snooze
{
    public class NoMethodActionDescriptor : ActionDescriptor {
        public readonly string httpMethod;

        public NoMethodActionDescriptor(string httpMethod)
        {
            this.httpMethod = httpMethod;
        }

        public override object Execute(ControllerContext controllerContext, IDictionary<string, object> parameters)
        { 
            throw new NotImplementedException();
        }

        public override ParameterDescriptor[] GetParameters()
        {
            return new ParameterDescriptor[] {};
        }

        public override string ActionName
        {
            get { return httpMethod; }
        }

        public override ControllerDescriptor ControllerDescriptor
        {
            get { return null; }
        }
    }

    /// <summary>
    ///   Find actions methods that match the HTTP method and has the requested Url type as the first parameter.
    /// </summary>
    public class ResourceActionInvoker : ControllerActionInvoker
    {
        static readonly Dictionary<string, MethodInfo> s_actionMethodCache = new Dictionary<string, MethodInfo>();

        protected override ActionResult InvokeActionMethod(ControllerContext controllerContext,
                                                           ActionDescriptor actionDescriptor,
                                                           IDictionary<string, object> parameters)
        {
            if (actionDescriptor is NoMethodActionDescriptor)
                return new ResourceResult<object>(400, ((NoMethodActionDescriptor) actionDescriptor).httpMethod);

            return Result = base.InvokeActionMethod(controllerContext, actionDescriptor, parameters);
        }


        public ActionResult Result { get; protected set; }


        protected override ActionDescriptor FindAction(ControllerContext controllerContext,
                                                       ControllerDescriptor controllerDescriptor, string actionName)
        {
            var urlType = GetUrlType(controllerContext);

            var httpMethod = GetHttpMethod(controllerContext);


            CheckChildAction(IsSnoozePartial(controllerContext), controllerContext.Controller.GetType(),
                             controllerContext.RequestContext.HttpContext.Request.Url.ToString());

            var methodInfo = GetMethodInfo(controllerContext.Controller.GetType(), urlType, httpMethod);

            // Fix up the "action" name to be the Url type name (minus the "Url" suffix).
            // This makes for decent View names e.g. AuthorUrl => controller=Book, action=Author => /Views/Book/Author.aspx

            controllerContext.RouteData.Values["action"] = urlType.Name.Substring(0, urlType.Name.Length - 3);

            if (methodInfo == null)
                return new NoMethodActionDescriptor(httpMethod);

            return new ReflectedActionDescriptor(methodInfo, httpMethod, controllerDescriptor);
        }


        void CheckChildAction(bool isChildaction, Type contollerType, string url)
        {
            if (isChildaction && !contollerType.Name.ToLower().StartsWith("partial"))

                throw new HttpException(400, "This partial controller cannot execute a non partial request " + url);
        }


        protected override ActionResult CreateActionResult(ControllerContext controllerContext,
                                                           ActionDescriptor actionDescriptor, object actionReturnValue)
        {
            if (!(actionReturnValue is ActionResult))
            {
                return new ResourceResult<object>(200, actionReturnValue);
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


        public static MethodInfo FindActionMethod(Type controllerType, Type urlType, string httpMethod)
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
            if (IsSnoozePartial(controllerContext))

                return "GET";


            var methodInForm = controllerContext.HttpContext.Request.Form["_method"];

            var methodInHeader = controllerContext.HttpContext.Request.Headers["X-HTTP-Method-Override"];

            var methodInRequest = controllerContext.HttpContext.Request.HttpMethod;


            return methodInForm ?? methodInHeader ?? methodInRequest ?? "GET";
        }


        static bool IsSnoozePartial(ControllerContext controllerContext)
        {
            return controllerContext.RouteData.Values.ContainsKey("snooze_partial");
        }
    }
}