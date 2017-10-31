using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using System.Reflection;

namespace Snooze.AutoMock.Castle.MoqContrib.AutoMock
{
    internal class MockGenerator : MoqContrib.AutoMock.IMockGenerator
    {
		private List<Type> _invalidTypes = new List<Type>();

        /// <summary>
        /// Central location for creating mocks. The mocks created here can be cast
        /// to Mock&lt;T&gt; later.
        /// </summary>
        /// <param name="type">the type of mock to generate</param>
        /// <returns></returns>
        public virtual Mock Generate(Type type)
        {
            if (type == null)
                throw new ArgumentException("type");

			Mock ret = null;
			if (type.IsInterface)
				ret = InstantiateMock(type);
			else
				ret = ResolveConstructorAndInstantiateMock(type);

			// TODO: add some commonly wanted features. Maybe auto-property setup
            return ret;
        }

		private Mock ResolveConstructorAndInstantiateMock(Type type)
		{
			var ctors = type.GetConstructors().OrderByDescending(x => x.GetParameters().Length);
			foreach (var ctor in ctors)
			{
				var ret = InstantiateMockForConstructor(type, ctor);
				if (ret != null)
					return ret;
			}
			return null;
		}

		private Mock InstantiateMock(Type type)
		{
			var genericCtor = GetMockType(type).GetConstructor(new Type[0]);
			return (Mock)genericCtor.Invoke(new object[0]);
		}

		private Mock InstantiateMock(Type type, object[] parameters)
		{
			var genericCtor = GetMockType(type).GetConstructor(new Type[]{ typeof(object[])});
			return (Mock)genericCtor.Invoke(new object[] { parameters });
		}

		private static Type GetMockType(Type type)
		{
			return typeof(Mock<>).MakeGenericType(type);
		}

		private Mock InstantiateMockForConstructor(Type type, ConstructorInfo ctor)
		{
			var @params = ctor.GetParameters();
			var values = new object[@params.Length];

			for (int i = 0; i < @params.Length; i++)
			{
				values[i] = Generate(@params[i].ParameterType);
				if (values[i] == null)
					// Throwing exceptions for internal use is so messy. null means
					// nothing, so return null to say we couldn't find anything
					return null;

				values[i] = ((Mock)values[i]).Object;
			}

			return InstantiateMock(type, values);
		}
    }
}
