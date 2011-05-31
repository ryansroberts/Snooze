#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Mvc;

#endregion

namespace Snooze
{
    public abstract class BaseViewFormatter : IResourceFormatter
    {
        protected string _targetMimeType;

        protected BaseViewFormatter(string targetMimeType)
        {
            _targetMimeType = targetMimeType;
        }

        public bool CanFormat(ControllerContext context, object resource, string mimeType)
        {
            return ((_targetMimeType == mimeType) || ( string.IsNullOrEmpty(_targetMimeType)))
                   && FindView(context, resource).View != null;
        }

        public void Output(ControllerContext context, object resource, string contentType)
        {
            if (!context.Controller.GetType().Name.StartsWith("Partial"))
            {
                SetContentType(context, contentType);
            }

            var result = FindView(context, resource);
            if (result.View != null)
            {
                context.Controller.ViewData.Model = resource;

                result.View.Render(
                    new ViewContext(
                        context,
                        result.View,
                        context.Controller.ViewData,
                        new TempDataDictionary(),
                        context.HttpContext.Response.Output
                        ),
                    context.HttpContext.Response.Output
                    );
            }

            result.ViewEngine.ReleaseView(context, result.View);
        }

        private void SetContentType(ControllerContext context, string contentType)
        {
            if (_targetMimeType == "*/*") //Oh dear 
                context.HttpContext.Response.ContentType = "text/html";
            else
                context.HttpContext.Response.ContentType = contentType ?? _targetMimeType;
        }

        private ViewEngineResult FindView(ControllerContext context, object resource)
        {
            var viewName = GetViewName(resource);
            var result = ViewEngines.Engines.FindView(context, viewName, null);
            if(result.View == null)
                Trace.WriteLine("Could not locate view with name " + viewName);
            return result;
        }

        protected abstract string GetViewName(object resource);
    }

    public class ExplicitNameViewFormatter : BaseViewFormatter
    {
        private readonly string viewname;

        public ExplicitNameViewFormatter(string targetMimeType,string viewname) : base(targetMimeType)
        {
            this.viewname = viewname;
        }

        protected override string GetViewName(object resource)
        {
            return viewname;
        }
    }


    



    public class ResourceTypeConventionViewFormatter : BaseViewFormatter
    {
        public ResourceTypeConventionViewFormatter(string targetMimeType) : base(targetMimeType)
        {
        }

        protected override string GetViewName(object resource)
        {
            var name = resource.GetType().Name;
            if (resource.GetType().IsGenericType)
                name = resource.GetType().GetGenericArguments()[0].Name;

            if (name.EndsWith("ViewModel"))
            {
                name = name.Substring(0, name.Length - "ViewModel".Length);
            }
            else if (name.EndsWith("Model"))
            {
                name = name.Substring(0, name.Length - "Model".Length);
            }
            return name;
        }
    }
}