#region

using System.Web.Mvc;

#endregion

namespace Snooze
{
    public class ByteArrayFormatter : IResourceFormatter
    {
        #region IResourceFormatter Members

        public bool CanFormat(ControllerContext context, object resource, string mimeType)
        {
            return resource != null && resource.GetType() == typeof (byte[]);
        }

        public void Output(ControllerContext context, object resource, string contentType)
        {
            context.HttpContext.Response.ContentType = contentType;
            context.HttpContext.Response.BinaryWrite((byte[]) resource);
            context.HttpContext.Response.Flush();
        }

        #endregion

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            return obj.GetType() == typeof(ByteArrayFormatter) ? 0 : -1;
        }
    }
}