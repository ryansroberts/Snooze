using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
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
		/// <summary>
		/// All IRouteRegistration instances and all delegates
		/// </summary>
		/// <param name="assembly"></param>
		/// <returns></returns>
        public IEnumerable<IRouteRegistration> Scan(Assembly assembly)
		{
            return Enumerable.Concat(
                    assembly.GetLoadableTypes().SelectMany(t => Enumerable.Concat(new[] { t }, t.GetLoadableNestedTypes()))
                        .Where(IsConstructableRouteRegistration)
                        .Select(t => (IRouteRegistration)Activator.CreateInstance(t)),
                    assembly.GetLoadableTypes().Where(t => typeof(Handler).IsAssignableFrom(t))
                        .SelectMany(t => t.GetFields(BindingFlags.NonPublic | BindingFlags.Static)
                            .Where(IsRegister)
                            .Select(f => new DelegatedRouteRegistration((Handler.Register)f.GetValue(null)))
                        ));
	    }


        static bool IsConstructableRouteRegistration(Type t)
        {
            return typeof (IRouteRegistration).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface;
        }

        static bool IsRegister(FieldInfo f)
        {
            try
            {
                return typeof(Handler.Register).IsAssignableFrom(f.FieldType);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public static class AssemblyExtensions
    {
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

        public static IEnumerable<Type> GetLoadableNestedTypes(this Type type)
        {
            try
            {
                return type.GetNestedTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }
    }
}