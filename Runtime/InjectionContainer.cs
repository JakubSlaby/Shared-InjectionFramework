using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WhiteSparrow.Shared.DependencyInjection
{
	public class InjectionContainer : MonoBehaviour
	{
		private HashSet<Object> m_ManagedInstances = new HashSet<Object>();
		[SerializeField]
		#if ODIN_INSPECTOR
		[ReadOnly]
		#endif
		private HashSet<Object> m_UnityInstances = new HashSet<Object>();
		
		private Dictionary<Type, object> m_instanceMap = new Dictionary<Type, object>();
		private Dictionary<object, uint> m_instanceCount = new Dictionary<object, uint>();

#if UNITY_EDITOR
		internal event Action<Type, object> OnMappingAdded;
		internal event Action<Type, object> OnMappingRemoved;
#endif

		internal Dictionary<Type, object> Mapping
		{
			get => m_instanceMap;
		}
		
		private void Awake()
		{
			DontDestroyOnLoad(this.gameObject);
		}

		public T Map<T>() where T : Object
		{
			Type t = typeof(T);
			T instance = null;
			if (typeof(MonoBehaviour).IsAssignableFrom(t))
			{
				GameObject instanceObject = new GameObject();
				instance = instanceObject.AddComponent(t) as T;
				if (instance == null)
				{
					GameObject.Destroy(instanceObject);
					return null;
				}
				instanceObject.transform.SetParent(this.transform);
			}
			else if (typeof(ScriptableObject).IsAssignableFrom(t))
			{
				var scriptableObject = ScriptableObject.CreateInstance(t);
				instance = scriptableObject as T;
				if (instance == null)
				{
					ScriptableObject.Destroy(scriptableObject);
					return null;
				}
				instance.hideFlags = HideFlags.DontSave;
			}
			
			m_ManagedInstances.Add(instance);
			
			#if UNITY_EDITOR
			OnMappingAdded?.Invoke(t, instance);
			#endif
			
			return Map<T>(instance);
		}

		
		
		public T Map<T>(T instance)
		{
			return (T) Map(typeof(T), instance);
		}

		public object Map(Type type, object instance)
		{
			if (instance == null)
			{
				throw new ArgumentNullException(nameof(instance), $"Tried to map a null instance for Type={type.FullName}.");
			}
			
			if (m_instanceMap.ContainsKey(type))
			{
				throw new InjectionMappingAlreadyExistsException(this, type, "Unable to add new Mapping.");
			}

			m_instanceMap.Add(type, instance);

			if (m_instanceCount.TryGetValue(instance, out uint count))
			{
				m_instanceCount[instance] = count + 1;
			}
			else
			{
				m_instanceCount.Add(instance, 1);
			}
			
			if (instance is Object instanceAsObject)
				m_UnityInstances.Add(instanceAsObject);

#if UNITY_EDITOR
			OnMappingAdded?.Invoke(type, instance);
#endif
			
			return instance;
		}


		public void Unmap<T>()
		{
			Unmap(typeof(T));
		}

		public void Unmap(Type type)
		{
			if (m_instanceMap.TryGetValue(type, out var instance) && instance != null)
			{
				m_instanceMap.Remove(type);
				if (m_instanceCount[instance] <= 1)
					m_instanceCount.Remove(instance);
				else
					m_instanceCount[instance] = m_instanceCount[instance] - 1;

				if (instance is Object instanceAsObject && m_ManagedInstances.Remove(instanceAsObject))
				{
					Object.Destroy(instanceAsObject);
				}

				
#if UNITY_EDITOR
				OnMappingRemoved?.Invoke(type, instance);
#endif
				
				return;
			}
			
			if (m_instanceMap.ContainsKey(type))
			{
#if UNITY_EDITOR
				OnMappingRemoved?.Invoke(type, null);
#endif
				m_instanceMap.Remove(type);
			}
			else
			{
				throw new InjectionMissingMappingException(this, type, "Unable to Unmap type.");
			}
		}

		public bool HasMapping<T>()
		{
			return HasMapping(typeof(T));
		}
		
		public bool HasMapping(Type type)
		{
			return m_instanceMap.ContainsKey(type);
		}

		public T GetMapping<T>()
		{
			return (T) GetMapping(typeof(T));
		}

		public object GetMapping(Type type)
		{
			if (m_instanceMap.TryGetValue(type, out var instance))
				return instance;

			return null;
		}


		public void Clean()
		{
			m_instanceCount.Clear();
			m_instanceMap.Clear();

			foreach (var managedInstance in m_ManagedInstances)
			{
				if (managedInstance is MonoBehaviour instanceAsBehaviour)
				{
					GameObject.Destroy(instanceAsBehaviour.gameObject);
				}
				else
				{
					Object.Destroy(managedInstance);
				}
			}
			
			m_ManagedInstances.Clear();
			m_UnityInstances.Clear();
		}

		private void OnDestroy()
		{
			Clean();
		}
		
	}

	
	
}