using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Snooze;

namespace SampleApplication.Controllers
{

    public class PartialResult
    {
        public string Something { get; set;}
    }

    public class PartialItemUrl : Url
    {
        public string Something { get; set; }
    }

    public class PartialItemController : ResourceController
    {
        public ResourceResult Get(PartialItemUrl url)
        {
            return OK(new PartialResult {Something = url.Something});
        }
    }
}