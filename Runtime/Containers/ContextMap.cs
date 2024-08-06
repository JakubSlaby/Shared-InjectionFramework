using System.Collections.Generic;
using UnityEngine;
using WhiteSparrow.Shared.DependencyInjection.Context;

namespace WhiteSparrow.Shared.DependencyInjection.Containers
{
	public class ContextMap : IContextMap
	{
		private Dictionary<ContextIdentifier, InjectionContainer> m_ContainersByContext  = new Dictionary<ContextIdentifier, InjectionContainer>();
		
		public IContextMap Impl => this;
		
		IInjectionContainer IContextMap.Get(ContextIdentifier context)
		{
			if (m_ContainersByContext.TryGetValue(context, out var container))
				return container;
			
			container = InjectionContainer.Create(this, context);
			container.OnContainerDestroy += OnContainerDestroyed;
			m_ContainersByContext[context] = container;
			return container;
		}

		private void OnContainerDestroyed(InjectionContainer container)
		{
			container.OnContainerDestroy -= OnContainerDestroyed;
			m_ContainersByContext.Remove(container.Context);
		}

		void IContextMap.Destroy(ContextIdentifier context)
		{
			if (!m_ContainersByContext.TryGetValue(context, out var container))
				return;
			
			m_ContainersByContext.Remove(context);
			container.Destroy();
		}

		void IContextMap.DestroyAll()
		{
			var array = new InjectionContainer[m_ContainersByContext.Count];
			m_ContainersByContext.Values.CopyTo(array, 0);
			m_ContainersByContext.Clear();
			foreach (var container in array)
			{
				container.Destroy();
			}
		}
	}

	public interface IContextMap
	{
		IInjectionContainer Get(ContextIdentifier context);
		void Destroy(ContextIdentifier context);

		void DestroyAll();
	}
}