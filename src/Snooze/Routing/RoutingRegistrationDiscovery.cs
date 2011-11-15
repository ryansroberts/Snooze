using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Snooze.Routing
{
    public class RoutingRegistrationDiscovery
    {
        public IEnumerable<IRouteRegistration> Scan(Assembly assembly)
        {
            return assembly.GetTypes()
				.SelectMany(outer => Enumerable.Concat(new[]{outer}, outer.GetNestedTypes()))
                .Where(IsConstructableRouteRegistration)
                .Select(t => (IRouteRegistration)Activator.CreateInstance(t));
        }

        static bool IsConstructableRouteRegistration(Type t)
        {
            return typeof (IRouteRegistration).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface;
        }
    }
}