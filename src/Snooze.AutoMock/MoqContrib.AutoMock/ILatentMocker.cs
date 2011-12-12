using System;
using Moq;

namespace Snooze.Mspecc.MoqContrib.AutoMock
{
	internal interface ILatentMocker
	{
		Mock CreateUnregisteredMock(Type type);
		void MockMyDependencies(Type type);
	}
}
