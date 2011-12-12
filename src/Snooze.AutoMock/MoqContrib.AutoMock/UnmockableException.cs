using System;

namespace Snooze.Mspecc.MoqContrib.AutoMock
{
	/// <summary>
	/// Indicates that we couldn't generate a mock for the given service. This is
	/// usually because the service was a primitive like a string or number.
	/// </summary>
	public class UnmockableException : ArgumentException
	{
		/// <summary>
		/// The service that couldn't be moced
		/// </summary>
		public Type Service { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="service"></param>
		public UnmockableException(Type service)
			:base("Cannot create a mock of the given type", service.Name)
		{
			Service = service;
		}
	}
}
