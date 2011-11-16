using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Moq;
using System.Linq.Expressions;
using Moq.Language.Flow;
using Snooze.Mspecc.AutoMock.Castle;

namespace Snooze.Mspecc.MoqContrib.AutoMock
{
    /// <summary>
    /// The central access point for functionality common accross all containers. This class
    /// is responsible for coordinating the generation, access and registration of mocks with
    /// the end containers.
    /// </summary>
    /// <remarks>
    /// As the only public class in this assembly, this acts as the mediator for all other classes
    /// in this assembly as well as the whole umbrella of projects under this.
    /// </remarks>
    public class AutoMockHelper : IAutoMocker, ILatentMocker, IAutoMockHelper
    {
		/// <summary></summary>
        protected IMockHeap _heap;
		/// <summary></summary>
		protected IMockGenerator _generator;

        /// <summary>
        /// Default constructor
        /// </summary>
        public AutoMockHelper()
            :this(new MockHeap(), new MockGenerator())
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="heap"></param>
        /// <param name="generator"></param>
        public AutoMockHelper(IMockHeap heap, IMockGenerator generator)
        {
            _heap = heap;
            _generator = generator;
        }

    	/// <summary>
    	/// This is required to be set before 
    	/// </summary>
    	public IWindsorContainer Container { get; set; }

    	#region IAutoMockContainer Members

    	/// <summary>
        /// Gets a mock for the given service. If there isn't already a mock registered
        /// it will create the mock and register it.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public virtual Mock<TService> Get<TService>() where TService : class
        {
            var type = typeof(TService);
            return (Mock<TService>)Get(type);
        }

        /// <summary>
        /// Gets a mock for the given service. If there isn't already a mock registered
        /// it will create the mock and register it.
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public virtual Mock<TService> Get<TService>(string key) where TService : class
        {
            var type = typeof(TService);
            return (Mock<TService>)Get(type, key);
        }

        /// <summary>
        /// Gets a mock of the given type
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public virtual Mock Get(Type service)
        {
			if (_heap.ContainsType(service))
				return _heap[service];
			else
			{
				var mock = CreateUnregisteredMock(service);
				RegisterInstance(service, mock.Object);
				return mock;
			}
        }

		#region ILatentMocker Members

		/// <summary>
		/// Creates a mock but doesn't register it with the IoC container
		/// </summary>
		/// <param name="service"></param>
		/// <returns></returns>
		public virtual Mock CreateUnregisteredMock(Type service)
		{
			// TODO: Tests were written for the generic version of this, but still no
			// coverage on the non-generic version
			if (_heap.ContainsType(service))
			{
				return _heap[service];
			}
			else
			{
				var mock = _generator.Generate(service);
				if (mock == null)
					throw new UnmockableException(service);
				_heap[service] = mock;
				return mock;
			}
		}

		public void MockMyDependencies(Type service)
		{
			var ctor = service.GetConstructors().OrderByDescending(x => x.GetParameters().Length).First();
			if (!(service.IsAbstract && service.IsInterface) && !Container.Kernel.HasComponent(service))
				Container.Register(Component.For(service));
			
			foreach (var param in ctor.GetParameters())
			{
				//Register concrete  type if..concrete
				if (!(param.ParameterType.IsAbstract && param.ParameterType.IsInterface))
				{
					if (!Container.Kernel.HasComponent(param.ParameterType))
					{
						Container.Register(Component.For(param.ParameterType));
					}
					
				}
				else Get(param.ParameterType);
			}
		}

		#endregion

        /// <summary>
        /// Gets a mock of the given type and named key
        /// </summary>
        /// <param name="service"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual Mock Get(Type service, string key)
        {
            // TODO: Tests were written for the generic version of this, but still no
            // coverage on the non-generic version
            if (_heap.ContainsType(service, key))
            {
                return _heap[service, key];
            }
            else
            {
                var mock = _generator.Generate(service);
                _heap[service, key] = mock;
                RegisterInstance(service, key, mock.Object);
                return mock;
            }
        }

        #endregion

    	#region IAutoMocker Members

        /// <summary>
        /// Registers a single mock object that will be used for all the given services.
        /// </summary>
        /// <param name="services">A list of interfaces to implement</param>
        /// <returns>a mock implementing all given interfaces</returns>
        public Mock Union(params Type[] services)
        {
            List<Type> typesNeedingMocks = new List<Type>();
            int classCount = 0;
            Mock ret = null;
            foreach (var service in services)
            {
				//var contained = RemoveComponents(service, ret != null);
				//if (contained != null && ret == null)
				//    ret = contained;
                if (!service.IsInterface)
                    classCount++;
                if (!(ret != null && service.IsInstanceOfType(ret.Object)))
                    typesNeedingMocks.Add(service);
            }
            if (classCount > 1)
                throw new ArgumentException("There can't be more than one class in a union");
            if (ret == null)
            {
                ret = _generator.Generate(services[0]);
            }
            foreach (var service in services.Skip(1))
            {
                var meth = typeof(Mock).GetMethod("As").MakeGenericMethod(service);
                meth.Invoke(ret, new object[0]);
                //RemoveComponents(service, true);
            }
            RegisterInstance(services, ret.Object);
            return ret;
        }

		/// <summary>
		/// Gets the mock for <c>T</c>. The mock will be created if it necessary
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns>A mock for the given type</returns>
		public T Of<T>() where T : class
		{
			return Get<T>().Object;
		}

		/// <summary>
		/// Gets the mock for <c>type</c>. The mock will be created if it necessary
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public object Of(Type type)
		{
			return Get(type).Object;
		}

		/// <summary>
		/// Gets the mock for <c>T</c>. The mock will be created if it necessary
		/// </summary>
		/// <param name="key"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns>A mock for the given type</returns>
		public T Of<T>(string key) where T : class
		{
			return Get<T>(key).Object;
		}

		/// <summary>
		/// Gets the mock for <c>type</c>. The mock will be created if it necessary
		/// </summary>
		/// <param name="key"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public object Of(Type type, string key)
		{
			return Get(type, key);
		}

		#region Setup & Verify

		/// <summary>
		/// Specifies a setup on the mocked type for a call to a value returning method
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="setupExpression"></param>
		/// <returns></returns>
		public ISetup<T, object> Setup<T>(Expression<Func<T, object>> setupExpression)
			where T : class
		{
			return Get<T>().Setup(setupExpression);
		}

		/// <summary>
		/// Specifies a setup on the mocked type for a call to a value returning method
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="setupExpression"></param>
		/// <returns></returns>
		public ISetup<T> Setup<T>(Expression<Action<T>> setupExpression)
			where T : class
		{
			return Get<T>().Setup(setupExpression);
		}

		/// <summary>
		/// Verifies that a specific invocation matching the given expression was performed
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="verifyExpression"></param>
		/// <returns></returns>
		public void Verify<T>(Expression<Func<T, object>> verifyExpression)
			where T : class
		{
			Get<T>().Verify(verifyExpression);
		}

		/// <summary>
		/// Verifies that a specific invocation matching the given expression was performed
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="verifyExpression"></param>
		/// <param name="times"></param>
		/// <returns></returns>
		public void Verify<T>(Expression<Func<T, object>> verifyExpression, Times times)
			where T : class
		{
			Get<T>().Verify(verifyExpression, times);
		}

		/// <summary>
		/// Verifies that a specific invocation matching the given expression was performed
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="verifyExpression"></param>
		/// <param name="failMessage"></param>
		/// <returns></returns>
		public void Verify<T>(Expression<Func<T, object>> verifyExpression, string failMessage)
			where T : class
		{
			Get<T>().Verify(verifyExpression, failMessage);
		}

		/// <summary>
		/// Verifies that a specific invocation matching the given expression was performed
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="verifyExpression"></param>
		/// <param name="times"></param>
		/// <param name="failMessage"></param>
		/// <returns></returns>
		public void Verify<T>(Expression<Func<T, object>> verifyExpression, Times times, string failMessage)
			where T : class
		{
			Get<T>().Verify(verifyExpression, times, failMessage);
		}

		/// <summary>
		/// Verifies that a specific invocation matching the given expression was performed
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="verifyExpression"></param>
		/// <returns></returns>
		public void Verify<T>(Expression<Action<T>> verifyExpression)
			where T : class
		{
			Get<T>().Verify(verifyExpression);
		}

		/// <summary>
		/// Verifies that a specific invocation matching the given expression was performed
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="verifyExpression"></param>
		/// <param name="times"></param>
		/// <returns></returns>
		public void Verify<T>(Expression<Action<T>> verifyExpression, Times times)
			where T : class
		{
			Get<T>().Verify(verifyExpression, times);
		}

		/// <summary>
		/// Verifies that a specific invocation matching the given expression was performed
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="verifyExpression"></param>
		/// <param name="failMessage"></param>
		/// <returns></returns>
		public void Verify<T>(Expression<Action<T>> verifyExpression, string failMessage)
			where T : class
		{
			Get<T>().Verify(verifyExpression, failMessage);
		}

		/// <summary>
		/// Verifies that a specific invocation matching the given expression was performed
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="verifyExpression"></param>
		/// <param name="times"></param>
		/// <param name="failMessage"></param>
		/// <returns></returns>
		public void Verify<T>(Expression<Action<T>> verifyExpression, Times times, string failMessage)
			where T : class
		{
			Get<T>().Verify(verifyExpression, times, failMessage);
		}

		#endregion

		#region Union<T1,..,n>() overloads
		/// <summary>
        /// Registers a single mock object that will be used for all the given services.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <returns>a mock implementing all given interfaces</returns>
        public Mock<T1> Union<T1, T2>() where T1 : class
        {
            return (Mock<T1>)Union(typeof(T1), typeof(T2));
        }

        /// <summary>
        /// Registers a single mock object that will be used for all the given services.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <returns>a mock implementing all given interfaces</returns>
        public Mock<T1> Union<T1, T2, T3>() where T1 : class
        {
            return (Mock<T1>)Union(typeof(T1), typeof(T2), typeof(T3));
        }

        /// <summary>
        /// Registers a single mock object that will be used for all the given services.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <returns>a mock implementing all given interfaces</returns>
        public Mock<T1> Union<T1, T2, T3, T4>() where T1 : class
        {
            return (Mock<T1>)Union(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        }

        /// <summary>
        /// Registers a single mock object that will be used for all the given services.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <returns>a mock implementing all given interfaces</returns>
        public Mock<T1> Union<T1, T2, T3, T4, T5>() where T1 : class
        {
            return (Mock<T1>)Union(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        }

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
        public Mock<T1> Union<T1, T2, T3, T4, T5, T6>() where T1 : class
        {
            return (Mock<T1>)Union(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        }

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
        public Mock<T1> Union<T1, T2, T3, T4, T5, T6, T7>() where T1 : class
        {
            return (Mock<T1>)Union(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        }

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
        public Mock<T1> Union<T1, T2, T3, T4, T5, T6, T7, T8>() where T1 : class
        {
            return (Mock<T1>)Union(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        }


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
        public Mock<T1> Union<T1, T2, T3, T4, T5, T6, T7, T8, T9>() where T1 : class
        {
            return (Mock<T1>)Union(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        }


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
        public Mock<T1> Union<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>() where T1 : class
        {
            return (Mock<T1>)Union(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
        }

        #endregion

        #endregion

    	/// <summary>
    	/// Called when the parent helper needs to register a mock
    	/// </summary>
    	/// <param name="type"></param>
    	/// <param name="instance"></param>
    	public virtual void RegisterInstance(Type type, object instance)
    	{
    		Container.Register(Component.For(type).Instance(instance));
    	}

    	/// <summary>
    	/// Create an instance of T with all it's dependancies mocked.
    	/// </summary>
    	/// <typeparam name="T"></typeparam>
    	/// <returns></returns>
    	public virtual T CreateTestSubject<T>() where T : class
    	{
    		if (!HasComponent<T>(Container))
    			Container.Register(Component.For<T>().LifeStyle.Is(LifestyleType.Transient));
			MockMyDependencies(typeof(T));
    		return Container.Resolve<T>();
    	}

    	static bool HasComponent<T>(IWindsorContainer container)
    		where T : class
    	{
    		bool isClass = typeof(T).IsClass;
    		bool hasComponent = container.Kernel.HasComponent(typeof(T));
    		if (isClass && hasComponent)
    			return true;
    		else
    		{
    			foreach (var @interface in typeof(T).GetInterfaces())
    			{
    				foreach (var component in container.ResolveAll(@interface))
    				{
    					if (component is T)
    					{
    						RegisterTypeAsAdditionalInterface(container, @interface, typeof(T));
    						return true;
    					}
    				}
    			}
    		}
    		return false;
    	}

    	static void RegisterTypeAsAdditionalInterface(IWindsorContainer container, Type @interface, Type type)
    	{
    		var handler = container.Kernel.GetHandler(@interface);
    		var kernel = container.Kernel as IKernelInternal;
    		kernel.RegisterHandlerForwarding(type, handler.ComponentModel.Name);
    	}

    	class CustomRegistration : IRegistration
    	{

    		#region IRegistration Members

    		public void Register(IKernel kernel)
    		{
    		}

    		#endregion
    	}

    	/// <summary>
    	/// Called when the parent helper needs to register a mock
    	/// </summary>
    	/// <param name="type"></param>
    	/// <param name="key"></param>
    	/// <param name="instance"></param>
    	public virtual void RegisterInstance(Type type, string key, object instance)
    	{
    		// TODO: write tests for this method
    		Container.Register(Component.For(type).Instance(instance).Named(key));
    	}

    	/// <summary>
    	/// Register a single instance for all of these services.
    	/// </summary>
    	/// <param name="services"></param>
    	/// <param name="instance"></param>
    	public virtual void RegisterInstance(Type[] services, object instance)
    	{
    		Container.Register(
    		                   Component.For(services).Instance(instance)
    			);
    	}
    }
}
