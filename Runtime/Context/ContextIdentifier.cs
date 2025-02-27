﻿using System;
using System.Collections.Generic;

namespace WhiteSparrow.Shared.DependencyInjection.Context
{
	public class ContextIdentifier
	{
		public readonly string Name;
		protected ContextIdentifier(string name)
		{
			this.Name = name;
		}
		
		public static implicit operator ContextIdentifier(string input)
		{
			return Get(input);
		}
		
		public static implicit operator ContextIdentifier(int intContext)
		{
			return Get(intContext);
		}

		public static implicit operator ContextIdentifier(Enum enumContext)
		{
			return Get(enumContext);
		}

		public static ContextIdentifier FromObject(object objectContext)
		{
			return Get(objectContext);
		}

#region Static Registry

		private static Dictionary<object, ContextIdentifier> s_TargetToContext = new Dictionary<object, ContextIdentifier>();

		private static ContextIdentifier Get(object target)
		{
			if (s_TargetToContext.TryGetValue(target, out var context))
				return context;

			if (target is ContextIdentifier contextIdentifier)
				context = contextIdentifier;
			else
				context = new ContextIdentifier(target.ToString());
			s_TargetToContext[target] = context;
			return context;
		}

#endregion

	}

	public class AbstractInjectionContext
	{
		
	}
}