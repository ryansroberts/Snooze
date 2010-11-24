#region

using System;
using System.Web.Mvc;

#endregion

namespace Snooze
{
    public class ResourceController : Controller
    {
        public ResourceController()
        {
            ActionInvoker = new ResourceActionInvoker();
        }


        public virtual ResourceResult OK(object resource)
        {
            return new ResourceResult(200, resource);
        }


        public virtual ResourceResult Created(Url url, object informationResource)
        {
            return new ResourceResult(201, informationResource).WithHeader("Location", url.ToString());
        }


        public virtual ResourceResult NoContent()
        {
            return new ResourceResult(204, null);
        }


        public virtual ResourceResult ResetContent()
        {
            return new ResourceResult(205, null);
        }


        public virtual ResourceResult SeeOther(Url url)
        {
            return new ResourceResult(303, null).WithHeader("Location", url.ToString());
        }


        public virtual ResourceResult SeeOther(string url)
        {
            return new ResourceResult(303, null).WithHeader("Location", url);
        }


        public virtual ResourceResult Found(Url url)
        {
            return new ResourceResult(302, null).WithHeader("Location", url.ToString());
        }


        public virtual ResourceResult Found(string url)
        {
            return new ResourceResult(302, null).WithHeader("Location", url);
        }


        public virtual ResourceResult NotModified()
        {
            return new ResourceResult(304, null);
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
            return new ResourceResult(307, url.ToString()).WithHeader("Location", url.ToString());
        }


        public virtual ActionResult TemporaryRedirect(string url)
        {
            return new ResourceResult(307, url).WithHeader("Location", url);
        }


        public virtual ResourceResult BadRequest()
        {
            return new ResourceResult(400, null);
        }


        public virtual ResourceResult BadRequest(object errorResource)
        {
            return new ResourceResult(400, errorResource);
        }


        public virtual ResourceResult NotFound()
        {
            return new ResourceResult(404, null);
        }


        public virtual ResourceResult NotFound(object errorResource)
        {
            return new ResourceResult(404, errorResource);
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
}