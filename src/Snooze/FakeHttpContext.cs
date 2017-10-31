#region

using System.Collections;
using System.Collections.Generic;
using System.Web;

#endregion

namespace Snooze
{
    // Url uses RouteTable to generate its ToString() value.
    // So if there is no current HttpContext we will fake it using these.

    internal class FakeHttpContext : HttpContextBase
    {
        protected IDictionary items = new Dictionary<string, object>();

        public override System.Collections.IDictionary Items
        {
            get { return items; }
        }

        public override HttpRequestBase Request
        {
            get { return new FakeHttpRequest(); }
        }

        public override HttpResponseBase Response
        {
            get { return new FakeHttpResponse(); }
        }
    }
}