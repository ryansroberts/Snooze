using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO.Compression;

namespace Snooze.Modules
{
    public class CompressionModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.PreRequestHandlerExecute += new EventHandler(context_PreRequestHandlerExecute);
        }

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

        public void Dispose()
        {
        }
    }
}
