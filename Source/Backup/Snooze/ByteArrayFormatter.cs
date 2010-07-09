using System.Web.Mvc;

namespace Snooze
{
    public class ByteArrayFormatter : IResourceFormatter
    {
        public bool CanFormat(ControllerContext context, object resource, string mimeType)
        {
            return resource != null && resource.GetType() == typeof(byte[]);
        }

        public void Output(ControllerContext context, object resource, string contentType)
        {
            context.HttpContext.Response.ContentType = contentType;
            context.HttpContext.Response.BinaryWrite((byte[])resource);
            context.HttpContext.Response.Flush();
        }
    }
}
