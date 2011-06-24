using Machine.Specifications;
using Moq;
using StructureMap.AutoMocking;

namespace Snooze.Testing
{
    public class with_mspec_auto_mocking<TUnderTest> where TUnderTest : class
    {
        public static MoqAutoMocker<TUnderTest> autoMocker;

        Establish automocking = () => autoMocker = new MoqAutoMocker<TUnderTest>();

        public static Mock<TInterface> Stub<TInterface>() where TInterface : class
        {
            var mocked = autoMocker.Get<TInterface>();
            return Mock.Get(mocked);
        }

        protected static TUnderTest class_under_test { get { return autoMocker.ClassUnderTest; } }
    }
}