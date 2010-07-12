using System.Web;

namespace Snooze
{
    class FakeHttpRequest : HttpRequestBase
    {
        public override string ApplicationPath
        {
            get
            {
                return "/";
            }
        }
    }
}