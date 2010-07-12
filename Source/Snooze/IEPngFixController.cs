using System.Reflection;
using System.Web.Mvc;
using System;

namespace Snooze
{
    public class IEPngFixController : ResourceController
    {
        public ActionResult Get(IEPngFixUrl url)
        {
            SetFarFutureExpires();
            return new FileStreamResult(Assembly.GetExecutingAssembly().GetManifestResourceStream("Snooze.Resources.iepngfix.htc"), "text/x-component");
        }

        public ActionResult Get(BlankGifUrl url)
        {
            SetFarFutureExpires();
            return new FileStreamResult(Assembly.GetExecutingAssembly().GetManifestResourceStream("Snooze.Resources.blank.gif"), "image/gif");
        }
        
        void SetFarFutureExpires()
        {
            Response.Cache.SetExpires(DateTime.Now.AddYears(10));
        }
    }
}
