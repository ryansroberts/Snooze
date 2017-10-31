#region

using System.Collections.Generic;
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
        public static List<JsonConverter> JsonConverters = new List<JsonConverter> { new UrlConverter() };

        #region IResourceFormatter Members

        public bool CanFormat(ControllerContext context, object resource, string mimeType)
        {
            return resource != null && mimeType == "application/json";
        }

        public void Output(ControllerContext context, object resource, string contentType)
        {

            var json = JsonConvert.SerializeObject(resource, JsonConverters.ToArray());
	
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

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            return obj.GetType() == typeof(JsonFormatter) ? 0 : -1;
        }
    }
}