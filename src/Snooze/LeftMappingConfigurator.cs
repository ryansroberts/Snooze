using System;

namespace Snooze
{
	public class LeftMappingConfigurator<TLeft>
	{
		TLeft item;

		public LeftMappingConfigurator(TLeft item) 
		{ 
			this.item = item; 
		}

		public RightConfigurator<TLeft, TRight> To<TRight>()
		{
			return new RightConfigurator<TLeft, TRight>(item);	
		}

		public RightConfigurator<TLeft, TRight> To<TRight>(Func<TLeft,TRight> projection)
		{
			return new RightConfigurator<TLeft, TRight>(item,projection(item));
		}
	}
}