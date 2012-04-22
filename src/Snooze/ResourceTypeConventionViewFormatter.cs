#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

			if(result.ViewEngine != null)
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
                Trace.WriteLine("Could not locate view with name " + viewName + " looked in" + string.Join("\r\n",result.SearchedLocations.ToArray()));
            return result;
        }

        protected abstract string GetViewName(object resource);


        public abstract int CompareTo(object obj);
    }

    public class ExplicitNameViewFormatter : BaseViewFormatter
    {
        private readonly string viewname;

        public ExplicitNameViewFormatter(string targetMimeType, string viewname) : base(targetMimeType)
        {
            this.viewname = viewname;
        }

        protected override string GetViewName(object resource)
        {
            return viewname;
        }

        public override int CompareTo(object obj)
        {
            if (obj == null)
                return 1;
            var asme = obj as ExplicitNameViewFormatter;
            if(asme==null)
                return -1;

            return _targetMimeType == asme._targetMimeType && viewname == asme.viewname  ? 0 : -1;
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
                name = name.Substring(0, name.Length - 9);
            }
            else if (name.EndsWith("Model"))
            {
                name = name.Substring(0, name.Length - 5);
            }
            else if (name.EndsWith("Command"))
            {
                name = name.Substring(0, name.Length - 7);
            }
            else if (name.EndsWith("Url"))
            {
                name = name.Substring(0, name.Length - 3);
            }
            return name;
        }


        public override int CompareTo(object obj)
        {
            if (obj == null)
                return 1;
            var asme = obj as ResourceTypeConventionViewFormatter;
            if (asme == null)
                return -1;

            return _targetMimeType == asme._targetMimeType? 0 : -1;
        }
    }
}