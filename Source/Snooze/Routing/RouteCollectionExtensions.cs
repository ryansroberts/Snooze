#region

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Routing;

#endregion

namespace Snooze.Routing
{
    public static class RouteCollectionExtensions
    {
        /// <summary>
        ///   Adds a route for the given Url type.
        /// </summary>
        public static void Map<TUrl>(this RouteCollection routes, Expression<Func<TUrl, string>> routeExpression)
            where TUrl : Url
        {
            var routeName = ResourceRoute.GetRouteNameFromUrlType(typeof (TUrl));

            if (typeof (TUrl).IsDefined(typeof (SubUrlAttribute), true))
            {
                var routeType = GetSubResourceType<TUrl>();
                var parentRoute = GetParentRoute<TUrl>(RouteTable.Routes);

                routes.Add(routeName, (RouteBase) Activator.CreateInstance(routeType, routeExpression, parentRoute));

                // ModelBinders are not inherited, so we need to explicitly add the SubUrlModelBinder here.
                ModelBinders.Binders.Add(typeof (TUrl), new SubUrlModelBinder());
            }
            else
            {
                routes.Add(routeName, new ResourceRoute<TUrl>(routeExpression));
            }
        }

        /// <summary>
        ///   Adds a route that will handle static versioned file requests.
        ///   Route URL is "static/{Version}/{*Path}".
        /// </summary>
        public static void AddVersionedStaticFilesSupport(this RouteCollection routes)
        {
            routes.AddVersionedStaticFilesSupport("static/");
        }

        /// <summary>
        ///   Adds a route that will handle static versioned file requests.
        ///   Route URL is "static/{Version}/{*Path}".
        /// </summary>
        /// <param name = "routePrefix">The route url prefix e.g.
        ///   <example>
        ///     "static/"
        ///   </example>
        ///   .</param>
        public static void AddVersionedStaticFilesSupport(this RouteCollection routes, string routePrefix)
        {
            routes.Map<StaticFileUrl>(s => routePrefix + s.Version + "/" + s.Path.CatchAll());
        }

        /// <summary>
        ///   Adds routes that provide IE6 support for PNG images.
        ///   Route URLs are "iesupport/{SnoozeVersion}/pngbehavior.htc" and "iesupport/{SnoozeVersion}/blank.gif".
        /// </summary>
        public static void AddIE6Support(this RouteCollection routes)
        {
            routes.Map<IEPngFixUrl>(u => "iesupport/" + u.SnoozeVersion + "/pngbehavior.htc");
            routes.Map<BlankGifUrl>(u => "iesupport/" + u.SnoozeVersion + "/blank.gif");
        }

        static Type GetSubResourceType<TUrl>() where TUrl : Url
        {
            var parentUrlType = typeof (TUrl).BaseType.GetGenericArguments()[0];
            return typeof (SubResourceRoute<,>).MakeGenericType(typeof (TUrl), parentUrlType);
        }

        static RouteBase GetParentRoute<TUrl>(RouteCollection routes) where TUrl : Url
        {
            var parentType = typeof (ResourceRoute<>).MakeGenericType(typeof (TUrl).BaseType.GetGenericArguments()[0]);
            var parent = routes.First(r => parentType.IsAssignableFrom(r.GetType()));
            return parent;
        }
    }
}