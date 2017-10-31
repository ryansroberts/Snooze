using Machine.Specifications;
using Snooze.MSpec;

namespace Snooze
{
    public class MspecMockingSpecs
    {
        [Subject(typeof(with_auto_mocking<ArrayReturner>))]
        public class when_injecting_an_array : with_auto_mocking<ArrayReturner>
        {
            static ISomethingInterface[] result;

            Establish context = () => InjectArray(new ISomethingInterface[] { new MyClassWithVirtuals { something = "something" }, new MyClassWithVirtuals2 { something = "something else" } });

            Because of = () => result = class_under_test.ReturnArray();

            It should_return_something = () => result.ShouldNotBeNull();

            It should_have_two_elements = () => result.Length.ShouldEqual(2);

            It should_return_them_in_the_input_order = () => result[1].something.ShouldEqual("something else");


        }

        [Subject(typeof(with_auto_mocking<ArrayReturner>))]
        public class when_depending_on_an_array_not_injected : with_auto_mocking<ArrayReturner>
        {
            static ISomethingInterface[] result;

            Establish context = () => InjectArray(new ISomethingInterface[] { new MyClassWithVirtuals { something = "something" }, new MyClassWithVirtuals2 { something = "something else" } });

            Because of = () => result = class_under_test.ReturnArray();

            It should_return_something = () => result.ShouldNotBeNull();

            It should_have_two_elements = () => result.Length.ShouldEqual(2);

            It should_return_them_in_the_input_order = () => result[1].something.ShouldEqual("something else");


        }


        [Subject(typeof(with_auto_mocking<Depender>))]
        public class depender_when_injecting : with_auto_mocking<Depender>
        {
            static ISomethingInterface result;

            Establish context = () => Inject<ISomethingInterface>(new MyClassWithVirtuals { something = "something" });

            Because of = () => result = class_under_test.ReturnSomething();

            It should_return_something = () => result.ShouldNotBeNull();

            It should_override_something = () => result.something.ShouldEqual("something");


        }

        [Subject(typeof(with_auto_mocking<Depender>))]
        public class depender_when_not_injecting : with_auto_mocking<Depender>
        {
            static ISomethingInterface result;

            Because of = () => result = class_under_test.ReturnSomething();

            It should_return_something = () => result.ShouldNotBeNull();

            It should_return_null = () => result.something.ShouldBeNull();


        }



        [Subject(typeof(with_auto_mocking<Returner>))]
        public class returner_when_injecting : with_auto_mocking<Returner>
        {
            static MyClassWithVirtuals result;

            Establish context = () => Inject(new MyClassWithVirtuals {something = "something"});

            Because of = () => result = class_under_test.ReturnClassWithVirtuals();

            It should_return_something = () => result.ShouldNotBeNull();

            It should_override_something = () => result.something.ShouldEqual("something");


        }

        [Subject(typeof(with_auto_mocking<Returner>))]
        public class returner_when_not_injecting : with_auto_mocking<Returner>
        {
            static MyClassWithVirtuals result;

            Because of = () => result = class_under_test.ReturnClassWithVirtuals();

            It should_return_something = () => result.ShouldNotBeNull();

            It should_return_null = () => result.something.ShouldBeNull();


        }


        [Subject(typeof(with_auto_mocking<Returner>))]
        public class when_stubbing_a_virtual_with_auto_mockery : with_auto_mocking<Returner>
        {
            static MyClassWithVirtuals result;

            Establish context = () => Stub<MyClassWithVirtuals>().SetupGet(s => s.something).Returns("something");

            Because of = () => result = class_under_test.ReturnClassWithVirtuals();

            It should_return_something = () => result.ShouldNotBeNull();

            It should_override_something = () => result.something.ShouldEqual("something");


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