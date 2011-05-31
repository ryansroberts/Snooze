#region

using System.Web;

#endregion

namespace Snooze
{
    internal class FakeHttpRequest : HttpRequestBase
    {
        public override string ApplicationPath
        {
            get { return "/"; }
        }
    }
}