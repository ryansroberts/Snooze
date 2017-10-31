using System;

namespace Snooze
{
	public class LeftMappingConfigurator<TLeft>
	{
		readonly TLeft item;

		public LeftMappingConfigurator(TLeft item)
		{
			this.item = item;
		}

		public RightConfigurator<TLeft, TRight> To<TRight>() where TRight : class {
			return new RightConfigurator<TLeft, TRight>(item);
		}

		public RightConfigurator<TLeft, TRight> To<TRight>(Func<TLeft,TRight> projection) where TRight : class {
			return new RightConfigurator<TLeft, TRight>(item,projection(item));
		}
	}


}