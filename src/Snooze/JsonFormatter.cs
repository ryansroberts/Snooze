#region

using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Glue;
using Newtonsoft.Json;

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

            var json = JsonConvert.SerializeObject(resource, new JsonConverter[]{new UrlConverter()});
	
			if(context.HttpContext.Request.QueryString["callback"] != null)
			{
				context.HttpContext.Response.ContentType = "application/javascript";
				context.HttpContext.Response.Output.Write(
					"{0}({1});", context.HttpContext.Request.QueryString["callback"], json);
			}
			else
			{
				context.HttpContext.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Access-Control-Allow-Origin, Access-Control-Allow-Headers, Accept");
				context.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", "*");
				context.HttpContext.Response.ContentType = "application/json";
				context.HttpContext.Response.Output.Write(json);	
			}
        }

        #endregion
    }
}