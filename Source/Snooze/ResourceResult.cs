#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

#endregion

namespace Snooze
{
    public class ResourceResult : ActionResult
    {
        readonly List<Action<HttpCachePolicyBase>> _cacheActions = new List<Action<HttpCachePolicyBase>>();
        readonly List<HttpCookie> _cookies = new List<HttpCookie>();
        readonly List<KeyValuePair<string, object>> _headers = new List<KeyValuePair<string, object>>();

        public ResourceResult(int statusCode, object resource)
        {
            StatusCode = statusCode;

            Resource = resource;
        }

        public ILookup<string, object> Headers
        {
            get { return _headers.ToLookup(k => k.Key, v => v.Value); }
        }


        public int StatusCode { get; set; }

        public object Resource { get; set; }


        private bool _contentTypeExplicitlySet = false;

        protected string _contentType;
        public string ContentType
        {
            get
            {
                return this._contentType; 
            }
            set
            {
                this._contentType = value;
                _contentTypeExplicitlySet = !string.IsNullOrEmpty(this._contentType);
            }
        }

        public ResourceResult WithHeader(string name, Url value)
        {
            _headers.Add(new KeyValuePair<string, object>(name, value));

            return this;
        }


        public ResourceResult WithHeader(string name, string value)
        {
            _headers.Add(new KeyValuePair<string, object>(name, value));

            return this;
        }


        public ResourceResult WithCookie(HttpCookie cookie)
        {
            _cookies.Add(cookie);

            return this;
        }


        public ResourceResult WithCache(Action<HttpCachePolicyBase> action)
        {
            _cacheActions.Add(action);

            return this;
        }


        public ResourceResult AsJson()
        {
            ContentType = "application/json";

            return this;
        }


        public ResourceResult AsXml()
        {
            ContentType = "text/xml";

            return this;
        }


        public ResourceResult AsText()
        {
            ContentType = "text/plain";

            return this;
        }

        public ResourceResult AsXhtml()
        {
            ContentType = "application/xhtml+xml";

            return this;
        }

        public ResourceResult AsHtml()
        {
            ContentType = "text/html";

            return this;
        }


        public ResourceResult As(string type)
        {
            ContentType = type;

            return this;
        }


        public ResourceResult AsFile(string type)
        {
            ContentType = type;

            return this;
        }


        public ResourceResult AsFile(string type, string defaultFilename)
        {
            ContentType = type;

            return WithHeader("Content-Disposition", "attachment; filename=" + defaultFilename);
        }


        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.StatusCode = StatusCode;

            AppendHeaders(context);

            AppendCookies(context);

            ApplyCacheActions(context);


            if (Resource == null) return;


            // Delegate to ActionResult, if one was given.

            var innerResult = Resource as ActionResult;

            if (innerResult != null)
            {
                innerResult.ExecuteResult(context);

                return;
            }


            IEnumerable<string> acceptTypes = ParseAcceptTypes(context.HttpContext.Request.AcceptTypes);

            IResourceFormatter formatter = FindFormatter(context, acceptTypes);

            if (formatter == null)
            {
                if (Resource is string)
                {
                    context.HttpContext.Response.Output.Write(Resource);
                }

                else
                {
                    if( this._contentTypeExplicitlySet )
                    {
                        throw new HttpException(500, string.Format("Mime type explicitly set to '{0}' but unable to find a view that can format this type.",ContentType));
                    }
                    else
                    {
                        context.HttpContext.Response.StatusCode = 406; // not acceptable
                    }
                }

                return;
            }

            formatter.Output(context, Resource, ContentType);
        }


        IEnumerable<string> ParseAcceptTypes(IEnumerable<string> types)
        {
            // TODO process "q" and "level" options and sort accordingly by stealing code from openrasta

            if (types == null) return Enumerable.Empty<string>();

            return from type in types
                   let pos = type.IndexOf(';')
                   let length = pos >= 0 ? pos : type.Length
                   select type.Substring(0, length);
        }


        void AppendCookies(ControllerContext context)
        {
            foreach (HttpCookie cookie in _cookies)
            {
                context.HttpContext.Response.AppendCookie(cookie);
            }
        }


        void AppendHeaders(ControllerContext context)
        {
            foreach (var header in _headers)
            {
                context.HttpContext.Response.AppendHeader(header.Key, header.Value.ToString());
            }
        }


        void ApplyCacheActions(ControllerContext context)
        {
            foreach (var action in _cacheActions)
            {
                action(context.HttpContext.Response.Cache);
            }
        }


        IResourceFormatter FindFormatter(ControllerContext context, IEnumerable<string> acceptTypes)
        {
            if (ContentType != null) // Controller action forced the content type.
            {
                EnsureContentTypeIsMimeType();

                return ResourceFormatters.Formatters.FirstOrDefault(f => f.CanFormat(context, Resource, ContentType));
            }


            return (from formatter in ResourceFormatters.Formatters
                    from acceptType in acceptTypes
                    where formatter.CanFormat(context, Resource, acceptType)
                    select formatter).FirstOrDefault();
        }

        void EnsureContentTypeIsMimeType()
        {
            if (!ContentType.Contains('/')) // then it's probably a file extension.
            {
                string mimeType = MimeTypes.GetMimeTypeForExtension(ContentType);
                if (!string.IsNullOrEmpty(mimeType))
                    ContentType = mimeType;
            }
        }
    }
}