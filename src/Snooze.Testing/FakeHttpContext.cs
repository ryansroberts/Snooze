using System;
using System.Collections;
using System.Security.Principal;
using System.Web;
using System.Web.Caching;
using System.Web.Profile;

namespace Snooze.Testing
{
    public class FakeHttpContext : HttpContextBase
    {

        public FakeHttpContext(FakeHttpRequest request) : this(request,new FakeHttpResponse())
        {
        }

        public FakeHttpContext(string path)
            : this(new FakeHttpRequest(path))
        {
        }

        public FakeHttpContext(Uri uri)
            : this(new FakeHttpRequest(uri))
        {
        }

        public FakeHttpContext(FakeHttpRequest request, FakeHttpResponse response)
        {
            _response = response;
            _request = request;
            _request._context = this;
            _response._context = this;
            _cache = new Cache();
            _allErrors = new Exception[0];
            _items = new Hashtable(); 
            User = new GenericPrincipal(new GenericIdentity(string.Empty),new string[0]);
        }

        

        private FakeHttpRequest __request;
        public FakeHttpRequest _request { get { return __request; } set { __request = value;
            __request._context = this;
        } }

        public override HttpRequestBase Request
        {
            get { return _request; }
        }

        public FakeHttpResponse _response { get; set; }
        public override HttpResponseBase Response
        {
            get { return _response; }
        }


        public Exception[] _allErrors { get; set; }

        public override Exception[] AllErrors
        {
            get { return _allErrors; }
        }

        public virtual HttpApplicationStateBase _application { get; set; }
        public override HttpApplicationStateBase Application
        {
            get { return _application; }
        }

        public override HttpApplication ApplicationInstance { get; set; }

        public virtual Cache _cache { get; set; }
        public override Cache Cache
        {
            get { return _cache; }
        }

        public IHttpHandler _currentHandler { get; set; }
        public override IHttpHandler CurrentHandler
        {
            get { return _currentHandler; }
        }

        public RequestNotification _currentNotification { get; set; }
        public override RequestNotification CurrentNotification
        {
            get { return _currentNotification; }
        }

        public Exception _error { get; set; }
        public override Exception Error
        {
            get { return _error; }
        }

        public override IHttpHandler Handler { get; set; }

        public bool _isCustomErrorEnabled { get; set; }
        public override bool IsCustomErrorEnabled
        {
            get { return _isCustomErrorEnabled; }
        }

        public bool _isDebuggingEnabled { get; set; }
        public override bool IsDebuggingEnabled
        {
            get { return _isDebuggingEnabled; }
        }

        public bool _isPostNotification { get; set; }
        public override bool IsPostNotification
        {
            get { return _isPostNotification; }
        }

        public IDictionary _items { get; set; }
        public override IDictionary Items
        {
            get { return _items; }
        }

        public IHttpHandler _previousHandler { get; set; }
        public override IHttpHandler PreviousHandler
        {
            get { return _previousHandler; }
        }

        public ProfileBase _profile { get; set; }
        public override ProfileBase Profile
        {
            get { return _profile; }
        }

        public HttpServerUtilityBase _server { get; set; }
        public override HttpServerUtilityBase Server
        {
            get { return _server; }
        }

        public HttpSessionStateBase _session { get; set; }
        public override HttpSessionStateBase Session
        {
            get { return _session; }
        }

        public override bool SkipAuthorization { get; set; }

        public DateTime _timestamp { get; set; }
        public override DateTime Timestamp
        {
            get { return _timestamp; }
        }

        public TraceContext _trace { get; set; }
        public override TraceContext Trace
        {
            get { return _trace; }
        }

        public override IPrincipal User { get; set; }

       
    }
}