using Moq;
using Snooze.AutoMock.Castle;

namespace Snooze.Nunit
{﻿
    public class with_nunit_auto_mocking<TUnderTest> : with_auto_mocking<TUnderTest>
        where TUnderTest : class
    {        
    }

    public class with_auto_mocking<TUnderTest> where TUnderTest : class
    {
        protected static AutoMockContainer<TUnderTest> autoMocker = new AutoMockContainer<TUnderTest>();

        public static Mock<TInterface> Stub<TInterface>() where TInterface : class
        {
            var mocked = autoMocker.GetService<TInterface>();
            return Mock.Get(mocked);
        }

        protected static TUnderTest class_under_test { get { return autoMocker.ClassUnderTest; } }
    }
}
