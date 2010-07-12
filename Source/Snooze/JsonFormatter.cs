using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Snooze
{
    public class JsonFormatter : IResourceFormatter
    {
        public bool CanFormat(ControllerContext context, object resource, string mimeType)
        {
            return resource != null && mimeType == "application/json";
        }

        public void Output(ControllerContext context, object resource, string contentType)
        {
            var s = new JavaScriptSerializer();
            s.RegisterConverters(new[] {new UrlConverter() });
            var json = s.Serialize(resource);
            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.Output.Write(json);
        }
    }
}
