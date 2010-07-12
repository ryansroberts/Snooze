using System.Web;

namespace Snooze
{
    class FakeHttpResponse : HttpResponseBase
    {
        public override string ApplyAppPathModifier(string virtualPath)
        {
            return virtualPath;
        }
    }
}