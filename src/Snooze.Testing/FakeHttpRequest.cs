using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace Snooze.Testing
{
    public class FakeHttpRequest : HttpRequestBase
    {

        public FakeHttpRequest(string url) : this()
        {
            var uri = new Uri(url, UriKind.RelativeOrAbsolute);

            if (!uri.IsAbsoluteUri)
            {
                uri = new Uri(new Uri("http://localhost"), uri);
            }

            SetUrl(uri);
        }

        public FakeHttpRequest(Uri uri) : this()
        {
            SetUrl(uri);
        }

        private void SetUrl(Uri uri)
        {
            _url = uri;
            if (uri != null)
            {
                _appRelativeCurrentExecutionFilePath = "~" + uri.AbsolutePath;
                _queryString = HttpUtility.ParseQueryString(uri.Query);
            }

        }


        public FakeHttpRequest()
        {
            SetUrl(new Uri("http://localhost"));
            _cookies = new HttpCookieCollection();
            _form = new NameValueCollection();
            _queryString = new NameValueCollection();
            _serverVariables = new NameValueCollection();
            _files = new FakeHttpFileCollection();
            _headers = new NameValueCollection();
            _context = new FakeHttpContext(this);
            _httpMethod = "GET";
        }

        public HttpCookieCollection _cookies { get; set; }
        public NameValueCollection _form { get; set; }
        public NameValueCollection _queryString { get; set; }
        public NameValueCollection _serverVariables { get; set; }
        public NameValueCollection _headers { get; set; }
        public string _appRelativeCurrentExecutionFilePath { get; set; }
        public Uri _url { get; set; }
        public Uri _urlReferrer { get; set; }
        public string _httpMethod { get; set; }
        public string[] _acceptTypes { get; set; }
        public HttpBrowserCapabilitiesBase _browser { get; set; }
        public string _anonymousID { get; set; }
        public HttpClientCertificate _clientCertificate { get; set; }
        public string _currentExecutionFilePath { get; set; }
        public string _filePath { get; set; }
        public FakeHttpFileCollection _files { get; set; }
        public FakeHttpContext _context { get; set; }
        public string _rawUrl { get; set; }
        public string _userAgent { get; set; }
        public string _userHostAddress { get; set; }
        public string _userHostName { get; set; }
        public string[] _userLanguages;
        public string _path { get; set; }
        public bool _isSecureConnection { get; set; }

        public override bool IsSecureConnection
        {
            get
            {
                return _isSecureConnection;
            }
        }

        public override string Path
        {
            get
            {
                return _path;
            }
        }

        public override string[] UserLanguages
        {
            get
            {
                return _userLanguages;
            }
        }

        public override string UserHostName
        {
            get
            {
                return _userHostName;
            }
        }

        public override string UserHostAddress
        {
            get
            {
                return _userHostAddress;
            }
        }

        public override string UserAgent
        {
            get
            {
                return _userAgent;
            }
        }

        public override string RequestType
        {
            get
            {
                return HttpMethod;
            }
            set
            {
                throw new NotImplementedException("you probably shouldn't be setting this");
            }
        }

        public override string RawUrl
        {
            get
            {
                return _rawUrl;
            }
        }

        public override NameValueCollection Params
        {
            get
            {

                var result = new NameValueCollection(64) {QueryString, Form};
                if (Cookies != null)
                    foreach (HttpCookie cookie in Cookies)
                    {
                        result.Add(cookie.Name, cookie.Value);
                    }
                result.Add(ServerVariables);
                return result;
            }

        }


        public override bool IsAuthenticated
        {
            get { return _context != null && _context.User != null && _context.User.Identity.IsAuthenticated; }
        }


        public override HttpFileCollectionBase Files
        {
            get
            {
                return _files;
            }
        }

        public override string FilePath
        {
            get
            {
                return _filePath;
            }
        }
        
        public override Encoding ContentEncoding { get; set; }
        public override string ContentType { get; set; }

        public override string this[string key]
        {
            get
            {
                if (_queryString != null)
                {
                    var result = _queryString[key];
                    if (result != null)
                        return result;
                }

                if (_form != null)
                {
                    var result = _form[key];
                    if (result != null)
                        return result;
                }

                if(_cookies!=null)
                {
                    var httpCookie = _cookies[key];
                    if (httpCookie != null)
                        return httpCookie.Value;
                }

                if(_serverVariables!=null)
                {
                    var result = _serverVariables[key];
                    if(result!=null)
                        return result;
                }

                return null;
            }
        }


        public override int ContentLength
        {
            get
            {
                throw new NotImplementedException("need to look at an input stream and length check etc");
            }
        }

        
        public override string CurrentExecutionFilePath
        {
            get
            {
                return _currentExecutionFilePath;
            }
        }

        public override HttpClientCertificate ClientCertificate
        {
            get
            {
                return _clientCertificate;
            }
        }

        public override string AnonymousID
        {
            get
            {
                return _anonymousID;
            }
        }

        public override NameValueCollection ServerVariables
        {
            get
            {
                return _serverVariables;
            }
        }

        public override NameValueCollection Headers
        {
            get
            {
                return _headers;
            }
        }

        public override string[] AcceptTypes
        {
            get
            {
                return _acceptTypes;
            }
        }

        public override HttpBrowserCapabilitiesBase Browser
        {
            get
            {
                return _browser;
            }
        }

        public override NameValueCollection Form
        {
            get
            {
                return _form;
            }
        }

        public override NameValueCollection QueryString
        {
            get
            {
                return _queryString;
            }
        }

        public override HttpCookieCollection Cookies
        {
            get
            {
                return _cookies;
            }
        }

        public override string AppRelativeCurrentExecutionFilePath
        {
            get
            {
                return _appRelativeCurrentExecutionFilePath;
            }
        }

        public override Uri Url
        {
            get
            {
                return _url;
            }
        }

        public override Uri UrlReferrer
        {
            get
            {
                return _urlReferrer;
            }
        }

        public override string PathInfo
        {
            get
            {
                return string.Empty;
            }
        }

        public override string ApplicationPath
        {
            get
            {
                return "";
            }
        }

        public override string HttpMethod
        {
            get
            {
                return _httpMethod;
            }
        }

     
    }
}