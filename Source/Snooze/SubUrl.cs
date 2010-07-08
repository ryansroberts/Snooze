using System;
using System.Web.Mvc;
using Snooze.Routing;
using System.Web.Routing;

namespace Snooze
{
    /// <summary>
    /// Base class for sub-URL strongly-typed parameters.
    /// </summary>
    /// <typeparam name="TParentUrl">Type of parent URL.</typeparam>
    [SubUrl]
    public abstract class SubUrl<TParentUrl> : Url
        where TParentUrl : Url
    {
        public TParentUrl Parent { get; set; }

        protected internal override void FillRouteValueDictionary(RouteValueDictionary values)
        {
            EnsureParentNotNull();
            Parent.FillRouteValueDictionary(values);
            base.FillRouteValueDictionary(values);
        }

        void EnsureParentNotNull()
        {
            if (Parent == null) throw new InvalidOperationException("Parent Url is null.");
        }
    }
}
