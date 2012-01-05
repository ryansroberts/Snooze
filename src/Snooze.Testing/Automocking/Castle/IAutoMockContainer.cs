using Castle.Windsor;
using Moq;

namespace Snooze.AutoMock.Castle
{
    /// <summary>
    /// Interface for an auto-mocking Castle.Windsor container
    /// </summary>
    public interface IAutoMockContainer : MoqContrib.AutoMock.IAutoMockContainer, IWindsorContainer
    {
        Mock<T> CreateMock<T>() where T : class ;
        T Inject<T>(T instance);
        //void InjectArray<T>(T[] objects);
    }
}
