#region

using System;
using System.IO.Compression;
using System.Web;

#endregion

namespace Snooze.Modules
{
    public class CompressionModule : IHttpModule
    {
        #region IHttpModule Members

        public void Init(HttpApplication context)
        {
            context.PreRequestHandlerExecute += context_PreRequestHandlerExecute;
        }

        public void Dispose()
        {
        }

        #endregion

        void context_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            var context = HttpContext.Current;

            var acceptEncoding = context.Request.Headers["Accept-Encoding"];
            if (string.IsNullOrEmpty(acceptEncoding)) return;

            if (acceptEncoding.IndexOf("deflate", StringComparison.OrdinalIgnoreCase) >= 0 || acceptEncoding == "*")
            {
                context.Response.Filter = new DeflateStream(context.Response.Filter, CompressionMode.Compress);
                context.Response.AppendHeader("Content-Encoding", "deflate");
                context.Response.AppendHeader("Vary", "Content-Encoding");
            }
            else if (acceptEncoding.IndexOf("gzip", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                context.Response.Filter = new GZipStream(context.Response.Filter, CompressionMode.Decompress);
                context.Response.AppendHeader("Content-Encoding", "gzip");
                context.Response.AppendHeader("Vary", "Content-Encoding");
            }
        }
    }
}