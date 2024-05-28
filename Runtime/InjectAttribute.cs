using System;
using WhiteSparrow.Shared.DependencyInjection.Context;

namespace WhiteSparrow.Shared.DependencyInjection
{
	
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
	public class InjectAttribute : Attribute
	{
		public readonly ContextIdentifier Context;

		public InjectAttribute(int context)
		{
			Context = context;
		}

		public InjectAttribute(ContextIdentifier context)
		{
			Context = context;
		}		
		public InjectAttribute(object context)
		{
			if(context is Enum enumValue)
			{
				Context = enumValue;
			}
			else if (context is IConvertible convertible)
			{
				Context = convertible.ToInt32(null);
			}
			else
			{
				Context = ContextIdentifier.FromObject(context);
			}
		}
	}
}