#region

using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Glue;

#endregion

namespace Snooze
{


    public class JsonFormatter : IResourceFormatter
    {
        #region IResourceFormatter Members

        public bool CanFormat(ControllerContext context, object resource, string mimeType)
        {
            return resource != null && mimeType == "application/json";
        }

        public void Output(ControllerContext context, object resource, string contentType)
        {
			var s = new JavaScriptSerializer();
			s.RegisterConverters(new[] { new UrlConverter() });
			var json = s.Serialize(resource);
	
			if(context.HttpContext.Request.QueryString["callback"] != null)
			{
				context.HttpContext.Response.ContentType = "application/javascript";
				context.HttpContext.Response.Output.Write(
					"{0}({1});", context.HttpContext.Request.QueryString["callback"], json);
			}
			else
			{
				context.HttpContext.Response.ContentType = "application/json";
				context.HttpContext.Response.Output.Write(json);	
			}
        }

        #endregion
    }
}