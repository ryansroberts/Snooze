using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;

namespace Snooze.Testing
{
    public class FakeHttpResponse : HttpResponseBase, IDisposable
    {
        public string _appPathModifier { get; set; }
        private MemoryStream outputStream = new MemoryStream();

        public FakeHttpResponse()
        {
            _headers = new NameValueCollection();
            _cookies = new HttpCookieCollection();
            _contentEncoding = Encoding.UTF8;
            _appPathModifier = string.Empty;
            _cachePolicy = new FakeCachePolicy();
            Output = new StreamWriter(outputStream);
        }

        

        public string ResponseOutput
        {
            get
            {
                Output.Flush();
                outputStream.Seek(0, SeekOrigin.Begin);
                return outputStream.ReadToEnd();
            }
        }


        public override void Write(string s)
        {
            Output.Write(s);
        }

        public FakeHttpContext _context { get; set; }
        public NameValueCollection _headers { get; set; }
        public HttpCookieCollection _cookies { get; set; }
        public bool _isClientConnected { get; set; }
        public bool _isRequestBeingRedirected { get; set; }
        public Encoding _contentEncoding { get; set; }
        public FakeCachePolicy _cachePolicy { get; set; }
        
        public override bool Buffer { get; set; }


        public override bool BufferOutput { get; set; }

        public override HttpCachePolicyBase Cache
        {
            get { return _cachePolicy; }
        }

        public override string CacheControl { get; set; }

        public override string Charset { get; set; }

        public override Encoding ContentEncoding
        {
            get
            {
                return _contentEncoding;
            }
            set
            {
                if(outputStream!=null)
                    outputStream.Close();
                outputStream = new MemoryStream();
                Output = new StreamWriter(outputStream, value);
                _contentEncoding = value; 
            }
        }

        public override string ContentType { get; set; }

        public override HttpCookieCollection Cookies
        {
            get { return _cookies; }
        }

        public override int Expires { get; set; }

        public override DateTime ExpiresAbsolute { get; set; }

        public override Stream Filter { get; set; }

        
        public override NameValueCollection Headers
        {
            get { return _headers; }
        }

        public override Encoding HeaderEncoding { get; set; }


        public override bool IsClientConnected
        {
            get { return _isClientConnected; }
        }

        public override bool IsRequestBeingRedirected
        {
            get { return _isRequestBeingRedirected; }
        }

        public override TextWriter Output { get; set; }

        public override Stream OutputStream
        {
            get { return outputStream; }
        }

        public override string RedirectLocation { get; set; }

        public override string Status { get; set; }

        public override int StatusCode { get; set; }

        public override string StatusDescription { get; set; }

        public override int SubStatusCode { get; set; }

        public override bool SuppressContent { get; set; }

        public override bool TrySkipIisCustomErrors { get; set; }

        public override void Redirect(string url)
        {
            RedirectInternal(url, true, false);
        }

        public override void Redirect(string url, bool endResponse)
        {
            RedirectInternal(url, endResponse, false);
        }


        public override void RedirectPermanent(string url)
        {
            RedirectInternal(url, true, true);
        }

        public override void RedirectPermanent(string url, bool endResponse)
        {
            RedirectInternal(url, endResponse, true);
        }

       

        internal void RedirectInternal(string url, bool endResponse, bool permanent)
        {

            if (url == null)
                throw new ArgumentNullException("url");
            if (url.IndexOf('\n') >= 0)
                throw new ArgumentException("Cannot redirect to newline");

            Clear();
            StatusCode = permanent ? 301 : 302;
            RedirectLocation = url;
            url = url.IndexOf(":", StringComparison.Ordinal) == -1 || url.StartsWith("http:", StringComparison.OrdinalIgnoreCase) || (url.StartsWith("https:", StringComparison.OrdinalIgnoreCase) || url.StartsWith("ftp:", StringComparison.OrdinalIgnoreCase)) || (url.StartsWith("file:", StringComparison.OrdinalIgnoreCase) || url.StartsWith("news:", StringComparison.OrdinalIgnoreCase)) ? HttpUtility.HtmlAttributeEncode(url) : HttpUtility.HtmlAttributeEncode(HttpUtility.UrlEncode(url));
            Write("<html><head><title>Object moved</title></head><body>\r\n");
            Write("<h2>Object moved to <a href=\"" + url + "\">here</a>.</h2>\r\n");
            Write("</body></html>\r\n");
            _isRequestBeingRedirected = true;
            
        }


        public override void AddHeader(string name, string value)
        {
            _headers.Add(name,value);
        }
      

        public override void Clear()
        {
            ClearHeaders();
            ClearContent();
        }

        public override void ClearHeaders()
        {
            _headers = new NameValueCollection();
        }

        public override void ClearContent()
        {
            if (outputStream != null)
                outputStream.Close();
            outputStream = new MemoryStream();
            Output = new StreamWriter(outputStream);
        }

        public override void Flush()
        {
            Output.Flush();
        }

        public void Dispose()
        {
            if (outputStream != null)
            {
                if (outputStream.CanRead)
                    outputStream.Close();
                outputStream = null;
            }
        }
    }
}