using System;
using System.Collections.Generic;
using UnityEngine;
using WhiteSparrow.Shared.DependencyInjection.Context;

namespace WhiteSparrow.Shared.DependencyInjection.Containers
{
	public interface IInjectionContainer
	{
		IInjectionContainer Create();
		bool Has<T>();
		bool Has(object key);

		T Map<T>();
		T Map<T>(T instance);
		object Map(Type type, object instance);

		T Get<T>();
		object Get(object type);

		void Clean();
		void Destroy();
		string name { get; }
	}
	
	public class InjectionContainer : MonoBehaviour, IInjectionContainer
	{
		internal static InjectionContainer Create(ContextMap contextMap, ContextIdentifier context)
		{
			GameObject gameObject = new GameObject($"[Injection: {context.Name}]");
			GameObject.DontDestroyOnLoad(gameObject);
			InjectionContainer container = gameObject.AddComponent<InjectionContainer>();
			container.Context = context;
			container.m_ContextMap = contextMap;
			return container;
		}

		private ContextMap m_ContextMap;
		public ContextIdentifier Context { get; private set; }


		private Dictionary<object, IInstanceBinding> m_instanceMap = new Dictionary<object, IInstanceBinding>();

#if UNITY_EDITOR
		internal event Action<object, object> OnMappingAdded;
		internal event Action<object, object> OnMappingRemoved;
#endif
		internal event Action<InjectionContainer> OnContainerDestroy;


		internal Dictionary<object, IInstanceBinding> Mapping
		{
			get => m_instanceMap;
		}

	
		
		private bool m_Created;
		public IInjectionContainer Create()
		{
			m_Created = true;
			this.Map<InjectionContainer>(this);
			this.Map<IInjectionContainer>(this);
			return this;
		}


		public bool Has<T>()
		{
			if (!m_Created)
				return false;
			return m_instanceMap.ContainsKey(typeof(T));
		}

		public bool Has(object key)
		{
			if (!m_Created)
				return false;
			return m_instanceMap.ContainsKey(key);
		}
		
		public T Map<T>()
		{
			if (!m_Created)
				return default;
			
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
			if (!m_Created)
			{
				Debug.LogError($"Not done {this.Context.Name}");
				return default;
			}
			
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
			if (!m_Created)
				return default;
			
			if (Has(type))
				throw new InjectionMappingAlreadyExistsException(this, type);
			
			IInternalInstanceBinding binding = CreateBinding(type);
			binding.SetInstance(instance);
			m_instanceMap[type] = binding;
			return instance;
		}
		

		private IInternalInstanceBinding CreateBinding(Type type)
		{
			if (typeof(MonoBehaviour).IsAssignableFrom(type))
				return new MonoBehaviourInstanceBinding();
			if (typeof(ScriptableObject).IsAssignableFrom(type))
				return new ScriptableObjectInstanceBinding();
			
			return new GenericInstanceBinding();
		}


		public T Get<T>()
		{
			if (!m_Created)
				return default;
			
			if (m_instanceMap.TryGetValue(typeof(T), out var binding))
				return (T)binding.Instance;

			if (m_ContextMap.FallbackContainer != null && m_ContextMap.FallbackContainer != this)
				return m_ContextMap.FallbackContainer.Get<T>();
			
			return default(T);
		}

		public object Get(object type)
		{
			if (!m_Created)
				return default;
			
			if (m_instanceMap.TryGetValue(type, out var binding))
				return binding.Instance;

			if (m_ContextMap.FallbackContainer != null && m_ContextMap.FallbackContainer != this)
				return m_ContextMap.FallbackContainer.Get(type);
			
			return null;
		}
		
		public void Clean()
		{
			if (!m_Created)
				return;
			
			m_instanceMap.Clear();
		}

		public void Destroy()
		{
			var callback = OnContainerDestroy;
			callback?.Invoke(this);
			OnContainerDestroy = null;
		}


		private void OnDestroy()
		{
			Clean();
		}
	}

	
	
}