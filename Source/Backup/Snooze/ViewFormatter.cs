using System.Web.Mvc;

namespace Snooze
{
    public class ViewFormatter : IResourceFormatter
    {
        public ViewFormatter()
        {
        }

        public ViewFormatter(string targetMimeType)
        {
            _targetMimeType = targetMimeType;
        }

        string _targetMimeType;

        public bool CanFormat(ControllerContext context, object resource, string mimeType)
        {
            return ((_targetMimeType == mimeType) || (_targetMimeType == null)) 
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

            result.ViewEngine.ReleaseView(context,result.View);

        }

        private ViewEngineResult FindView(ControllerContext context, object resource)
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
