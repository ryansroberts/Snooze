using System;
using Moq;

namespace Snooze.AutoMock.Castle.MoqContrib.AutoMock
{
	internal interface ILatentMocker
	{
		Mock CreateUnregisteredMock(Type type);
		void MockMyDependencies(Type type);
	}
}
