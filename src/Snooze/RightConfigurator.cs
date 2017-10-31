using System;
using System.Collections.Generic;
using Glue;
using Glue.Converters;

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

		public RightConfigurator<TLeft,TRight> Convert<TProperty,TDest>(Func<TProperty,TDest> convertor)
		{
			mapping.AddConverter(new QuickConverter<TProperty,TDest>(convertor));
			return this;
		}


		public RightConfigurator<TLeft, TRight> Configure(Action<Mapping<TLeft, TRight>> dothis) { configuration.Add(dothis);
			return this;
		}

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