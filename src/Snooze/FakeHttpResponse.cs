#region

using System.Collections.Specialized;
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

		public override void AddHeader(string name, string value)
		{
			
		}

		public override void AppendHeader(string name, string value)
		{
		}
		public override void AppendCookie(HttpCookie cookie)
		{
		}

		public override NameValueCollection Headers
		{
			get
			{
				return new NameValueCollection();
			}
		}
    }
}