using System;
using System.Collections.Generic;
using UnityEngine;
using WhiteSparrow.Shared.DependencyInjection.Context;

namespace WhiteSparrow.Shared.DependencyInjection.Containers
{
	public interface IInjectionContainer
	{
		IInjectionContainer Create();
		bool IsCreated { get; }
		
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
		
		int Count { get; }

		IInjectionContainer Parent { get; }
		void AddChild(IInjectionContainer child);
		void RemoveChild(IInjectionContainer child);
		IInjectionContainer[] Children { get; }
	}
	
	public class InjectionContainer : ScriptableObject, IInjectionContainer
	{
		internal static InjectionContainer Create(ContextMap contextMap, ContextIdentifier context)
		{
			InjectionContainer container = ScriptableObject.CreateInstance<InjectionContainer>();
			container.Context = context;
			container.m_ContextMap = contextMap;
			container.name = $"InjectionContainer: {context.Name}";
			return container;
		}

		private ContextMap m_ContextMap;
		public ContextIdentifier Context { get; private set; }

		private List<IInjectionContainer> m_Children;
		private IInjectionContainer[] m_ChildrenCache;


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

		public bool IsCreated => m_Created;


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

			if (Parent != null)
				return Parent.Get<T>();
			
			return default(T);
		}

		public object Get(object type)
		{
			if (!m_Created)
				return default;
			
			if (m_instanceMap.TryGetValue(type, out var binding))
				return binding.Instance;

			if (Parent != null)
				return Parent.Get(type);

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
			if(Application.isPlaying)
				ScriptableObject.Destroy(this);
			else
				ScriptableObject.DestroyImmediate(this);
		}

		public int Count => m_instanceMap.Count;
		
		
		public IInjectionContainer Parent { get; private set; }
		public void AddChild(IInjectionContainer child)
		{
			if (child is not InjectionContainer injectionContainer)
				throw new Exception("Unhandled type of Injection Container introduced - not supporting parenting.");

			if (injectionContainer.Parent == this)
				return;
			
			if (injectionContainer.Parent != null)
				throw new Exception($"Injection Context {child.name} already has a parent mapped");

			injectionContainer.Parent = this;
			m_Children ??= new List<IInjectionContainer>();
			m_Children.Add(child);

			m_ChildrenCache = null;
		}

		public void RemoveChild(IInjectionContainer child)
		{
			if (m_Destroyed)
				return;
			
			if(m_Children == null)
				return;
			
			if (child is not InjectionContainer injectionContainer)
				throw new Exception("Unhandled type of Injection Container introduced - not supporting parenting.");

			if (injectionContainer.Parent == this)
				injectionContainer.Parent = null;
			m_Children.Remove(child);
			
			m_ChildrenCache = null;
		}

		public IInjectionContainer[] Children
		{
			get
			{
				if (m_Children == null || m_Children.Count == 0)
					return Array.Empty<IInjectionContainer>();
				if (m_ChildrenCache == null)
					m_ChildrenCache = m_Children.ToArray();
				return m_ChildrenCache;
			}
		}


		private bool m_Destroyed;
		private void OnDestroy()
		{
			if (m_Destroyed)
				return;
			m_Destroyed = true;
			
			Clean();

			if (m_Children != null)
			{
				foreach (var child in m_Children)
				{
					child.Destroy();
				}
			}
			m_Children = null;
			m_ChildrenCache = null;

			if (Parent != null)
			{
				Parent.RemoveChild(this);
				Parent = null;
			}
			
#if UNITY_EDITOR
			OnMappingAdded = null;
			OnMappingRemoved = null;
#endif
			m_ContextMap = null;
			m_instanceMap = null;
			
			var callback = OnContainerDestroy;
			OnContainerDestroy = null;
			callback?.Invoke(this);
		}
	}


	
	
}