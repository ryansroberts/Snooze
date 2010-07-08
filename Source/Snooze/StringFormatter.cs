using System.Web.Mvc;

namespace Snooze
{
    public class StringFormatter : IResourceFormatter
    {
        public bool CanFormat(ControllerContext context, object resource, string mimeType)
        {
            if (resource != null && resource.GetType() == typeof(string)) return true;
            if (mimeType == "text/plain") return true;
            return false;
        }

        public void Output(ControllerContext context, object resource, string contentType)
        {
            var text = resource.ToString();
            context.HttpContext.Response.ContentType = "text/plain";
            context.HttpContext.Response.Output.Write(text);
        }
    }
}
