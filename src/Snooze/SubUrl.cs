#region

using System;
using System.Web.Routing;

#endregion

namespace Snooze
{
    /// <summary>
    ///   Base class for sub-URL strongly-typed parameters.
    /// </summary>
    /// <typeparam name = "TParentUrl">Type of parent URL.</typeparam>
    [SubUrl]
    public abstract class SubUrl<TParentUrl> : Url, ISubUrl
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

        public Url GetParentUrl()
        {
            return Parent;
        }

        public Type GetParentUrlType()
        {
            return typeof (TParentUrl);
        }
    }

    public interface ISubUrl
    {
        Url GetParentUrl();
        Type GetParentUrlType();
    }
}