#region

using System.Web.Mvc;

#endregion

namespace Snooze
{
    public class ViewFormatter : IResourceFormatter
    {
        readonly string _targetMimeType;

        public ViewFormatter()
        {
        }

        public ViewFormatter(string targetMimeType)
        {
            _targetMimeType = targetMimeType;
        }

        #region IResourceFormatter Members

        public bool CanFormat(ControllerContext context, object resource, string mimeType)
        {
            return ((_targetMimeType == mimeType) || ( string.IsNullOrEmpty(_targetMimeType)))
                   && FindView(context, resource).View != null;
        }

        public void Output(ControllerContext context, object resource, string contentType)
        {
            if (contentType != null)
            {
                context.HttpContext.Response.ContentType = contentType;
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

        #endregion

        ViewEngineResult FindView(ControllerContext context, object resource)
        {
            var viewName = GetViewName(resource);
            var result = ViewEngines.Engines.FindView(context, viewName, null);
            return result;
        }

        string GetViewName(object resource)
        {
            var name = resource.GetType().Name;
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