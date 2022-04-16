using System.Collections.Generic;
using WhiteSparrow.Shared.DependencyInjection.Context;

namespace WhiteSparrow.Shared.DependencyInjection.Containers
{
	public class ContainersMap
	{
		private Dictionary<ContextIdentifier, InjectionContainer> m_ContainersByContext;
		
		public ContainersMap()
		{
			m_ContainersByContext = new Dictionary<ContextIdentifier, InjectionContainer>();
		}

		public InjectionContainer Get(ContextIdentifier context)
		{
			if (m_ContainersByContext.TryGetValue(context, out var container))
				return container;

			container = InjectionContainer.Create(context);
			m_ContainersByContext[context] = container;
			return container;
		}

	}
}