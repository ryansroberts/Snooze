using System;
using Castle.Windsor;

namespace Snooze.AutoMock.Castle
{
	public interface IAutoMockHelper {
		/// <summary>
		/// Called when the parent helper needs to register a mock
		/// </summary>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		void RegisterInstance(Type type, object instance);

		/// <summary>
		/// Called when the parent helper needs to register a mock
		/// </summary>
		/// <param name="type"></param>
		/// <param name="key"></param>
		/// <param name="instance"></param>
		void RegisterInstance(Type type, string key, object instance);

		/// <summary>
		/// Register a single instance for all of these services.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="instance"></param>
		void RegisterInstance(Type[] services, object instance);

		/// <summary>
		/// This is required to be set before 
		/// </summary>
		IWindsorContainer Container { get; set; }
	}
}
