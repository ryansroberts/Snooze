using System.Web;

namespace Snooze
{
    // Url uses RouteTable to generate its ToString() value.
    // So if there is no current HttpContext we will fake it using these.

    class FakeHttpContext : HttpContextBase
    {
        public override HttpRequestBase Request
        {
            get
            {
                return new FakeHttpRequest();
            }
        }

        public override HttpResponseBase Response
        {
            get
            {
                return new FakeHttpResponse();
            }
        }
    }

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

    class FakeHttpResponse : HttpResponseBase
    {
        public override string ApplyAppPathModifier(string virtualPath)
        {
            return virtualPath;
        }
    }
}
