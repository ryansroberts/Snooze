#region

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Glue;

#endregion

namespace Snooze
{
	public class LeftMappingConfigurator<TLeft>
	{
		TLeft item;

		public LeftMappingConfigurator(TLeft item) 
		{ 
			this.item = item; 
		}

		public RightConfigurator<TLeft, TRight> To<TRight>() where TRight : class {
			return new RightConfigurator<TLeft, TRight>(item);	
		}

		public RightConfigurator<TLeft, TRight> To<TRight>(Func<TLeft,TRight> projection) where TRight : class {
			return new RightConfigurator<TLeft, TRight>(item,projection(item));
		}
	}

	public class RightConfigurator<TLeft, TRight> where TRight : class
	{
		readonly TRight right;
		readonly TLeft left;
		Mapping<TLeft, TRight> mapping = new Mapping<TLeft, TRight>();
		readonly IList<Action<Mapping<TLeft, TRight>>>  configuration = new List<Action<Mapping<TLeft, TRight>>>();

		public RightConfigurator(TLeft left) 
		{ 
			this.left = left;
			configuration.Add(m => m.AutoRelateEqualNames(true,true));
		}

		public RightConfigurator(TLeft left,TRight right) : this(left) 
		{
			this.right = right;
		}

		public void Configure(Action<Mapping<TLeft, TRight>> dothis) { configuration.Add(dothis); }

		public TRight Item
		{
			get
			{
				foreach (var action in configuration)
					action(mapping);

				if (right == null)
					return mapping.Map(left);

				return mapping.Map(left, right);
			}
		}
	}

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
}