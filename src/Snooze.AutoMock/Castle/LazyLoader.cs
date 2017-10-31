using System;
using Castle.MicroKernel.Resolvers;
using Castle.MicroKernel.Registration;
using Snooze.AutoMock.Castle.MoqContrib.AutoMock;

namespace Snooze.AutoMock.Castle
{
	class LazyLoader : ILazyComponentLoader
	{
		private ILatentMocker _helper;

		public LazyLoader(ILatentMocker helper)
		{
			_helper = helper;
		}

		#region ILazyComponentLoader Members

		public IRegistration Load(string key, Type service, System.Collections.IDictionary arguments)
		{
			// if we've gotten this far, the container definitely doesn't have the component
			if (service.IsInterface || service.IsAbstract)
				return Component.For(service).Instance(_helper.CreateUnregisteredMock(service).Object);
			else
			{
				// maybe we should get this guy to return info for registrations??
				_helper.MockMyDependencies(service);
				return null;
			}
		}

		#endregion
	}
}
