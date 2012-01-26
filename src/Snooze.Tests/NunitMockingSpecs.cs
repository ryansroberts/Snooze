using NUnit.Framework;
using Snooze.Nunit;
using Should;

namespace Snooze
{
    public class NUnitMockingSpecs
    {
        public class when_injecting_an_array : with_auto_mocking<ArrayReturner>
        {
            static ISomethingInterface[] result;

            [SetUp]
            public void setup()
            {
                InjectArray(new ISomethingInterface[] { new MyClassWithVirtuals { something = "something" }, new MyClassWithVirtuals2 { something = "something else" } });
                result = class_under_test.ReturnArray();
            }

            [Test]
            public void should_return_something() { result.ShouldNotBeNull(); }

            [Test]
            public void should_have_two_elements() { result.Length.ShouldEqual(2);}

            [Test]
            public void should_return_them_in_the_input_order() { result[1].something.ShouldEqual("something else"); }
        }

        public class when_depending_on_an_array_not_injected : with_auto_mocking<ArrayReturner>
        {
            static ISomethingInterface[] result;

            [SetUp]
            public void setup()
            {
                InjectArray(new ISomethingInterface[] { new MyClassWithVirtuals { something = "something" }, new MyClassWithVirtuals2 { something = "something else" } });
                result = class_under_test.ReturnArray();
            }

            [Test]
            public void should_return_something() { result.ShouldNotBeNull(); }

            [Test]
            public void should_have_two_elements() { result.Length.ShouldEqual(2); }

            [Test]
            public void should_return_them_in_the_input_order() { result[1].something.ShouldEqual("something else"); }
        }

        public class depender_when_injecting : with_auto_mocking<Depender>
        {
            static ISomethingInterface result;

            [SetUp]
            public void setup()
            {
                Inject<ISomethingInterface>(new MyClassWithVirtuals { something = "something" });
                result = class_under_test.ReturnSomething();
            }

            [Test]
            public void should_return_something() { result.ShouldNotBeNull(); }

            [Test]
            public void should_override_something() { result.something.ShouldEqual("something"); }
        }
        
        public class depender_when_not_injecting : with_auto_mocking<Depender>
        {
            static ISomethingInterface result;

            [SetUp]
            public void setup()
            {
                result = class_under_test.ReturnSomething();
            }

            [Test]
            public void should_return_something() { result.ShouldNotBeNull(); }

            [Test]
            public void should_return_null() { result.something.ShouldBeNull(); }
        }

        public class returner_when_injecting : with_auto_mocking<Returner>
        {
            static MyClassWithVirtuals result;

            [SetUp]
            public void setup()
            {
                Inject(new MyClassWithVirtuals {something = "something"});
                result = class_under_test.ReturnClassWithVirtuals();
            }

            [Test]
            public void should_return_something() { result.ShouldNotBeNull(); }

            [Test]
            public void should_override_something() { result.something.ShouldEqual("something"); }
        }

        public class returner_when_not_injecting : with_auto_mocking<Returner>
        {
            static MyClassWithVirtuals result;

            [SetUp]
            public void setup()
            {
                result = class_under_test.ReturnClassWithVirtuals();
            }

            [Test]
            public void should_return_something() { result.ShouldNotBeNull(); }

            [Test]
            public void should_override_something() { result.something.ShouldBeNull(); }
        }


        public class when_stubbing_a_virtual_with_auto_mockery : with_auto_mocking<Returner>
        {
            static MyClassWithVirtuals result;

            [SetUp]
            public void setup()
            {
                Stub<MyClassWithVirtuals>().SetupGet(s => s.something).Returns("something");
                result = class_under_test.ReturnClassWithVirtuals();
            }

            [Test]
            public void should_return_something() { result.ShouldNotBeNull(); }

            [Test]
            public void should_override_something() { result.something.ShouldEqual("something"); }
        }

        public class Depender
        {
            protected readonly ISomethingInterface something;

            public Depender(ISomethingInterface something)
            {
                this.something = something;
            }

            public ISomethingInterface ReturnSomething()
            {
                return something;
            }
        }


        public class ArrayReturner
        {
            protected readonly ISomethingInterface[] inputs;

            public ArrayReturner(ISomethingInterface[] inputs)
            {
                this.inputs = inputs;
            }

            public ISomethingInterface[] ReturnArray()
            {
                return inputs;
            }
        }


        public class Returner
        {
            protected readonly MyClassWithVirtuals classWithVirtuals;

            public Returner(MyClassWithVirtuals classWithVirtuals)
            {
                this.classWithVirtuals = classWithVirtuals;
            }

            public MyClassWithVirtuals ReturnClassWithVirtuals()
            {
                return classWithVirtuals;
            }
        }

    
        public interface ISomethingInterface
        {
            string something { get; set; }
        }

        public class MyClassWithVirtuals : ISomethingInterface
        {
            public virtual string something { get; set; }
        }

        public class MyClassWithVirtuals2 : ISomethingInterface
        {
            public virtual string something { get; set; }
        }
    }
}