using System.Web.Mvc;
using Snooze;

namespace SampleApplication.Controllers
{
    public class MentalUrl : Url
    {
        public string brains { get; set; }
    }

    public class MentalController : ResourceController
    {
        public ActionResult Get(MentalUrl url)
        {
            return OK(url.brains);
        }
    }
}