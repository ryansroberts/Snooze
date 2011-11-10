using System;
using System.Collections;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.Windsor;
using Machine.Specifications;
using Moq;

namespace Snooze.MSpec
{
	public class LazyComponentAutoMocker : ILazyComponentLoader
	{
		public class MoqFactory
		{
			private readonly Type mockOpenType;

			public MoqFactory()
			{
				Assembly Moq = Assembly.Load("Moq");
				mockOpenType = Moq.GetType("Moq.Mock`1");
				if (mockOpenType == null)
					throw new InvalidOperationException("Unable to find Type Moq.Mock<T> in assembly " + Moq.Location);
			}

			public object CreateMock(Type type)
			{
				Type closedType = mockOpenType.MakeGenericType(new[] { type });
				PropertyInfo objectProperty = closedType.GetProperty("Object", type);
				object instance = Activator.CreateInstance(closedType);
				return objectProperty.GetValue(instance, null);
			}

			public object CreateMockThatCallsBase(Type type, object[] args)
			{
				Type closedType = mockOpenType.MakeGenericType(new[] { type });
				PropertyInfo callBaseProperty = closedType.GetProperty("CallBase", typeof(bool));
				PropertyInfo objectProperty = closedType.GetProperty("Object", type);
				ConstructorInfo constructor = closedType.GetConstructor(new[] { typeof(object[]) });
				object instance = constructor.Invoke(new[] { args });
				callBaseProperty.SetValue(instance, true, null);
				return objectProperty.GetValue(instance, null);
			}
		}

		protected MoqFactory factory = new MoqFactory();

		public IRegistration Load(string key, Type service, IDictionary arguments)
		{
			return Component.For(service).Instance(factory.CreateMock(service));
		}
	}

	public class MoqCastleAutoMocker<TUnderTest> where TUnderTest : class
	{
		IWindsorContainer container = new WindsorContainer();

		public MoqCastleAutoMocker()
		{
			container.Register(Component.For<LazyComponentAutoMocker>());
		}

		public T Get<T>() { return container.Resolve<T>(); }
		public T Inject<T>(T item) 
		{ 
			container.Register(Component.For<T>().Instance(item));
			return item;
		}

		public void InjectArray<T>(T[] stubs)
		{
			foreach (var stub in stubs)
			{
				container.Register(Component.For<T>().Instance(stub));
			}
		}

		TUnderTest classUnderTest;

		public TUnderTest ClassUnderTest
		{
			get
			{
				if (classUnderTest == null)
				{
					container.Register(Component.For<TUnderTest>());
					classUnderTest = container.Resolve<TUnderTest>();
				}

				return classUnderTest;
			}
		}
	}

    public class with_auto_mocking<TUnderTest> where TUnderTest : class
    {
    	protected static MoqCastleAutoMocker<TUnderTest> autoMocker = new MoqCastleAutoMocker<TUnderTest>();


		public static Mock<TInterface> Stub<TInterface>() where TInterface : class
		{
			var mocked = autoMocker.Get<TInterface>();
			return Mock.Get(mocked);
		}

		protected static TUnderTest class_under_test { get { return autoMocker.ClassUnderTest; } }
    }
}