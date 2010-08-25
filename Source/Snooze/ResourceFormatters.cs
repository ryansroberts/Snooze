#region

using System.Collections.Generic;

#endregion

namespace Snooze
{
    public static class ResourceFormatters
    {
        static readonly IList<IResourceFormatter> _formatters = new List<IResourceFormatter>();

        static ResourceFormatters()
        {
            // The order of formatters matters.

            // Browsers like Chrome will ask for "text/xml, application/xhtml+xml, ..."
            // But we don't want to use the XML formatter by default 
            // - which would happen since "text/xml" appears first in the list.
            // So we add an explicitly typed ViewFormatter first.

            _formatters.Add(new ViewFormatter("application/xhtml+xml"));
            _formatters.Add(new ViewFormatter("text/html"));
            _formatters.Add(new ViewFormatter("*/*")); // similar reason for this.
            _formatters.Add(new ViewFormatter("application/xml"));
            _formatters.Add(new JsonFormatter());
            //_formatters.Add(new XmlFormatter());
            _formatters.Add(new ViewFormatter());
            _formatters.Add(new StringFormatter());
            _formatters.Add(new ByteArrayFormatter());
        }

        public static IList<IResourceFormatter> Formatters
        {
            get { return _formatters; }
        }
    }
}