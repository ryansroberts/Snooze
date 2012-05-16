using System.Web.Mvc;
using Snooze;

namespace SampleApplication.Controllers
{
    public class HomeUrl : Url { }
    public class Html5Url : Url { }
    public class LoginUrl : Url { }

    public class HomeController : ResourceController
    {
        public ActionResult Get(HomeUrl url)
        {

            return OK(new HomeViewModel
            {
                Login = new LoginUrl(),
                BooksLink = new BooksUrl().ToString()
            });
            
        }


        public ActionResult Get(Html5Url url)
        {

            return OK(new Html5ViewModel
            {
                Login = new LoginUrl(),
                BooksLink = new BooksUrl().ToString()
            });

        }

    }

    public class HomeViewModel
    {
        public LoginUrl Login { get; set; }
        public string BooksLink { get; set; }
    }

    public class Html5ViewModel
    {
        public LoginUrl Login { get; set; }
        public string BooksLink { get; set; }
    }
}
