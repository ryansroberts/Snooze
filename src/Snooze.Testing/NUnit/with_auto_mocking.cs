using Moq;
using System;
using NUnit.Framework;
using StructureMap.AutoMocking;

namespace Snooze.Nunit
{﻿
    public class with_nunit_auto_mocking<TUnderTest> : with_auto_mocking<TUnderTest>
        where TUnderTest : class
    {        
    }

    public class with_auto_mocking<TUnderTest> where TUnderTest : class
    {
        protected static SpecificationException err;
        public static MoqAutoMocker<TUnderTest> autoMocker;

        public with_auto_mocking()
        {
            autoMocker = new MoqAutoMocker<TUnderTest>();
        }

        [TearDown]
        void CleanUp()
        {
            err = null;
            autoMocker = null;
        }

        public static Mock<TInterface> Stub<TInterface>() where TInterface : class
        {
            var mocked = autoMocker.Get<TInterface>();
            return Mock.Get(mocked);
        }

        protected static void Execute(Action<TUnderTest> action)
        {
            try
            {
                action(class_under_test);
            }
            catch(Exception e)
            {
                err = new SpecificationException(e.Message);
            }
        }

        protected static void Inject<T>(T instance)
        {
            autoMocker.Inject(instance);
        }

        protected static void InjectArray<T>(T[] objects)
        {
            autoMocker.InjectArray(objects);
        }

        protected static TUnderTest class_under_test { get { return autoMocker.ClassUnderTest; } }

        protected static TUnderTest Service { get { return class_under_test; } }
    }
}
