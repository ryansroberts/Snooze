using System;
using System.Collections.Generic;
using Glue;

namespace Snooze
{
	public class RightConfigurator<TLeft, TRight> where TRight : class
	{
		readonly TRight right;
		readonly TLeft left;
		Mapping<TLeft, TRight> mapping = new Mapping<TLeft, TRight>();
		readonly IList<Action<Mapping<TLeft, TRight>>>  configuration = new List<Action<Mapping<TLeft, TRight>>>();

		public RightConfigurator(TLeft left) 
		{ 
			this.left = left;
			configuration.Add(m => m.AutoRelateEqualNames(true,true));
		}

		public RightConfigurator(TLeft left,TRight right) : this(left) 
		{
			this.right = right;
		}

		public void Configure(Action<Mapping<TLeft, TRight>> dothis) { configuration.Add(dothis); }

		public void Run()
		{
			foreach (var action in configuration)
				action(mapping);

			if (right == null) mapping.Map(left); else mapping.Map(left, right);
		}

		public TRight Item
		{
			get
			{
				foreach (var action in configuration)
					action(mapping);

				return right == null ? mapping.Map(left) : mapping.Map(left, right);
			}
		}
	}
}