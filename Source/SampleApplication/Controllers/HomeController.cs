using System.Web.Mvc;
using Snooze;

namespace SampleApplication.Controllers
{
    public class HomeUrl : Url { }

    public class HomeController : ResourceController
    {
        public ActionResult Get(HomeUrl url)
        {
            return OK(new HomeViewModel
            {
                BooksLink = new BooksUrl().ToString()
            });

            
        }
    }

    public class HomeViewModel
    {
        public Url Login { get; set; }
        public string BooksLink { get; set; }
    }
}
