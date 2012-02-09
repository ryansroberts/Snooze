#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Glue;

#endregion

namespace Snooze
{
	public abstract class ResourceResult : ActionResult
	{
		protected readonly List<Action<HttpContextBase, HttpCachePolicyBase>> _cacheActions = new List<Action<HttpContextBase,HttpCachePolicyBase>>();
		readonly List<HttpCookie> _cookies = new List<HttpCookie>();
		protected readonly List<KeyValuePair<string, object>> _headers = new List<KeyValuePair<string, object>>();
		protected bool _contentTypeExplicitlySet;
		protected string _contentType;
		protected Func<object,object> projection = o => o; 
		
	
		public object Resource { get; set; }

		public ILookup<string, object> Headers
		{
			get { return _headers.ToLookup(k => k.Key, v => v.Value); }
		}

		public int StatusCode { get; set; }

		public string ContentType
		{
			get { return _contentType; }
			set
			{
				_contentType = value;
				_contentTypeExplicitlySet = !string.IsNullOrEmpty(_contentType);
			}
		}

		public IList<HttpCookie> Cookies
		{
			get { return _cookies; }
		}

		public IEnumerable<Action<HttpContextBase,HttpCachePolicyBase>> CacheActions
		{
			get { return _cacheActions; }
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
			Cookies.Add(cookie);

			return this;
		}

		public ResourceResult WithCache(Action<HttpContextBase, HttpCachePolicyBase> action)
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

		

	}

	public class ResourceResult<T> : ResourceResult
    {
		public ResourceResult(int statusCode, object resource)
        {
            StatusCode = statusCode;

            Resource = resource;
        }


		public new ResourceResult<T> WithHeader(string name, Url value)
        {
            _headers.Add(new KeyValuePair<string, object>(name, value));

            return this;
        }


        public new ResourceResult<T> WithHeader(string name, string value)
        {
            _headers.Add(new KeyValuePair<string, object>(name, value));

            return this;
        }


        public new ResourceResult<T> WithCookie(HttpCookie cookie)
        {
            Cookies.Add(cookie);

            return this;
        }


        public new ResourceResult<T> WithCache(Action<HttpContextBase,HttpCachePolicyBase> action)
        {
            _cacheActions.Add(action);

            return this;
        }


        public new ResourceResult<T> AsJson()
        {
            ContentType = "application/json";

            return this;
        }


		public new ResourceResult<T> AsXml()
        {
            ContentType = "text/xml";

            return this;
        }


		public new ResourceResult<T> AsText()
        {
            ContentType = "text/plain";

            return this;
        }

		public new ResourceResult<T> AsXhtml()
        {
            ContentType = "application/xhtml+xml";

            return this;
        }

		public new ResourceResult<T> AsHtml()
        {
            ContentType = "text/html";

            return this;
        }


		public new ResourceResult<T> As(string type)
        {
            ContentType = type;

            return this;
        }


		public new ResourceResult<T> AsFile(string type)
        {
            ContentType = type;

            return this;
        }


		public new ResourceResult<T> AsFile(string type, string defaultFilename)
        {
            ContentType = type;

            return WithHeader("Content-Disposition", "attachment; filename=" + defaultFilename);
        }

		public ResourceResult<T> ProjectedTo<TOtherType>(Func<T,TOtherType> projection)
		{
			var mapping = new Mapping<T, TOtherType>();
			mapping.AutoRelateEqualNames(true, true);

			projection = mapping.Map;

			return this;
		}


        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.StatusCode = StatusCode;
        
            AppendHeaders(context);

            AppendCookies(context);

            if (Resource == null) return;

            // Delegate to ActionResult, if one was given.
            var innerResult = Resource as ActionResult;

            if (innerResult != null)
            {
                innerResult.ExecuteResult(context);
                return;
            }

			if (IsRedirection()) return;


            var acceptTypes = ParseAcceptTypes(context.HttpContext.Request.AcceptTypes);

            var formatter = FindFormatter(context, acceptTypes);

            if (formatter == null)
            {
                if (Resource is string)
                {
                    context.HttpContext.Response.Output.Write(Resource);
                }
				if (Resource is Url)
					return;
				if (_contentTypeExplicitlySet)
				{
					throw new HttpException(500,
											string.Format(
												"Mime type explicitly set to '{0}' but unable to find a view that can format this type.",
												ContentType));
				}
				context.HttpContext.Response.StatusCode = 406; // not acceptable
				
            	return;
            }

            formatter.Output(context, projection(Resource), ContentType);

            ApplyCacheActions(context);

        }

		bool IsRedirection() { return StatusCode >= 300 && StatusCode < 400; }


		private IEnumerable<string> ParseAcceptTypes(IEnumerable<string> types)
        {
            // TODO process "q" and "level" options and sort accordingly by stealing code from openrasta

            if (types == null) return Enumerable.Empty<string>();

            return from type in types
                   let pos = type.IndexOf(';')
                   let length = pos >= 0 ? pos : type.Length
                   select type.Substring(0, length);
        }


        private void AppendCookies(ControllerContext context)
        {
            foreach (var cookie in Cookies)
            {
                context.HttpContext.Response.AppendCookie(cookie);
            }
        }


        private void AppendHeaders(ControllerContext context)
        {
            foreach (var header in _headers)
            {
                context.HttpContext.Response.AppendHeader(header.Key, header.Value.ToString());
            }
        }


        private void ApplyCacheActions(ControllerContext context)
        {
            foreach (var action in _cacheActions)
            {
                action(context.HttpContext,context.HttpContext.Response.Cache);
            }
        }

        private Type ResourceType()
        {
            return Resource == null ? typeof (object) : Resource.GetType();
        }

        private IResourceFormatter FindFormatter(ControllerContext context, IEnumerable<string> acceptTypes)
        {
            if (!acceptTypes.Any())
                return ResourceFormatters.FormattersFor(ResourceType()).FirstOrDefault();

            if (ContentType != null) // Controller action forced the content type.
            {
                EnsureContentTypeIsMimeType();
                return ResourceFormatters.FormattersFor(ResourceType()).FirstOrDefault(f => f.CanFormat(context, Resource, ContentType));
            }

            var formatters = (from acceptType in acceptTypes
                              from formatter in ResourceFormatters.FormattersFor(ResourceType())
                              where formatter.CanFormat(context, Resource, acceptType)
                              select formatter).ToArray();

            return formatters.FirstOrDefault();
        }

        private void EnsureContentTypeIsMimeType()
        {
            if (ContentType.Contains('/')) return;
            var mimeType = MimeTypes.GetMimeTypeForExtension(ContentType);
            if (!string.IsNullOrEmpty(mimeType))
                ContentType = mimeType;
        }
    }
}