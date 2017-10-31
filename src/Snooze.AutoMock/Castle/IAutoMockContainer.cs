using Castle.Windsor;

namespace Snooze.AutoMock.Castle
{
    /// <summary>
    /// Interface for an auto-mocking Castle.Windsor container
    /// </summary>
    public interface IAutoMockContainer : MoqContrib.AutoMock.IAutoMockContainer, IWindsorContainer
    {
    }
}
