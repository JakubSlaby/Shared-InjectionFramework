using System;
using WhiteSparrow.Shared.DependencyInjection.Containers;

namespace WhiteSparrow.Shared.DependencyInjection
{
	public static class Injection
	{
		private static ContextMap s_Context = new ContextMap();
		public static ContextMap Context => s_Context;

		public static void Inject(this object instance)
		{
			InjectLogic.Inject(instance);
		}

		public static void Inject(this object instance, Func<ContextMap, IInjectionContainer> context)
		{
			InjectLogic.Inject(instance, context.Invoke(Context));
		}
	}
}