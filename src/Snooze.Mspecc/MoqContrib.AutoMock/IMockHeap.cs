using System;
using Moq;

namespace Snooze.Mspecc.MoqContrib.AutoMock
{
    /// <summary>
    /// Keeps track of all mock instances that are generated
    /// </summary>
    public interface IMockHeap
    {
        /// <summary>
        /// Gets or sets a mock 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        Mock this[Type type] { get; set; }

        /// <summary>
        /// Gets or sets a mock for the given key
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        Mock this[Type type, string key] { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool ContainsType(Type type);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        bool ContainsType(Type type, string key);
    }
}
