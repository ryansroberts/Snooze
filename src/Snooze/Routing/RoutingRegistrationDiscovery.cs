using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Web.Routing;
using Castle.DynamicProxy;

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
				    assembly.GetTypes().SelectMany(t => Enumerable.Concat(new[] {t}, t.GetNestedTypes()))
						.Where(IsConstructableRouteRegistration)
						.Select(t => (IRouteRegistration) Activator.CreateInstance(t)),
					assembly.GetTypes().Where(t => typeof (Handler).IsAssignableFrom(t))
						.Select(t => new DelegatedRouteRegistration(
							(Handler.Register) t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(f => typeof (Handler.Register).IsAssignableFrom(f.FieldType))
							.Select(f => f.GetValue(CreateDerivedClassWithParameterlessConstructor(t)))
							.FirstOrDefault())
						));
	    }

		private static readonly ProxyGenerator _generator = new ProxyGenerator();


		static object CreateDerivedClassWithParameterlessConstructor(Type t) { return _generator.CreateClassProxy(t); }

        static bool IsConstructableRouteRegistration(Type t)
        {
            return typeof (IRouteRegistration).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface;
        }
    }
}