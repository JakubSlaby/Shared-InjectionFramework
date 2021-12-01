using System;

namespace WhiteSparrow.Shared.DependencyInjection
{
	public class InjectAttribute : Attribute
	{
		public readonly int Context;
		
		public InjectAttribute(int context)
		{
			this.Context = context;
		}

		public InjectAttribute(object context)
		{
			if (context is IConvertible convertible)
			{
				this.Context = convertible.ToInt32(null);
			}
			else
			{
				throw new ArgumentException($"InjectAttribute property {nameof(context)} has to be convertible to int.");
			}
		}
	}
}