using System;
using System.Collections.Generic;
using UnityEngine;
using WhiteSparrow.Common.Reflection;
using WhiteSparrow.Shared.DependencyInjection.Context;

namespace WhiteSparrow.Shared.DependencyInjection.Containers
{
	public class InjectionContainer : MonoBehaviour
	{
		internal static InjectionContainer Create(ContextIdentifier context)
		{
			GameObject gameObject = new GameObject(context.Name);
			InjectionContainer container = gameObject.AddComponent<InjectionContainer>();
			container.Context = context;
			return container;
		}

		public ContextIdentifier Context { get; private set; }


		private Dictionary<object, IInstanceBinding> m_instanceMap = new Dictionary<object, IInstanceBinding>();

#if UNITY_EDITOR
		internal event Action<object, object> OnMappingAdded;
		internal event Action<object, object> OnMappingRemoved;
#endif

		internal Dictionary<object, IInstanceBinding> Mapping
		{
			get => m_instanceMap;
		}

		public bool Has<T>()
		{
			return m_instanceMap.ContainsKey(typeof(T));
		}

		public bool Has(object key)
		{
			return m_instanceMap.ContainsKey(key);
		}
		
		public T Map<T>()
		{
			Type type = typeof(T);
			if (Has(type))
				throw new InjectionMappingAlreadyExistsException(this, type);
			
			IInternalInstanceBinding binding = CreateBinding(type);

			throw new Exception("Didn't implement creation of instances yet.");
			
			m_instanceMap[type] = binding;

			return (T)binding.Instance;
		}

		public T Map<T>(T instance)
		{
			
			Type type = typeof(T);
			if (Has(type))
				throw new InjectionMappingAlreadyExistsException(this, type);
				
			IInternalInstanceBinding binding = CreateBinding(type);
			binding.SetInstance(instance);
			m_instanceMap[type] = binding;
			return instance;
		}

		public object Map(Type type, object instance)
		{
			if (Has(type))
				throw new InjectionMappingAlreadyExistsException(this, type);
			
			IInternalInstanceBinding binding = CreateBinding(type);
			binding.SetInstance(instance);
			m_instanceMap[type] = binding;
			return instance;
		}
		

		private IInternalInstanceBinding CreateBinding(Type type)
		{
			if (type.InheritsFrom(typeof(MonoBehaviour)))
				return new MonoBehaviourInstanceBinding();
			if (type.InheritsFrom<ScriptableObject>())
				return new ScriptableObjectInstanceBinding();
			
			return new GenericInstanceBinding();
		}


		public T Get<T>()
		{
			if (m_instanceMap.TryGetValue(typeof(T), out var binding))
				return (T)binding.Instance;
			return default(T);
		}

		public object Get(object type)
		{
			if (m_instanceMap.TryGetValue(type, out var binding))
				return binding.Instance;
			return null;
		}
		
		public void Clean()
		{
			m_instanceMap.Clear();
		}

		private void OnDestroy()
		{
			Clean();
		}
		
	}

	
	
}