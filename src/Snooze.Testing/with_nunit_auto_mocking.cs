using Moq;
using Snooze.MSpec;

namespace Snooze.Testing
{
    public class with_nunit_auto_mocking<TUnderTest> : with_auto_mocking<TUnderTest> where TUnderTest : class
    {
    	
    }
}