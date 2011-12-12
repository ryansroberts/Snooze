using System;

namespace Snooze.AutoMock.Castle.MoqContrib.AutoMock
{
    /// <summary>
    /// Creates generic mocks without using compile-time generic parameters
    /// </summary>
    public interface IMockGenerator
    {
        /// <summary>
        /// Generate a new mock with the given generic parameter
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        Moq.Mock Generate(Type type);

	}
}
