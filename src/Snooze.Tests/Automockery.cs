using System;
using Machine.Specifications;
using Snooze.AutoMock.Castle;

namespace Snooze
{

	public interface IAbstractDependency
	{
		
	}

	public class SubConcreteDependency
	{}

	public interface ISubAbstractDependency {}

	public class ConcreteDependency
	{
		public  ConcreteDependency(SubConcreteDependency subConcrete,ISubAbstractDependency subabstract)
		{}
	}

	public class Root
	{
		public readonly IAbstractDependency abstractDependency;
		public readonly ConcreteDependency concreteDependency;
		public readonly Func<ConcreteDependency> factory;

		public Root(IAbstractDependency abstractDependency,ConcreteDependency concreteDependency,Func<ConcreteDependency> factory)
		{
			this.abstractDependency = abstractDependency;
			this.concreteDependency = concreteDependency;
			this.factory = factory;
		}
	}

	public class automocking_abstract
	{
		static AutoMockContainer<Root> mocked;

		Establish context = () => mocked = new AutoMockContainer<Root>();

		It has_built_object = () => mocked.ClassUnderTest.ShouldNotBeNull();

		It has_supplied_factory = () => mocked.ClassUnderTest.factory.ShouldNotBeNull();
	}


}