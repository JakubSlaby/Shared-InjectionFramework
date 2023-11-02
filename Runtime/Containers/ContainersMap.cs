using System;
using System.Collections.Generic;
using UnityEngine;
using WhiteSparrow.Shared.DependencyInjection.Context;

namespace WhiteSparrow.Shared.DependencyInjection.Containers
{
	public class ContainersMap
	{
		private Dictionary<ContextIdentifier, InjectionContainer> m_ContainersByContext  = new Dictionary<ContextIdentifier, InjectionContainer>();

		public InjectionContainer Create(ContextIdentifier context)
		{
			if (m_ContainersByContext.TryGetValue(context, out var container))
			{
				Debug.LogError($"InjectionContainer with identifier={container} already exists. Either use Get or dispose the existing container first.");
				return container;
			}
			
			container = InjectionContainer.Create(context);
			m_ContainersByContext[context] = container;
			return container;
		}
		
		public InjectionContainer Get(ContextIdentifier context)
		{
			if (m_ContainersByContext.TryGetValue(context, out var container))
				return container;
			
			Debug.LogError($"InjectionContainer with identifier={container} doesn't exist, create the container first.");
			return null;
		}

		public InjectionContainer Get(Enum enumContext)
		{
			return Get((ContextIdentifier)enumContext);
		}

	}
}