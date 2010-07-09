using System.Reflection;
using System.Web.Mvc;
using System;

namespace Snooze
{
    public static class UrlHelperExtensions
    {
        /// <summary>
        /// Generates a static, versioned, URL for a given file path.
        /// If web.config is in debug mode then the URL includes current Tick count to force the browser to download again.
        /// </summary>
        /// <param name="path">Path to the file, relative to the current request.</param>
        public static string StaticFile(this UrlHelper url, string path)
        {
            return new StaticFileUrl
            {
                Path = path, 
                Version = url.RequestContext.HttpContext.IsDebuggingEnabled ?
                    "debug" + DateTime.UtcNow.Ticks :
                    Assembly.GetCallingAssembly().GetName().Version.ToString(4)
            }.ToString(url.RequestContext);
        }

        public static string IE6PngBehavior(this UrlHelper url)
        {
            return new IEPngFixUrl().ToString(url.RequestContext);
        }
    }
}
