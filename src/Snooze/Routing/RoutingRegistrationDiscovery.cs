using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Routing;

namespace Snooze.Routing
{

	public class DelegatedRouteRegistration : IRouteRegistration
	{
		Handler.Register registration;
		public DelegatedRouteRegistration(Handler.Register registration) { this.registration = registration; }
		public void Register(RouteCollection routes) { if(registration != null) registration(routes); }
	}

    public class RoutingRegistrationDiscovery
    {
        public IEnumerable<IRouteRegistration> Scan(Assembly assembly)
        {
        	return assembly.GetTypes().SelectMany(t => Enumerable.Concat(new []{t},t.GetNestedTypes()))
        		.Where(IsConstructableRouteRegistration)
        		.Select(t => (IRouteRegistration) Activator.CreateInstance(t));
        }

        static bool IsConstructableRouteRegistration(Type t)
        {
            return typeof (IRouteRegistration).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface;
        }
    }
}