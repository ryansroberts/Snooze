﻿#region

using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

#endregion

namespace Snooze
{
    public class XmlFormatter : IResourceFormatter
    {
        #region IResourceFormatter Members

        public bool CanFormat(ControllerContext context, object resource, string mimeType)
        {
            return resource != null && mimeType.Contains("/xml");
        }

        public void Output(ControllerContext context, object resource, string contentType)
        {
            if (resource is XNode)
            {
                using (var w = XmlWriter.Create(context.HttpContext.Response.Output))
                {
                    ((XNode) resource).WriteTo(w);
                }
            }
            else
            {
                var s = new XmlSerializer(resource.GetType());
                if (!context.Controller.GetType().Name.StartsWith("Partial"))
                {
                    context.HttpContext.Response.ContentType = contentType ?? "text/xml";
                }
                s.Serialize(context.HttpContext.Response.Output, resource);
            }
        }

        #endregion

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            return obj.GetType() == typeof (XmlFormatter) ? 0 : -1;
        }
    }
}