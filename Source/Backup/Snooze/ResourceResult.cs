using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Snooze
{
    public class ResourceResult : ActionResult
    {
        public ResourceResult(int statusCode, object resource)
        {
            StatusCode = statusCode;
            Resource = resource;
        }

        public int StatusCode { get; set; }
        public object Resource { get; set; }
        public string ContentType { get; set; }

        List<KeyValuePair<string, string>> _headers = new List<KeyValuePair<string, string>>();
        List<HttpCookie> _cookies = new List<HttpCookie>();
        List<Action<HttpCachePolicyBase>> _cacheActions = new List<Action<HttpCachePolicyBase>>();

        public ResourceResult WithHeader(string name, string value)
        {
            _headers.Add(new KeyValuePair<string, string>(name, value));
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

        public ResourceResult AsFile(string type, string defaultFilename )
        {
            ContentType = type;
            return this.WithHeader("Content-Disposition", "attachment; filename=" + defaultFilename);
        }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.StatusCode = StatusCode;
            AppendHeaders(context);
            AppendCookies(context);
            ApplyCacheActions(context);

            if (Resource == null) return;

            // Delegate to ActionResult, if one was given.
            ActionResult innerResult = Resource as ActionResult;
            if (innerResult != null)
            {
                innerResult.ExecuteResult(context);
                return;
            }

            var acceptTypes = ParseAcceptTypes(context.HttpContext.Request.AcceptTypes);
            var formatter = FindFormatter(context, acceptTypes);
            if (formatter == null)
            {
                if (Resource is string)
                {
                    context.HttpContext.Response.Output.Write(Resource);
                }
                else
                {
                    context.HttpContext.Response.StatusCode = 406; // not acceptable
                }
                return;
            }
            formatter.Output(context, Resource, ContentType);
        }

        IEnumerable<string> ParseAcceptTypes(IEnumerable<string> types)
        {
            // TODO process "q" and "level" options and sort accordingly
            if (types == null) return Enumerable.Empty<string>();
            return from type in types
                   let pos = type.IndexOf(';')
                   let length = pos >= 0 ? pos : type.Length
                   select type.Substring(0, length);
        }

        void AppendCookies(ControllerContext context)
        {
            foreach (var cookie in _cookies)
            {
                context.HttpContext.Response.AppendCookie(cookie);
            }
        }

        void AppendHeaders(ControllerContext context)
        {
            foreach (var header in _headers)
            {
                context.HttpContext.Response.AppendHeader(header.Key, header.Value);
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

            foreach (var formatter in ResourceFormatters.Formatters)
            {
                foreach (var acceptType in acceptTypes)
                {
                    if (formatter.CanFormat(context, Resource, acceptType)) return formatter;
                }
            }
            return null;
        }

        void EnsureContentTypeIsMimeType()
        {
            if (!ContentType.Contains('/')) // then it's probably a file extension.
            {
                ContentType = MimeTypes.GetMimeTypeForExtension(ContentType);
            }
        }
    }
}
