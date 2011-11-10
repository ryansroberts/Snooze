using Moq;
using Snooze.MSpec;

namespace Snooze.Testing
{
    public class with_nunit_auto_mocking<TUnderTest> where TUnderTest : class
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