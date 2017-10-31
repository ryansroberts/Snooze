#region

using System;
using System.IO;
using System.Web.Mvc;

#endregion

namespace Snooze
{

    public class StringFormatter : IResourceFormatter
    {
        #region IResourceFormatter Members

        public bool CanFormat(ControllerContext context, object resource, string mimeType)
        {
            if (resource != null && resource.GetType() == typeof (string)) return true;
            if (mimeType == "text/plain") return true;
            return false;
        }

        public void Output(ControllerContext context, object resource, string contentType)
        {
            var text = resource.ToString();
			context.HttpContext.Response.ContentType = contentType;
            context.HttpContext.Response.Output.Write(text);
        }

        #endregion


        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            return obj.GetType() == typeof(StringFormatter) ? 0 : -1;
        }
    }
}