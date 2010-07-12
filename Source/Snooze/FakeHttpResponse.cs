#region

using System.Web;

#endregion

namespace Snooze
{
    internal class FakeHttpResponse : HttpResponseBase
    {
        public override string ApplyAppPathModifier(string virtualPath)
        {
            return virtualPath;
        }
    }
}