using System;
using WhiteSparrow.Shared.DependencyInjection.Containers;
using WhiteSparrow.Shared.DependencyInjection.Context;
using Object = UnityEngine.Object;

namespace WhiteSparrow.Shared.DependencyInjection
{
	public static class Injection
	{
		private static ContainersMap s_ContainersMap = new ContainersMap();
		public static ContainersMap Containers => s_ContainersMap;

		private static InjectionContainer[] s_PreviousContainersMap = new InjectionContainer[8];

		public static void Inject(this object instance)
		{
			InjectLogic.Inject(instance);
		}

		public static InjectionContainer Map<T>(ContextIdentifier context, T instance)
		{
			InjectionContainer container = Containers.Get(context);
			container.Map<T>(instance);
			return container;
		}

		public static T Map<T, TContext>(TContext context) where TContext : struct, IConvertible
			where T : Object
		{
			return Map<T>(context.ToInt32(null));
		}
		
		public static T Map<T>(ContextIdentifier context)
		where T : Object
		{
			InjectionContainer container = Containers.Get(context);
			return container.Map<T>();
		}

		public static T Get<T>(ContextIdentifier context)
		{
			InjectionContainer container = Containers.Get(context);
			return container.Get<T>();
		}


	}
}