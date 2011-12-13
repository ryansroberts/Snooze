using System;
using Castle.Windsor;
using Moq;
using Moq.Language.Flow;
using System.Linq.Expressions;

namespace Snooze.AutoMock.Castle.MoqContrib.AutoMock
{
    /// <summary>
    /// Provides access to easily work with mocks. 
    /// </summary>
    /// <remarks>
    /// All members of this interface do things with mocks and the container. This interface
    /// is exposed on all containers as a property called `Mock`.
    /// </remarks>
    /// <example>
    /// IWindsorContainer container = new AutoMockContainer();
    /// // registrations
    /// var sut = container.Resolve&lt;TheThingWereTesting&gt;(); // uses an IService
    /// sut.DoSomething();
    /// var serviceMock = container.Mock.Get&lt;IService&gt;();
    /// serviceMock.Verify(x => x.ProvideServices());
    /// </example>
    public interface IAutoMocker
    {
		/// <summary>
		/// Create an instance of T with all it's dependancies mocked.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		T CreateTestSubject<T>() where T : class;

        /// <summary>
        /// Gets the mock for <c>T</c>. The mock will be created if it necessary
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>A mock for the given type</returns>
        Mock<T> Get<T>() where T : class;

        /// <summary>
        /// Gets the mock for <c>type</c>. The mock will be created if it necessary
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        Mock Get(Type type);

        /// <summary>
        /// Gets the mock for <c>T</c>. The mock will be created if it necessary
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns>A mock for the given type</returns>
        Mock<T> Get<T>(string key) where T : class;

        /// <summary>
        /// Gets the mock for <c>type</c>. The mock will be created if it necessary
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <returns></returns>
		Mock Get(Type type, string key);

		/// <summary>
		/// Gets the mock for <c>T</c>. The mock will be created if it necessary
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns>A mock for the given type</returns>
		T Of<T>() where T : class;

		/// <summary>
		/// Gets the mock for <c>type</c>. The mock will be created if it necessary
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		object Of(Type type);

		/// <summary>
		/// Gets the mock for <c>T</c>. The mock will be created if it necessary
		/// </summary>
		/// <param name="key"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns>A mock for the given type</returns>
		T Of<T>(string key) where T : class;

		/// <summary>
		/// Gets the mock for <c>type</c>. The mock will be created if it necessary
		/// </summary>
		/// <param name="key"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		object Of(Type type, string key);

		

		/// <summary>
		/// Specifies a setup on the mocked type for a call to a value returning method
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="setupExpression"></param>
		/// <returns></returns>
		ISetup<T, object> Setup<T>(Expression<Func<T, object>> setupExpression)
			where T : class;

		/// <summary>
		/// Specifies a setup on the mocked type for a call to a value returning method
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="setupExpression"></param>
		/// <returns></returns>
		ISetup<T> Setup<T>(Expression<Action<T>> setupExpression)
			where T : class;

		/// <summary>
		/// Verifies that a specific invocation matching the given expression was performed
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="verifyExpression"></param>
		/// <returns></returns>
		void Verify<T>(Expression<Func<T, object>> verifyExpression)
			where T : class;

		/// <summary>
		/// Verifies that a specific invocation matching the given expression was performed
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="verifyExpression"></param>
		/// <param name="times"></param>
		/// <returns></returns>
		void Verify<T>(Expression<Func<T, object>> verifyExpression, Times times)
			where T : class;

		/// <summary>
		/// Verifies that a specific invocation matching the given expression was performed
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="verifyExpression"></param>
		/// <param name="failMessage"></param>
		/// <returns></returns>
		void Verify<T>(Expression<Func<T, object>> verifyExpression, string failMessage)
			where T : class;

		/// <summary>
		/// Verifies that a specific invocation matching the given expression was performed
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="verifyExpression"></param>
		/// <param name="times"></param>
		/// <param name="failMessage"></param>
		/// <returns></returns>
		void Verify<T>(Expression<Func<T, object>> verifyExpression, Times times, string failMessage)
			where T : class;

		/// <summary>
		/// Verifies that a specific invocation matching the given expression was performed
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="verifyExpression"></param>
		/// <returns></returns>
		void Verify<T>(Expression<Action<T>> verifyExpression)
			where T : class;

		/// <summary>
		/// Verifies that a specific invocation matching the given expression was performed
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="verifyExpression"></param>
		/// <param name="times"></param>
		/// <returns></returns>
		void Verify<T>(Expression<Action<T>> verifyExpression, Times times)
			where T : class;

		/// <summary>
		/// Verifies that a specific invocation matching the given expression was performed
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="verifyExpression"></param>
		/// <param name="failMessage"></param>
		/// <returns></returns>
		void Verify<T>(Expression<Action<T>> verifyExpression, string failMessage)
			where T : class;

		/// <summary>
		/// Verifies that a specific invocation matching the given expression was performed
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="verifyExpression"></param>
		/// <param name="times"></param>
		/// <param name="failMessage"></param>
		/// <returns></returns>
		void Verify<T>(Expression<Action<T>> verifyExpression, Times times, string failMessage)
			where T : class;

        /// <summary>
        /// Registers a single mock object that will be used for all the given services.
        /// </summary>
        /// <param name="services">A list of interfaces to implement</param>
        /// <returns>a mock implementing all given interfaces</returns>
        Mock Union(params Type[] services);

        #region Union<T,..,n>() overloads

        /// <summary>
        /// Registers a single mock object that will be used for all the given services.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <returns>a mock implementing all given interfaces</returns>
        Mock<T1> Union<T1, T2>() where T1:class;

        /// <summary>
        /// Registers a single mock object that will be used for all the given services.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <returns>a mock implementing all given interfaces</returns>
        Mock<T1> Union<T1, T2, T3>() where T1 : class;

        /// <summary>
        /// Registers a single mock object that will be used for all the given services.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <returns>a mock implementing all given interfaces</returns>
        Mock<T1> Union<T1, T2, T3, T4>() where T1 : class;

        /// <summary>
        /// Registers a single mock object that will be used for all the given services.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <returns>a mock implementing all given interfaces</returns>
        Mock<T1> Union<T1, T2, T3, T4, T5>() where T1 : class;

        /// <summary>
        /// Registers a single mock object that will be used for all the given services.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <returns>a mock implementing all given interfaces</returns>
        Mock<T1> Union<T1, T2, T3, T4, T5, T6>() where T1 : class;

        /// <summary>
        /// Registers a single mock object that will be used for all the given services.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <returns>a mock implementing all given interfaces</returns>
        Mock<T1> Union<T1, T2, T3, T4, T5, T6, T7>() where T1 : class;

        /// <summary>
        /// Registers a single mock object that will be used for all the given services.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <returns>a mock implementing all given interfaces</returns>
        Mock<T1> Union<T1, T2, T3, T4, T5, T6, T7, T8>() where T1 : class;

        /// <summary>
        /// Registers a single mock object that will be used for all the given services.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <returns>a mock implementing all given interfaces</returns>
        Mock<T1> Union<T1, T2, T3, T4, T5, T6, T7, T8, T9>() where T1 : class;

        /// <summary>
        /// Registers a single mock object that will be used for all the given services.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <returns>a mock implementing all given interfaces</returns>
        Mock<T1> Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>() where T1 : class;

        #endregion

    	/// <summary>
    	/// Register an instance to be bound to a particular type
    	/// </summary>
    	/// <param name="type"></param>
    	/// <param name="instance"></param>
    	void RegisterInstance(Type type, object instance);

    	/// <summary>
    	/// Register an instance to be bound to a particular type
    	/// </summary>
    	/// <param name="type"></param>
    	/// <param name="key"></param>
    	/// <param name="instance"></param>
    	void RegisterInstance(Type type, string key, object instance);

    	/// <summary>
    	/// Register a single instance for all of these services. It is safe to assume that 
    	/// the instance definitely implements or inherits all types listed.
    	/// </summary>
    	/// <param name="services">all the interfaces that the instance should be returned 
    	/// for</param>
    	/// <param name="instance"></param>
    	void RegisterInstance(Type[] services, object instance);

    	/// <summary>
    	/// This is required to be set before 
    	/// </summary>
    	IWindsorContainer Container { get; set; }

    	void UseContainer(IWindsorContainer container);
    }
}
