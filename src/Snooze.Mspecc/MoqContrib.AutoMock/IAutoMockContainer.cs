namespace Snooze.Mspecc.MoqContrib.AutoMock
{
    /// <summary>
    /// Interface that exposes mocking capabilities for a container
    /// </summary>
    public interface IAutoMockContainer
    {
        /// <summary>
        /// Add mocking capabilities
        /// </summary>
        IAutoMocker Mock { get; }
    }
}
