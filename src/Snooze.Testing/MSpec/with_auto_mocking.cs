using System;
using Machine.Specifications;
using Moq;
using Snooze.AutoMock.Castle;

namespace Snooze.MSpec
{	
    public class with_auto_mocking<TUnderTest> where TUnderTest : class
    {
        protected static Exception err;
    	protected static AutoMockContainer<TUnderTest> autoMocker;

		Establish container = () => autoMocker = new AutoMockContainer<TUnderTest>();

        protected static Mock<TInterface> Stub<TInterface>() where TInterface : class
		{
		    return autoMocker.CreateMock<TInterface>();
		}

        protected static void Execute(Action<TUnderTest> action)
        {
            err = Catch.Exception(() => action(class_under_test));
        }


        protected static T Inject<T>(T instance)
        {
            return autoMocker.Inject(instance);
        }

        protected static void InjectArray<T>(T[] objects)
        {
            autoMocker.InjectArray(objects);
        }

		protected static TUnderTest class_under_test { get { return autoMocker.ClassUnderTest; } }

        protected static TUnderTest Service { get { return class_under_test; } }


        Cleanup cleanup = () => 
        {
            err = null;
            autoMocker = null;
        };
	}
}