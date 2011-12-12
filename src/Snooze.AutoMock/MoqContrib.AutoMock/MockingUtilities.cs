using Moq;

namespace Snooze.AutoMock.Castle.MoqContrib.AutoMock
{
    /// <summary>
    /// tidbits of functionality that are useful internally
    /// </summary>
    internal static class MockingUtilities
    {
        /// <summary>
        /// If the object is actually a mocked object, return the mock that controlls
        /// this mocked object.
        /// </summary>
        /// <param name="possiblyMocked"></param>
        /// <returns>A mock, or null if the object is not mocked</returns>
        public static Mock GetMockFor(object possiblyMocked)
        {
            if (possiblyMocked is IMocked)
                return ((IMocked)possiblyMocked).Mock;
            else return null;
        }
    }
}
