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
		internal static InjectionContainer Create(ContextIdentifier context)
		{
			GameObject gameObject = new GameObject($"[Injection: {context.Name}]");
			GameObject.DontDestroyOnLoad(gameObject);
			InjectionContainer container = gameObject.AddComponent<InjectionContainer>();
			container.Context = context;
			return container;
		}

		public ContextIdentifier Context { get; private set; }


		private Dictionary<object, IInstanceBinding> m_instanceMap = new Dictionary<object, IInstanceBinding>();

#if UNITY_EDITOR
		internal event Action<object, object> OnMappingAdded;
		internal event Action<object, object> OnMappingRemoved;
		internal event Action<InjectionContainer> OnContainerDestroy;
#endif

		internal Dictionary<object, IInstanceBinding> Mapping
		{
			get => m_instanceMap;
		}

		private bool m_Initialized;
		public IInjectionContainer Create()
		{
			m_Initialized = true;
			this.Map<InjectionContainer>(this);
			this.Map<IInjectionContainer>(this);
			return this;
		}


		public bool Has<T>()
		{
			if (!m_Initialized)
				return false;
			return m_instanceMap.ContainsKey(typeof(T));
		}

		public bool Has(object key)
		{
			if (!m_Initialized)
				return false;
			return m_instanceMap.ContainsKey(key);
		}
		
		public T Map<T>()
		{
			if (!m_Initialized)
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
			if (!m_Initialized)
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
			if (!m_Initialized)
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
			if (!m_Initialized)
				return default;
			
			if (m_instanceMap.TryGetValue(typeof(T), out var binding))
				return (T)binding.Instance;
			return default(T);
		}

		public object Get(object type)
		{
			if (!m_Initialized)
				return default;
			
			if (m_instanceMap.TryGetValue(type, out var binding))
				return binding.Instance;
			return null;
		}
		
		public void Clean()
		{
			if (!m_Initialized)
				return;
			
			m_instanceMap.Clear();
		}

		public void Destroy()
		{
			var callback = OnContainerDestroy;
			OnContainerDestroy = null;
			callback?.Invoke(this);
		}

		private void OnDestroy()
		{
			Clean();
		}
		
	}

	
	
}