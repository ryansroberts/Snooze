using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Routing;

namespace Snooze.Routing
{

	public delegate void RegisterRoute(RouteCollection route);


	public class DelegatedRouteRegistration : IRouteRegistration
	{
		RegisterRoute registration;
		public DelegatedRouteRegistration(RegisterRoute registration) { this.registration = registration; }
		public void Register(RouteCollection routes) { registration(routes); }
	}

    public class RoutingRegistrationDiscovery
    {
        public IEnumerable<IRouteRegistration> Scan(Assembly assembly)
        {
        	return Enumerable.Concat(assembly.GetTypes()
        		.Where(IsConstructableRouteRegistration)
        		.Select(t => (IRouteRegistration) Activator.CreateInstance(t)),
        		assembly.GetTypes().SelectMany(t => t.GetNestedTypes())
        		.Where(t => typeof(RegisterRoute).IsAssignableFrom(t))
        		.Select(t => new DelegatedRouteRegistration((RegisterRoute) Activator.CreateInstance(t))));
        }

        static bool IsConstructableRouteRegistration(Type t)
        {
            return typeof (IRouteRegistration).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface;
        }
    }
}