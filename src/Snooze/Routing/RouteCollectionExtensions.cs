#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Routing;

#endregion

namespace Snooze.Routing
{
    public static class RouteCollectionExtensions
    {
        static HashSet<Type> added = new HashSet<Type>();

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

                DoIfRouteIsNotRegistered < TUrl>(() =>
                {
                    routes.Add(routeName,  (RouteBase) Activator.CreateInstance(routeType, routeExpression,parentRoute));
                    ModelBinders.Binders.Add(typeof(TUrl), new SubUrlModelBinder());
                });

                // ModelBinders are not inherited, so we need to explicitly add the SubUrlModelBinder here.
               
            }
            else
            {
                 DoIfRouteIsNotRegistered<TUrl>(()=>routes.Add(routeName, new ResourceRoute<TUrl>(routeExpression)));
            }
        }

        static void DoIfRouteIsNotRegistered<TUrl>(Action action)
        {
            if (!added.Contains(typeof(TUrl)))
                action();
            added.Add(typeof(TUrl));
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

        public static void FromAssemblyWithType<TRef>(this RouteCollection routes)
        {
            var disco = new RoutingRegistrationDiscovery();

            foreach (var routeRegistration in disco.Scan(typeof(TRef).Assembly))
            {
                routeRegistration.Register(routes);
            }
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