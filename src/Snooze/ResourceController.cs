#region

using System;
using System.Web.Mvc;
using System.Web.Routing;

#endregion

namespace Snooze
{
	public class ResourceController : Controller
    {
		

        public ResourceController()
        {
            ActionInvoker = new ResourceActionInvoker();
        }


		protected LeftMappingConfigurator<T> Map<T>(T item)
		{
    		return new LeftMappingConfigurator<T>(item);
		}

        public virtual ResourceResult<T> OK<T>(T resource)
        {
            return new ResourceResult<T>(200, resource);
        }


		public virtual ResourceResult<T> Created<T>(Url url, T informationResource)
        {
            return new ResourceResult<T>(201, informationResource).WithHeader("Location", url.ToString());
        }


        public virtual ResourceResult<object> NoContent()
        {
            return new ResourceResult<object>(204, null);
        }


        public virtual ResourceResult<object> ResetContent()
        {
            return new ResourceResult<object>(205, null);
        }

        public virtual ResourceResult<object> MovedPermenently(Url url)
        {
            return new ResourceResult<object>(301, null).WithHeader("Location", url.ToString());
        }

        public virtual ResourceResult<object> MovedPermenently(string url)
        {
            return new ResourceResult<object>(301, null).WithHeader("Location", url);
        }

        public virtual ResourceResult<object> MovedPermanently(Url url)
        {
            return new ResourceResult<object>(301, null).WithHeader("Location", url.ToString());
        }


        public virtual ResourceResult<object> MovedPermanently(string url)
        {
            return new ResourceResult<object>(301, null).WithHeader("Location", url);
        }

        public virtual ResourceResult<object> Found(Url url)
        {
            return new ResourceResult<object>(302, null).WithHeader("Location", url.ToString());
        }


        public virtual ResourceResult<object> Found(string url)
        {
            return new ResourceResult<object>(302, null).WithHeader("Location", url);
        }

        public virtual ResourceResult<object> SeeOther(Url url)
        {
            return new ResourceResult<object>(303, null).WithHeader("Location", url.ToString());
        }


        public virtual ResourceResult<object> SeeOther(string url)
        {
            return new ResourceResult<object>(303, null).WithHeader("Location", url);
        }

        public virtual ResourceResult<object> NotModified()
        {
            return new ResourceResult<object>(304, null);
        }


        public virtual ActionResult Redirect(Url url)
        {
            return Redirect(url.ToString());
        }

        protected override RedirectResult Redirect(string url)
        {
            throw new InvalidOperationException("302 Redirects should not be used unless you REALLY mean to return a 'FOUND' response, if so use the Found method instead.");
        }


        public virtual ActionResult TemporaryRedirect(Url url)
        {
            return new ResourceResult<string>(307, url.ToString()).WithHeader("Location", url.ToString());
        }


        public virtual ActionResult TemporaryRedirect(string url)
        {
            return new ResourceResult<string>(307, url).WithHeader("Location", url);
        }

        public virtual ResourceResult<object> BadRequest()
        {
            return new ResourceResult<object>(400, null);
        }

        public virtual ResourceResult<object> BadRequest(object errorResource)
        {
			return new ResourceResult<object>(400, errorResource);
        }

		public virtual ResourceResult<object> Forbidden()
        {
			return new ResourceResult<object>(403, null);
        }

		public virtual ResourceResult<object> Forbidden(object errorResource)
        {
			return new ResourceResult<object>(403, errorResource);
        }

		public virtual ResourceResult<object> NotFound()
        {
			return new ResourceResult<object>(404, null);
        }


		public virtual ResourceResult<object> NotFound(object errorResource)
        {
			return new ResourceResult<object>(404, errorResource);
        }


        protected override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var r = filterContext.Result as ResourceResult;

            if (r != null)
            {
                filterContext.Controller.ViewData.Model = r.Resource;
            }


            base.OnResultExecuting(filterContext);
        }
    }


	public abstract class Handler : ResourceController
	{
		public delegate void Register(RouteCollection collection);

	}
}