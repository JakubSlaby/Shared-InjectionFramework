using UnityEngine;
using Object = UnityEngine.Object;

namespace WhiteSparrow.Shared.DependencyInjection.Containers
{
	public interface IInstanceBinding
	{
		object Instance { get; }
	}
	public interface IInstanceBinding<T> : IInstanceBinding
	{
		new T Instance { get; }
	}

	internal interface IInternalInstanceBinding : IInstanceBinding
	{
		void SetInstance(object instance);
	}

	public class InstanceBinding<T> : IInstanceBinding<T>, IInternalInstanceBinding
	{
		private T m_Instance;

		object IInstanceBinding.Instance => m_Instance;
		
		void IInternalInstanceBinding.SetInstance(object instance)
		{
			this.SetInstance((T)instance);
		}

		public T Instance => m_Instance;

		internal void SetInstance(T instance)
		{
			m_Instance = instance;
		}
	}

	public interface IManagedInstanceBinding
	{
		bool IsInstanced { get; }
		void Instantiate();
		void Dispose();
	}

	public abstract class AbstractUnityBinding<T> : InstanceBinding<T>, IManagedInstanceBinding
		where T : Object
	{
		private bool m_IsInstanced;
		private bool m_Disposed;
		bool IManagedInstanceBinding.IsInstanced => m_IsInstanced;

		void IManagedInstanceBinding.Instantiate() => this._Instantiate();

		private void _Instantiate()
		{
			if (m_IsInstanced)
				return;

			if (Instance != null)
			{
				throw new BindingAlreadyHasInstanceException(this, "Instance already mapped to binding.");
			}

			m_IsInstanced = true;
			Instantiate();
		}
		protected abstract void Instantiate();

		void IManagedInstanceBinding.Dispose() => this._Dispose();

		private void _Dispose()
		{
			if (m_Disposed)
				return;

			if (!m_IsInstanced)
			{
				throw new BindingDisposingUnmanagedInstanceException(this, "Instance is not managed through the Injection system. Can't dispose.");
			}

			m_IsInstanced = false;
			m_Disposed = true;
			Dispose();
		}
		protected abstract void Dispose();
	}
	
	public class MonoBehaviourInstanceBinding : AbstractUnityBinding<MonoBehaviour>
	{
		protected override void Instantiate()
		{
			// GameObject go = new GameObject($"[Injection] Instance Binding: {typeof(T).Name}");
			// T component = go.AddComponent<T>();
			//
			// SetInstance(component);
		}

		protected override void Dispose()
		{
			GameObject.Destroy(Instance);
		}
	}

	public class ScriptableObjectInstanceBinding : AbstractUnityBinding<ScriptableObject>
	{
		protected override void Instantiate()
		{
			// SetInstance(ScriptableObject.CreateInstance());
		}

		protected override void Dispose()
		{
			ScriptableObject.Destroy(Instance);
		}
	}

	public class GenericInstanceBinding : InstanceBinding<object>
	{
		
	}
}