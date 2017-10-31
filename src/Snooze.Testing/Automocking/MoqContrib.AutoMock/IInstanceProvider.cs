using System;

namespace Snooze.AutoMock.Castle.MoqContrib.AutoMock
{
    /// <summary>
    /// Internal interface that simply gets and sets simple instances
    /// </summary>
    internal interface IInstanceProvider
    {
        /// <summary>
        /// Gets an instance from the client container.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>null if the instance wasn't found, otherwise the instance</returns>
        object GetInstance(Type type);

        /// <summary>
        /// Gets an instance from the client container.
        /// </summary>
        /// <typeparam name="T">The type of the instance being retrieved</typeparam>
        /// <returns>null if the instance wasn't found, otherwise the instance</returns>
        T GetInstance<T>();

        /// <summary>
        /// Sets an instance for the given type. This will only ever be a mock
        /// </summary>
        /// <param name="type"></param>
        /// <param name="instance"></param>
        void SetInstance(Type type, object instance);

        /// <summary>
        /// Sets an instance for the given type. This will only ever be a mock
        /// </summary>
        /// <typeparam name="T">The type of the instance being set</typeparam>
        /// <param name="instance">The instance being set</param>
        void SetInstance<T>(T instance);
    }
}
