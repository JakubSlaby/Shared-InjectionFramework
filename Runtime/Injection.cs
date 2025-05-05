using System;
using UnityEngine;
using WhiteSparrow.Shared.DependencyInjection.Containers;

namespace WhiteSparrow.Shared.DependencyInjection
{
	public static class Injection
	{
		private static ContextMap s_Context = new ContextMap();
		public static ContextMap Context => s_Context;

		public static void Inject(this object instance)
		{
			if(instance is IInjectedType injectedType)
			{
				Debug.Log("YYYAYYY");
				injectedType._Inject();
				return;
			}
			
			InjectLogic.Inject(instance);
		}

		public static void Inject(this object instance, IInjectionContainer container)
		{
			InjectLogic.Inject(instance, container);
		}

		public static void Inject(this object instance, Func<ContextMap, IInjectionContainer> context)
		{
			InjectLogic.Inject(instance, context.Invoke(Context));
		}
	}
}